using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
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

	internal class CallInfo
	{

	}

	internal interface IStackValue
	{

	}

	internal class LuaStackNumber(double _value) : IStackValue
	{
		public LuaNumber value;
	}

	internal class LuaStackInteger(long _value) : IStackValue
	{
		public LuaInteger value;
	}

	internal class LuaStackNil() : IStackValue { }

	internal class LuaStackClosure(LuaProto _proto) : IStackValue
	{
		public LuaProto proto = _proto;
	}

	internal class LuaStackString(byte[] _value) : IStackValue
	{
		public byte[] value = _value;
	}

	internal class LuaStackBool(bool _value) : IStackValue
	{
		public bool value = _value;
	}

	internal class State(CallInfo ci)
	{
		public int pc = 0;
		public List<CallInfo> callInfoList = new([ci]);
		public CallInfo currentCallInfo = ci;
		public List<IStackValue> stack = [];
		public int stackBase = 0;
		public LuaStackClosure? currentClosure;
	}

	public class LVM
	{
		public static void RunFile(LuaCFile file)
		{
			var state = new State(new CallInfo());
			state.stack.Add(new LuaStackClosure(file.topLevelFunction));
			Call(state, 0);
		}

		private static void Call(State state, int index)
		{
			var closure = (LuaStackClosure)state.stack[index];
			for (int i = state.stack.Count; i < index + 1 + closure.proto.maxStackSize; i++)
			{
				state.stack.Add(new LuaStackNil());
			}
			state.stackBase = index + 1;
			Step(state);
		}

		private static void Step(State state)
		{
			if (state.currentClosure == null)
			{
				throw new Exception("Expected currentClosure to be defined");
			}
			var instruction = state.currentClosure.proto.code[state.pc];
			var opcode = InstructionDecoder.GetOpcode(instruction);
			state = opcode switch {
				InstructionEnum.Move => LVMTransitions.Move(state, InstructionDecoder.DecodeIABC(instruction)),
				InstructionEnum.LoadI => LVMTransitions.LoadI(state, InstructionDecoder.DecodeIAsBx(instruction)),
				InstructionEnum.LoadF => LVMTransitions.LoadF(state, InstructionDecoder.DecodeIAsBx(instruction)),
				_ => throw new InvalidInstructionException(instruction, $"unrecognized opcode {opcode}")
			};
		}
	}

	internal class LVMTransitions
	{
		private static IStackValue ConstantToValue(ILuaConstant constant)
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

		public static State Move(State state, LuaIABCInstruction decoded)
		{
			state.stack[state.stackBase + decoded.A] = state.stack[state.stackBase + decoded.B];
			state.pc++;
			return state;
		}

		public static State LoadI(State state, LuaIAsBx decoded)
		{
			state.stack[state.stackBase + decoded.A] = new LuaStackInteger(decoded.sBx);
			state.pc++;
			return state;
		}

		public static State LoadF(State state, LuaIAsBx decoded)
		{
			state.stack[state.stackBase + decoded.A] = new LuaStackNumber(decoded.sBx);
			state.pc++;
			return state;
		}

		public static State LoadK(State state, LuaIABx decoded)
		{
			if (state.currentClosure == null)
			{
				throw new Exception("Expected currentClosure to be defined");
			}
			var constant = state.currentClosure.proto.constants[decoded.Bx];
			state.stack[state.stackBase + decoded.A] = ConstantToValue(constant);
			state.pc++;
			return state;
		}

		public static State LoadKX(State state, LuaIABCInstruction decoded)
		{
			if (state.currentClosure == null)
			{
				throw new Exception("Expected currentClosure to be defined");
			}
			var nextInstruction = state.currentClosure.proto.code[state.pc + 1];
			if (InstructionDecoder.GetOpcode(nextInstruction) != InstructionEnum.ExtraArg)
			{
				throw new InvalidInstructionException(nextInstruction, "Expected ExtraArg opcode to follow LoadK");
			}
			var decoded2 = InstructionDecoder.DecodeAx(nextInstruction);
			var constant = state.currentClosure.proto.constants[decoded2.Ax];
			state.stack[state.stackBase + decoded.A] = ConstantToValue(constant);
			state.pc += 2;
			return state;
		}

		public static State LoadFalse(State state, LuaIABCInstruction decoded)
		{
			state.stack[state.stackBase + decoded.A] = new LuaStackBool(false);
			state.pc++;
			return state;
		}

		public static State LFalseSkip(State state, LuaIABCInstruction decoded)
		{
			state.stack[state.stackBase + decoded.A] = new LuaStackBool(false);
			state.pc += 2;
			return state;
		}

		public static State LoadTrue(State state, LuaIABCInstruction decoded)
		{
			//state.stack[state.stackBase + ]
		}
	}
}
