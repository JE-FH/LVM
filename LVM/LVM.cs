using LVM.RuntimeType;
using System.Runtime.CompilerServices;
using System.Text;

namespace LVM
{
	public class InvalidInstructionException(LuaInstruction _instruction, string? message) : Exception(message)
	{
		public LuaInstruction instruction = _instruction;
	}

	public class InvalidInstructionParamException(LuaInstruction _instruction, string? message) : Exception(message)
	{
		public LuaInstruction instruction = _instruction;
	}

	public class ExpectedExtraArgException(string? message) : Exception(message)
	{

	}

	public class UnexpectedConstantType(string? message) : Exception(message)
	{

	}

	public class CompiledProto {
		public required byte[] source;
		public required int lineDefined;
		public required int lastLineDefined;
		public required byte numParams;
		public required bool isVarArg;
		public required byte maxStackSize;
		public required IStateTransition[] transitions;
		public required LuaCUpValue[] upValues;
		public required LuaCFunctionDebugInfo debugInfo;
	}

	public class LuaValueReference(IRuntimeValue _value)
	{
		public IRuntimeValue value = _value;
	}

	public class LuaStack()
	{
		public List<LuaValueReference> stack = new();
		public IRuntimeValue this[int index]
		{
			get { return stack[index].value; }
			set { stack[index].value = value; }
		}

		public LuaValueReference GetReference(int index)
		{
			return stack[index];
		}

		public void Push(IRuntimeValue val)
		{
			stack.Add(new LuaValueReference(val));
		}

		public void Pop()
		{
			stack.RemoveAt(stack.Count - 1);
		}

		public void PushNils(uint count)
		{
			for (uint i = 0; i < count; i++)
			{
				Push(new LuaNil());
			}
		}

		public int Length => stack.Count;
	}

	public class CallInfo(LuaState _luaState, LuaClosure _closure)
	{
		public int pc = 0;
		public LuaClosure closure = _closure;
		public int stackBase = 0;
		public LuaState luaState = _luaState;
		public IRuntimeValue this[int relativeIndex]
		{
			get => luaState.stack[stackBase + relativeIndex];
			set => luaState.stack[stackBase + relativeIndex] = value;
		}

		public LuaValueReference GetStackReference(int relativeIndex)
		{
			return luaState.stack.GetReference(stackBase + relativeIndex);
		}

		public T GetRegister<T>(int relativeIndex) where T : IRuntimeValue
		{
			var val = this[relativeIndex];
			if (val is T realValue)
			{
				return realValue;
			} else
			{
				throw new WrongRegisterTypeException(stackBase + relativeIndex, luaState.stack[stackBase + relativeIndex]);
			}
		}

		public double? CoerceToNumber(int relativeIndex)
		{
			return this[relativeIndex] switch
			{
				LuaNumber n => n.value,
				LuaInteger i => i.value,
				_ => null
			};
		}

		public long? CoerceToInteger(int relativeIndex)
		{
			return this[relativeIndex] switch
			{
				LuaNumber n => (long)Math.Floor(n.value),
				LuaInteger i => i.value,
				_ => null
			};
		}
	}

	public class LuaState
	{
		public List<CallInfo> callStack = [];
		public LuaStack stack = new();
		public LuaTable envTable = new();
		public LuaState()
		{
			envTable = new LuaTable();
		}

		public void RunFunction(LuaCFile luaCFile)
		{
			var closure = new LuaClosure(
				LuaTransitionCompiler.CompileProto(
					luaCFile.topLevelFunction
				),
				[new LuaValueReference(envTable)]
			);

			stack.Push(closure);
			Call(stack.Length - 1);
		}

		private void Call(int index)
		{
			var closure = (LuaClosure)stack[index];
			stack.PushNils(closure.proto.maxStackSize);
			var callInfo = new CallInfo(this, closure);
			callStack.Add(callInfo);
			while (Step()) { }
		}

		private bool Step()
		{
			if (callStack.Count == 0)
			{
				return false;
			}

			var callInfo = callStack[callStack.Count - 1];

			var nextTransition = callInfo.closure.proto.transitions[callInfo.pc];

			nextTransition.Execute(callInfo, this);

			return true;
		}
	}

	public class LuaTransitionCompiler
	{
		private static IRuntimeValue ConstantToValue(ILuaConstant constant)
		{
			return constant switch
			{
				LuaStringConstant s => new LuaString(s.data),
				LuaBoolConstant b => new LuaBool(b.value),
				LuaIntegerConstant i => new LuaInteger(i.value),
				LuaFloatConstant f => new LuaNumber(f.value),
				LuaNilConstant => new LuaNil(),
				_ => throw new Exception("Unknown constant value")
			};
		}

		private static T CheckedConstant<T>(ILuaConstant constant) where T : IRuntimeValue
		{
			var runtimeValue = ConstantToValue(constant);
			if (runtimeValue is T checkedValue)
			{
				return checkedValue;
			}
			else
			{
				throw new UnexpectedConstantType("Unexpected constant type");
			}
		}

		public static CompiledProto CompileProto(LuaProto proto)
		{
			var protos = proto.protos
				.Select(CompileProto)
				.ToArray();
			return new CompiledProto
			{
				source = proto.source,
				lineDefined = proto.lineDefined,
				lastLineDefined = proto.lastLineDefined,
				numParams = proto.numParams,
				isVarArg = proto.isVarArg,
				maxStackSize = proto.maxStackSize,
				transitions = 
					InstructionToTransitions(proto)
					.ToArray(),
				upValues = proto.upValues,
				debugInfo = proto.debugInfo
			};
		}


		private static IEnumerable<IStateTransition> InstructionToTransitions(LuaProto proto)
		{
			var enumerator = proto.code.AsEnumerable().GetEnumerator();
			while (enumerator.MoveNext())
			{
				var ins = enumerator.Current;
				yield return ins.OpCode switch
				{
					InstructionEnum.Move => new TrMove(ins.A, ins.B),
					InstructionEnum.LoadI => new TrLoadI(ins.A, ins.SBx),
					InstructionEnum.LoadF => new TrLoadF(ins.A, ins.SBx),
					InstructionEnum.LoadK => new TrLoadK(
						ins.A,
						(LuaString) ConstantToValue(proto.constants[ins.Bx])
					),
					InstructionEnum.LoadKX => new TrLoadKx(
						ins.A,
						ConstantToValue(proto.constants[GetExtraArg(enumerator)])
					),
					InstructionEnum.LoadFalse => new TrLoadFalse(ins.A),
					InstructionEnum.LFalseSkip => new TrLFalseSkip(ins.A),
					InstructionEnum.LoadTrue => new TrLoadTrue(ins.A),
					InstructionEnum.LoadNil => new TrLoadNil(ins.A, ins.B),
					InstructionEnum.GetUpVal => new TrGetUpVal(ins.A, ins.B),
					InstructionEnum.SetUpVal => new TrSetUpVal(ins.A, ins.B),
					InstructionEnum.GetTabUp => new TrGetTabUp(ins.A, ins.B, CheckedConstant<LuaString>(proto.constants[ins.C]).value),
					InstructionEnum.GetTable => new TrGetTable(ins.A, ins.B, ins.C),
					InstructionEnum.GetI => new TrGetI(ins.A, ins.B, ins.C),
					InstructionEnum.GetField => new TrGetField(
						ins.A,
						ins.B,
						CheckedConstant<LuaString>(proto.constants[GetExtraArg(enumerator)]).value
					),
					InstructionEnum.SetTabUp => ins.K 
						? new TrSetTabUpK(
							ins.A,
							CheckedConstant<LuaString>(proto.constants[ins.B]).value,
							ConstantToValue(proto.constants[ins.C])
						)
						: new TrSetTabUpR(
							ins.A,
							CheckedConstant<LuaString>(proto.constants[ins.B]).value,
							ins.C
						),
					InstructionEnum.SetTable => ins.K switch
					{
						true => new TrSetTableK(
							ins.A,
							ins.B,
							ConstantToValue(proto.constants[ins.C])
						),
						false => new TrSetTableC(ins.A, ins.B, ins.C)
					},
					InstructionEnum.SetI => ins.K switch
					{
						true => new TrSetIK(
							ins.A,
							ins.B,
							ConstantToValue(proto.constants[ins.C])
						),
						false => new TrSetIR(ins.A, ins.B, ins.C)
					},
					InstructionEnum.SetField => ins.K switch
					{
						true => new TrSetFieldK(
							ins.A,
							CheckedConstant<LuaString>(proto.constants[ins.B]).value,
							ConstantToValue(proto.constants[ins.C])
						),
						false => new TrSetFieldR(
							ins.A,
							CheckedConstant<LuaString>(proto.constants[ins.B]).value,
							ins.C
						)
					},
					InstructionEnum.NewTable => new TrNewTable(ins.A),
					InstructionEnum.Self => ins.K switch
					{
						true => new TrSelfK(
							ins.A,
							ins.B,
							CheckedConstant<LuaString>(proto.constants[ins.C]).value
						),
						false => new TrSelfR(ins.A, ins.B, ins.C)
					},
					InstructionEnum.AddI => new TrAddI(ins.A, ins.B, unchecked((sbyte)ins.C)),
					InstructionEnum.AddK => new TrAddK(
						ins.A,
						ins.B,
						CheckedConstant<LuaNumber>(proto.constants[ins.C]).value
					),
					InstructionEnum.SubK => new TrSubK(
						ins.A,
						ins.B,
						CheckedConstant<LuaNumber>(proto.constants[ins.C]).value
					),
					InstructionEnum.MulK => new TrMulK(
						ins.A,
						ins.B,
						CheckedConstant<LuaNumber>(proto.constants[ins.C]).value
					),
					InstructionEnum.ModK => new TrModK(
						ins.A,
						ins.B,
						CheckedConstant<LuaNumber>(proto.constants[ins.C]).value
					),
					InstructionEnum.PowK => new TrPowK(
						ins.A,
						ins.B,
						CheckedConstant<LuaNumber>(proto.constants[ins.C]).value
					),
					InstructionEnum.DivK => new TrDivK(
						ins.A,
						ins.B,
						CheckedConstant<LuaNumber>(proto.constants[ins.C]).value
					),
					InstructionEnum.IDivK => new TrIDivK(
						ins.A,
						ins.B,
						CheckedConstant<LuaNumber>(proto.constants[ins.C]).value
					),
					InstructionEnum.BAndK => new TrBAndK(
						ins.A,
						ins.B,
						CheckedConstant<LuaInteger>(proto.constants[ins.C]).value
					),
					InstructionEnum.BOrK => new TrBOrK(
						ins.A,
						ins.B,
						CheckedConstant<LuaInteger>(proto.constants[ins.C]).value
					),
					InstructionEnum.BXorK => new TrBXorK(
						ins.A,
						ins.B,
						CheckedConstant<LuaInteger>(proto.constants[ins.C]).value
					),
					InstructionEnum.ShrI => new TrShrI(ins.A, ins.B, ins.SC),
					InstructionEnum.ShlI => new TrShlI(ins.A, ins.B, ins.SC),
					InstructionEnum.Add => new TrAdd(ins.A, ins.B, ins.C),
					InstructionEnum.Sub => new TrSub(ins.A, ins.B, ins.C),
					InstructionEnum.Mul => new TrMul(ins.A, ins.B, ins.C),
					InstructionEnum.Mod => new TrMod(ins.A, ins.B, ins.C),
					InstructionEnum.Pow => new TrPow(ins.A, ins.B, ins.C),
					InstructionEnum.Div => new TrDiv(ins.A, ins.B, ins.C),
					InstructionEnum.IDiv => new TrIDiv(ins.A, ins.B, ins.C),
					InstructionEnum.BAnd => new TrBAnd(ins.A, ins.B, ins.C),
					InstructionEnum.BOr => new TrBOr(ins.A, ins.B, ins.C),
					InstructionEnum.BXor => new TrBXor(ins.A, ins.B, ins.C),
					InstructionEnum.Shl => new TrShl(ins.A, ins.B, ins.C),
					InstructionEnum.Shr => new TrShr(ins.A, ins.B, ins.C),
					InstructionEnum.MMBIN => new TrMetaMethodBinary(ins.A, ins.B, ins.C),
					InstructionEnum.MMBINI => new TrMetaMethodBinaryI(ins.A, ins.SB, ins.C, ins.K),
					InstructionEnum.MMBINK => new TrMetaMethodBinaryK(ins.A, ins.B, ins.C, ins.K),
					InstructionEnum.UNM => new TrUnaryMinus(ins.A, ins.B),
					InstructionEnum.BNot => new TrBNot(ins.A, ins.B),
					InstructionEnum.Not => new TrNot(ins.A, ins.B),
					InstructionEnum.Len => new TrLen(ins.A, ins.B),
					InstructionEnum.Concat => new TrConcat(ins.A, ins.B),
					InstructionEnum.Close => new TrClose(ins.A),
					InstructionEnum.TBC => new TrTBC(ins.A),
					InstructionEnum.Jmp => new TrJmp(ins.SJ),
					InstructionEnum.Closure => new TrClosure(
						ins.A,
						CompileProto(proto.protos[ins.Bx])
					),
					InstructionEnum.Call => new TrCall(ins.A, ins.B, ins.C),
					InstructionEnum.VarArgPrep => new TrNOP(),
					InstructionEnum.Return => new TrReturn(ins.A, ins.B, ins.C, ins.K)
				};
			}
		}

		private static uint GetExtraArg(IEnumerator<LuaInstruction> iterator)
		{
			if (!iterator.MoveNext())
			{
				throw new ExpectedExtraArgException("Expected extra arg after LoadKx");
			}
			if (iterator.Current.OpCode != InstructionEnum.ExtraArg)
			{
				throw new ExpectedExtraArgException("Expected extra arg after LoadKx");
			}
			return iterator.Current.Ax;
		}
	}
}
