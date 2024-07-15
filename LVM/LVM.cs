using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace LVM
{
	using LuaNumber = double;
	using LuaInteger = long;

	public class InvalidInstructionException(LuaInstruction _instruction, string? message) : Exception(message)
	{
		public LuaInstruction instruction = _instruction;
	}

	internal class CallInfo(LuaStackClosure closure)
	{
		public int pc = 0;
		public LuaStackClosure closure;
		public int stackBase = 0;
	}

	internal interface IRuntimeValue
	{

	}

	internal class LuaStackNumber(double _value) : IRuntimeValue
	{
		public LuaNumber value;
	}

	internal class LuaStackInteger(long _value) : IRuntimeValue
	{
		public LuaInteger value;
	}

	internal class LuaStackNil() : IRuntimeValue { }

	internal class LuaStackClosure(LuaProto _proto) : IRuntimeValue
	{
		public LuaProto proto = _proto;
	}

	internal class LuaStackString(byte[] _value) : IRuntimeValue
	{
		public byte[] value = _value;
	}

	internal class LuaStackBool(bool _value) : IRuntimeValue
	{
		public bool value = _value;
	}

	internal class LuaStackTable() : IRuntimeValue
	{
		public IRuntimeValue GetValue(IRuntimeValue index)
		{
			throw new NotImplementedException();
		}

		public IRuntimeValue GetValue(int index)
		{
			throw new NotImplementedException();
		}

		public IRuntimeValue GetValue(byte[] index)
		{
			throw new NotImplementedException();
		}

		public void SetValue(IRuntimeValue index, IRuntimeValue value)
		{
			throw new NotImplementedException();
		}

		public void SetValue(int index, IRuntimeValue value)
		{
			throw new NotImplementedException();
		}

		public void SetValue(byte[] index, IRuntimeValue value)
		{
			throw new NotImplementedException();
		}
	}

	internal class State()
	{
		public List<CallInfo> callStack = [];
		public List<IRuntimeValue> stack = [];
	}

	public class LVM
	{
		public static void RunFile(LuaCFile file)
		{
			var state = new State();
			var closure = new LuaStackClosure(file.topLevelFunction);
			state.stack.Add(closure);
			Call(state, 0);
		}

		private static void Call(State state, int index)
		{
			var closure = (LuaStackClosure)state.stack[index];
			for (int i = state.stack.Count; i < index + 1 + closure.proto.maxStackSize; i++)
			{
				state.stack.Add(new LuaStackNil());
			}
			var callInfo = new CallInfo(closure);
			state.callStack.Add(callInfo);
			while (Step(state)) { }
		}

		private static bool Step(State state)
		{
			if (state.callStack.Count == 0)
			{
				return false;
			}

			var callInfo = state.callStack[state.callStack.Count];

			var instruction = callInfo.closure.proto.code[callInfo.pc];
			var opcode = instruction.DecodeOpCode();
			switch (opcode) {
				case InstructionEnum.Move:
					LVMTransitions.Move(callInfo, state, instruction.DecodeA(), instruction.DecodeB());
					break;
				case InstructionEnum.LoadI:
					LVMTransitions.LoadI(callInfo, state, instruction.DecodeA(), instruction.DecodeSBx());
					break;
				case InstructionEnum.LoadF:
					LVMTransitions.LoadF(callInfo, state, instruction.DecodeA(), instruction.DecodeSBx());
					break;
				default:
					throw new InvalidInstructionException(instruction, $"unrecognized opcode {opcode}");
			}

			return true;
		}
	}

	internal class LVMTransitions
	{
		private static IRuntimeValue ConstantToValue(ILuaConstant constant)
		{
			return constant switch
			{
				LuaStringConstant s => new LuaStackString(s.data),
				LuaBoolConstant b => new LuaStackBool(b.value),
				LuaIntegerConstant i => new LuaStackInteger(i.value),
				LuaFloatConstant f => new LuaStackNumber(f.value),
				LuaNilConstant => new LuaStackNil(),
				_ => throw new Exception("Unknown constant value")
			};
		}

		public static void Move(CallInfo ci, State state, byte A, byte B)
		{
			state.stack[ci.stackBase + A] = state.stack[ci.stackBase + B];
			ci.pc++;
		}

		public static void LoadI(CallInfo ci, State state, byte A, int sBx)
		{
			state.stack[ci.stackBase + A] = new LuaStackInteger(sBx);
			ci.pc++;
		}

		public static void LoadF(CallInfo ci, State state, byte A, int sBx)
		{
			state.stack[ci.stackBase + A] = new LuaStackNumber(sBx);
			ci.pc++;
		}

		public static void LoadK(CallInfo ci, State state, byte A, uint Bx)
		{
			var constant = ci.closure.proto.constants[Bx];
			state.stack[ci.stackBase + A] = ConstantToValue(constant);
			ci.pc++;
		}

		public static void LoadKX(CallInfo ci, State state, byte A)
		{
			var nextInstruction = ci.closure.proto.code[ci.pc + 1];
			if (nextInstruction.DecodeOpCode() != InstructionEnum.ExtraArg)
			{
				throw new InvalidInstructionException(nextInstruction, "Expected ExtraArg opcode to follow LoadK");
			}
			var Ax = nextInstruction.DecodeAx();
			var constant = ci.closure.proto.constants[Ax];
			state.stack[ci.stackBase + A] = ConstantToValue(constant);
			ci.pc += 2;
		}

		public static void LoadFalse(CallInfo ci, State state, byte A)
		{
			state.stack[ci.stackBase + A] = new LuaStackBool(false);
			ci.pc++;
		}

		public static void LFalseSkip(CallInfo ci, State state, byte A)
		{
			state.stack[ci.stackBase + A] = new LuaStackBool(false);
			ci.pc += 2;
		}

		public static void LoadTrue(CallInfo ci, State state, byte A)
		{
			state.stack[ci.stackBase + A] = new LuaStackBool(true);
			ci.pc++;
		}

		public static void LoadNil(CallInfo ci, State state, byte A, byte B)
		{
			for (int i = A; i < A + B; ++i) {
				state.stack[ci.stackBase + i] = new LuaStackNil();
			}
			ci.pc++;
		}

		public static void GetUpVal(CallInfo ci, State state, byte A, byte B)
		{
			throw new NotImplementedException();
		}

		public static void SetUpVal(CallInfo ci, State state, byte A, byte B)
		{
			throw new NotImplementedException();
		}

		public static void GetTabUp(CallInfo ci, State state, byte A, byte B, byte C)
		{
			throw new NotImplementedException();
		}

		public static void GetTable(CallInfo ci, State state, byte A, byte B, byte C)
		{
			LuaStackTable table = (LuaStackTable) state.stack[ci.stackBase + B];
			state.stack[ci.stackBase + A] = table.GetValue(state.stack[ci.stackBase + C]);
			ci.pc += 1;
		}

		public static void GetI(CallInfo ci, State state, byte A, byte B, byte C)
		{
			LuaStackTable table = (LuaStackTable)state.stack[ci.stackBase + B];
			state.stack[ci.stackBase + A] = table.GetValue(C);
			ci.pc += 1;
		}

		public static void GetField(CallInfo ci, State state, byte A, byte B, byte C)
		{
			//TODO: check if short string
			LuaStackTable table = (LuaStackTable)state.stack[ci.stackBase + B];
			var constant = (LuaStringConstant) ci.closure.proto.constants[C];
			state.stack[ci.stackBase + A] = table.GetValue(constant.data);
			ci.pc++;
		}

		public static void SetTabUp(CallInfo ci, State state, byte A, byte B, byte C)
		{
			throw new NotImplementedException();
		}

		public static void SetTable(CallInfo ci, State state, byte A, bool k, byte B, byte C)
		{
			LuaStackTable table = (LuaStackTable)state.stack[ci.stackBase + A];
			var val = k switch
			{
				true => ConstantToValue(ci.closure.proto.constants[C]),
				false => state.stack[ci.stackBase + C]
			};
			table.SetValue(state.stack[ci.stackBase + B], val);
			ci.pc++;
		}

		public static void SetI(CallInfo ci, State state, byte A, bool k, byte B, byte C)
		{
			LuaStackTable table = (LuaStackTable)state.stack[ci.stackBase + A];
			var val = k switch
			{
				true => ConstantToValue(ci.closure.proto.constants[C]),
				false => state.stack[ci.stackBase + C]
			};
			table.SetValue(C, val);
			ci.pc += 1;
		}

		public static void SetField(CallInfo ci, State state, byte A, bool k, byte B, byte C)
		{
			LuaStackTable table = (LuaStackTable)state.stack[ci.stackBase + A];
			var val = k switch
			{
				true => ConstantToValue(ci.closure.proto.constants[C]),
				false => state.stack[ci.stackBase + C]
			};
			var constant = (LuaStringConstant)ci.closure.proto.constants[B];
			table.SetValue(constant.data, val);
		}

		public static void NewTable(CallInfo ci, State state, byte A)
		{
			//TODO: Implement tables that exploit extra information from
			//this opcode
			state.stack[ci.stackBase + A] = new LuaStackTable();
			//we skip the extra arg
			ci.pc += 2;
		}
	}
}
