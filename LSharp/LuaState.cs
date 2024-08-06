﻿using LSharp.LTypes;
using LSharp.Transitions;
using LSharp.Transitions.CallStack;
using LSharp.Transitions.For;
using LSharp.Transitions.MetaMethod;
using LSharp.Transitions.Stack;
using LSharp.Transitions.Table;
using LuaByteCode.LuaCConstructs;
using LuaByteCode.LuaCConstructs.Types;
using System.Text;

namespace LSharp
{
	public class LuaState()
	{
		public LStack Stack { get; } = new();
		public List<IStackFrame> CallStack { get; } = [];
		public LTable EnvironmentTable { get; } = new();

		public LClosure LoadFile(LuaCFile luaCFile, ILValue[] args)
		{
			return LuaFileToTopClosure(luaCFile, args);
		}

		public void TopLevelCall(LClosure closure, ILValue[] args)
		{
			CallAt(Stack.Top, closure, -1, args);
		}

		public void TopLevelCall(CSClosure closure, ILValue[] args)
		{
			CallAt(Stack.Top, closure, -1, args);
		}

		internal void CallAt(int frameBase, LClosure closure, int returnCount, ILValue[] args)
		{
			Stack.SetTop(frameBase + closure.MinStackSize);
			for (int i = 0; i < closure.ParamCount; i++)
			{
				Stack[frameBase + i] = i < args.Length ? args[i] : LNil.Instance;
			}

			var extraArgs = args.Skip(closure.ParamCount).ToArray();

			CallStack.Add(new LStackFrame(frameBase, Stack.Top, closure, returnCount, extraArgs));
		}

		internal void CallAt(int frameBase, CSClosure closure, int returnCount, ILValue[] args)
		{
			Stack.SetTop(frameBase + closure.MinStackSize);
			CallStack.Add(new CSStackFrame(this, Stack.Top, closure, returnCount, args));
		}

		internal void CallAt(int frameBase, IClosure closure, int returnCount, ILValue[] args)
		{
			if (closure is LClosure lClosure)
			{
				CallAt(frameBase, lClosure, returnCount, args);
			} 
			else if (closure is CSClosure csClosure)
			{
				CallAt(frameBase, csClosure, returnCount, args);
			}
			else
			{
				throw new NotImplementedException();
			}
		}

		public bool Step()
		{
			var frame = CallStack[^1];
			if (frame is CSStackFrame csStackFrame)
			{
				HandleCsStackFrame(csStackFrame);
			}
			else if (frame is LStackFrame lStackFrame)
			{
				var transition = lStackFrame.Closure.Prototype.Transitions[lStackFrame.PC];
				transition.Transfer(this, lStackFrame);
			}
			else
			{
				throw new NotImplementedException();
			}

			return CallStack.Count == 0;
		}

		private LClosure LuaFileToTopClosure(LuaCFile luaCFile, ILValue[] args)
		{
			return new LClosure(
				ToPrototype(luaCFile.TopLevelFunction),
				[new UpValue(EnvironmentTable)]
			);
		}

		private LPrototype ToPrototype(LuaCPrototype prototype)
		{
			return new LPrototype(
				ToTransitions(prototype).ToArray(),
				prototype.NumParams,
				prototype.MaxStackSize,
				prototype.UpValues
			);
		}

		private ILValue ConstantToValue(ILuaConstant constant)
		{
			return constant switch
			{
				BoolConstant c => c.Value ? LBool.TrueInstance : LBool.FalseInstance,
				FloatConstant f => new LNumber(f.Value),
				IntegerConstant i => new LInteger(i.Value),
				NilConstant => LNil.Instance,
				StringConstant s => new LString(Encoding.UTF8.GetString(s.Data)),
				_ => throw new NotImplementedException()
			};
		}

		private T ConstantToSpecific<T>(ILuaConstant constant) where T : ILValue
		{
			return (T)ConstantToValue(constant);
		}

		private IEnumerable<ITransition> ToTransitions(LuaCPrototype prototype)
		{
			var instructions = prototype.Code;
			for (int i = 0; i < instructions.Length; i++)
			{
				var ins = instructions[i];
				yield return ins.OpCode switch
				{
					InstructionEnum.Move => new OMove(ins.A, ins.B),
					InstructionEnum.LoadI => new OLoadConstant(ins.A, new LInteger(ins.SBx)),
					InstructionEnum.LoadF => new OLoadConstant(ins.A, new LNumber(ins.SBx)),
					InstructionEnum.LoadK => new OLoadConstant(ins.A, ConstantToValue(prototype.Constants[ins.Bx])),
					InstructionEnum.LoadKX => new OLoadConstant(ins.A, ConstantToValue(prototype.Constants[instructions[i + 1].Ax])),
					InstructionEnum.LoadFalse => new OLoadConstant(ins.A, LBool.FalseInstance),
					InstructionEnum.LoadTrue => new OLoadConstant(ins.A, LBool.TrueInstance),
					InstructionEnum.LoadNil => new OLoadConstant(ins.A, LNil.Instance),
					
					InstructionEnum.LFalseSkip => new OLoadFalseSkip(ins.A),
					
					InstructionEnum.GetTable => new OGetTable(ins.A, ins.B, ins.C),
					InstructionEnum.GetTabUp => new OGetTabUp(ins.A, ins.B, ConstantToSpecific<LString>(prototype.Constants[ins.C]).Value),
					InstructionEnum.GetI => new OGetI(ins.A, ins.B, ins.C),
					InstructionEnum.GetField => new OGetField(ins.A, ins.B,  ConstantToSpecific<LString>(prototype.Constants[ins.C]).Value),
					
					InstructionEnum.SetTable => ins.K
						? new OSetTableK(ins.A, ins.B, ConstantToValue(prototype.Constants[ins.C]))
						: new OSetTableR(ins.A, ins.B, ins.C),
					InstructionEnum.SetI => ins.K
						? new OSetIK(ins.A, ins.B, ConstantToValue(prototype.Constants[ins.C]))
						: new OSetIR(ins.A, ins.B, ins.C),
					InstructionEnum.SetTabUp => ins.K
						? new OSetTabUpK(
							ins.A,
							ConstantToSpecific<LString>(prototype.Constants[ins.B]).Value, 
							ConstantToValue(prototype.Constants[ins.C])
						)
						: new OSetTabUpR(ins.A, ConstantToSpecific<LString>(prototype.Constants[ins.B]).Value, ins.C),

					InstructionEnum.MMBIN => new OMMBin(ins.A, ins.B, (MetaMethodTag)ins.C, instructions[i - 1].A),
					InstructionEnum.MMBINI => ins.K
						? new OMMBinKk(ins.A, new LInteger(ins.SB), (MetaMethodTag)ins.C, instructions[i - 1].A)
						: new OMMBinK(ins.A, new LInteger(ins.SB), (MetaMethodTag)ins.C, instructions[i - 1].A),
					InstructionEnum.MMBINK => ins.K
						? new OMMBinKk(ins.A, ConstantToValue(prototype.Constants[ins.B]), (MetaMethodTag)ins.C, instructions[i - 1].A)
						: new OMMBinK(ins.A, ConstantToValue(prototype.Constants[ins.B]), (MetaMethodTag)ins.C, instructions[i - 1].A),

					InstructionEnum.Call => new OCall(ins.A, ins.B, ins.C),
					InstructionEnum.TailCall => new OTailCall(ins.A, ins.B),
					
					InstructionEnum.Return => new OReturn(ins.A, ins.B),
					InstructionEnum.Return0 => new OReturn0(),
					InstructionEnum.Return1 => new OReturn1(ins.A),
					
					InstructionEnum.Closure => new OClosure(ins.A, ToPrototype(prototype.Prototypes[ins.Bx])),
					
					InstructionEnum.ForPrep => new OForPrep(ins.A, ins.Bx),
					InstructionEnum.ForLoop => new OForLoop(ins.A, ins.Bx),
					
					InstructionEnum.TForPrep => new OTForPrep(ins.A, ins.Bx),
					InstructionEnum.TForCall => new OTForCall(ins.A, ins.C),
					InstructionEnum.TForLoop => new OTForLoop(ins.A, ins.Bx),
					
					InstructionEnum.VarArgPrep => new ONop(),
					InstructionEnum.VarArg => new OVarArg(ins.A, ins.C),
					
					InstructionEnum.SetList => new OSetList(ins.A, ins.B, ins.C, ins.K),
					
					InstructionEnum.Close => new ONop(), // Close is not supported yet

					InstructionEnum.ExtraArg => new OExtraArg(),
					_ => throw new NotImplementedException(),
				};
			}
		}

		private void HandleCsStackFrame(CSStackFrame csStackFrame)
		{
			if (csStackFrame.ActionGenerator.MoveNext())
			{
				var callYield = csStackFrame.ActionGenerator.Current;
				CallAt(csStackFrame.FrameTop, callYield.Closure, -1, callYield.Parameters);
				return;
			}

			CallStack.RemoveAt(CallStack.Count - 1);
			if (CallStack.Count == 0)
			{
				//Optionally return the values somehow
				return;
			}
			var lowerStackFrame = CallStack[^1];
			if (lowerStackFrame is LStackFrame lowerLStackFrame)
			{
				if (csStackFrame.MandatoryReturnCount == -1)
				{
					var newTop = Math.Max(csStackFrame.FrameBase + csStackFrame.MandatoryReturnCount, lowerStackFrame.FrameTop);
					lowerLStackFrame.FrameTop = newTop;
					Stack.SetTop(Math.Max(csStackFrame.FrameBase + csStackFrame.MandatoryReturnCount, lowerStackFrame.FrameTop));
					for (int i = 0; i < csStackFrame.ReturnValues.Length; i++)
					{
						Stack[csStackFrame.FrameBase + i] = csStackFrame.ReturnValues[i];
					}
				}
				else
				{
					for (int i = 0; i < csStackFrame.MandatoryReturnCount; i++)
					{
						Stack[csStackFrame.FrameBase + i] = i < csStackFrame.ReturnValues.Length 
							? csStackFrame.ReturnValues[i]
							: LNil.Instance;
					}
					Stack.SetTop(lowerStackFrame.FrameTop);
				}
			}
			else if (lowerStackFrame is CSStackFrame lowerCSStackFrame)
			{
				lowerCSStackFrame.ActionGenerator.Current.Return = csStackFrame.ReturnValues;
				Stack.SetTop(lowerStackFrame.FrameTop);
			}
			else
			{
				throw new NotImplementedException();
			}
		}
	}
}
