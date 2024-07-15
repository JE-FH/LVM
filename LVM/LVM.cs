namespace LVM
{
	using LuaInteger = long;
	using LuaNumber = double;

	public class InvalidInstructionException(LuaInstruction _instruction, string? message) : Exception(message)
	{
		public LuaInstruction instruction = _instruction;
	}

	public class InvalidInstructionParam(LuaInstruction _instruction, string? message) : Exception(message)
	{
		public LuaInstruction instruction = _instruction;
	}

	public class CallInfo(LuaStackClosure closure)
	{
		public int pc = 0;
		public LuaStackClosure closure;
		public int stackBase = 0;
	}

	public interface IRuntimeValue
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

	public class LuaStackString(byte[] _value) : IRuntimeValue
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

	public class LuaState()
	{
		public List<CallInfo> callStack = [];
		public List<IRuntimeValue> stack = [];
	}

	public class LVM
	{
		public static void RunFile(LuaCFile file)
		{
			var state = new LuaState();
			var closure = new LuaStackClosure(file.topLevelFunction);
			state.stack.Add(closure);
			Call(state, 0);
		}

		private static void Call(LuaState luaState, int index)
		{
			var closure = (LuaStackClosure)luaState.stack[index];
			for (int i = luaState.stack.Count; i < index + 1 + closure.proto.maxStackSize; i++)
			{
				luaState.stack.Add(new LuaStackNil());
			}
			var callInfo = new CallInfo(closure);
			luaState.callStack.Add(callInfo);
			while (Step(luaState)) { }
		}

		private static bool Step(LuaState luaState)
		{
			if (luaState.callStack.Count == 0)
			{
				return false;
			}

			var callInfo = luaState.callStack[luaState.callStack.Count];

			var instruction = callInfo.closure.proto.code[callInfo.pc];
			var opcode = instruction.DecodeOpCode();
			switch (opcode) {
				case InstructionEnum.Move:
					LVMTransitions.Move(callInfo, luaState, instruction.DecodeA(), instruction.DecodeB());
					break;
				case InstructionEnum.LoadI:
					LVMTransitions.LoadI(callInfo, luaState, instruction.DecodeA(), instruction.DecodeSBx());
					break;
				case InstructionEnum.LoadF:
					LVMTransitions.LoadF(callInfo, luaState, instruction.DecodeA(), instruction.DecodeSBx());
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

		public static void Move(CallInfo ci, LuaState luaState, byte A, byte B)
		{
			luaState.stack[ci.stackBase + A] = luaState.stack[ci.stackBase + B];
			ci.pc++;
		}

		public static void LoadI(CallInfo ci, LuaState luaState, byte A, int sBx)
		{
			luaState.stack[ci.stackBase + A] = new LuaStackInteger(sBx);
			ci.pc++;
		}

		public static void LoadF(CallInfo ci, LuaState luaState, byte A, int sBx)
		{
			luaState.stack[ci.stackBase + A] = new LuaStackNumber(sBx);
			ci.pc++;
		}

		public static void LoadK(CallInfo ci, LuaState luaState, byte A, uint Bx)
		{
			var constant = ci.closure.proto.constants[Bx];
			luaState.stack[ci.stackBase + A] = ConstantToValue(constant);
			ci.pc++;
		}

		public static void LoadKX(CallInfo ci, LuaState luaState, byte A)
		{
			var nextInstruction = ci.closure.proto.code[ci.pc + 1];
			if (nextInstruction.DecodeOpCode() != InstructionEnum.ExtraArg)
			{
				throw new InvalidInstructionException(nextInstruction, "Expected ExtraArg opcode to follow LoadK");
			}
			var Ax = nextInstruction.DecodeAx();
			var constant = ci.closure.proto.constants[Ax];
			luaState.stack[ci.stackBase + A] = ConstantToValue(constant);
			ci.pc += 2;
		}

		public static void LoadFalse(CallInfo ci, LuaState luaState, byte A)
		{
			luaState.stack[ci.stackBase + A] = new LuaStackBool(false);
			ci.pc++;
		}

		public static void LFalseSkip(CallInfo ci, LuaState luaState, byte A)
		{
			luaState.stack[ci.stackBase + A] = new LuaStackBool(false);
			ci.pc += 2;
		}

		public static void LoadTrue(CallInfo ci, LuaState luaState, byte A)
		{
			luaState.stack[ci.stackBase + A] = new LuaStackBool(true);
			ci.pc++;
		}

		public static void LoadNil(CallInfo ci, LuaState luaState, byte A, byte B)
		{
			for (int i = A; i < A + B; ++i) {
				luaState.stack[ci.stackBase + i] = new LuaStackNil();
			}
			ci.pc++;
		}

		public static void GetUpVal(CallInfo ci, LuaState luaState, byte A, byte B)
		{
			throw new NotImplementedException();
		}

		public static void SetUpVal(CallInfo ci, LuaState luaState, byte A, byte B)
		{
			throw new NotImplementedException();
		}

		public static void GetTabUp(CallInfo ci, LuaState luaState, byte A, byte B, byte C)
		{
			throw new NotImplementedException();
		}

		public static void GetTable(CallInfo ci, LuaState luaState, byte A, byte B, byte C)
		{
			LuaStackTable table = (LuaStackTable) luaState.stack[ci.stackBase + B];
			luaState.stack[ci.stackBase + A] = table.GetValue(luaState.stack[ci.stackBase + C]);
			ci.pc += 1;
		}

		public static void GetI(CallInfo ci, LuaState luaState, byte A, byte B, byte C)
		{
			LuaStackTable table = (LuaStackTable)luaState.stack[ci.stackBase + B];
			luaState.stack[ci.stackBase + A] = table.GetValue(C);
			ci.pc += 1;
		}

		public static void GetField(CallInfo ci, LuaState luaState, byte A, byte B, byte C)
		{
			//TODO: check if short string
			LuaStackTable table = (LuaStackTable)luaState.stack[ci.stackBase + B];
			var constant = (LuaStringConstant) ci.closure.proto.constants[C];
			luaState.stack[ci.stackBase + A] = table.GetValue(constant.data);
			ci.pc++;
		}

		public static void SetTabUp(CallInfo ci, LuaState luaState, byte A, byte B, byte C)
		{
			throw new NotImplementedException();
		}

		public static void SetTable(CallInfo ci, LuaState luaState, byte A, bool k, byte B, byte C)
		{
			LuaStackTable table = (LuaStackTable)luaState.stack[ci.stackBase + A];
			var val = k switch
			{
				true => ConstantToValue(ci.closure.proto.constants[C]),
				false => luaState.stack[ci.stackBase + C]
			};
			table.SetValue(luaState.stack[ci.stackBase + B], val);
			ci.pc++;
		}

		public static void SetI(CallInfo ci, LuaState luaState, byte A, bool k, byte B, byte C)
		{
			LuaStackTable table = (LuaStackTable)luaState.stack[ci.stackBase + A];
			var val = k switch
			{
				true => ConstantToValue(ci.closure.proto.constants[C]),
				false => luaState.stack[ci.stackBase + C]
			};
			table.SetValue(C, val);
			ci.pc += 1;
		}

		public static void SetField(CallInfo ci, LuaState luaState, byte A, bool k, byte B, byte C)
		{
			LuaStackTable table = (LuaStackTable)luaState.stack[ci.stackBase + A];
			var val = k switch
			{
				true => ConstantToValue(ci.closure.proto.constants[C]),
				false => luaState.stack[ci.stackBase + C]
			};
			var constant = (LuaStringConstant)ci.closure.proto.constants[B];
			table.SetValue(constant.data, val);
		}

		public static void NewTable(CallInfo ci, LuaState luaState, byte A)
		{
			//TODO: Implement tables that exploit extra information from
			//this opcode
			luaState.stack[ci.stackBase + A] = new LuaStackTable();
			//we skip the extra arg
			ci.pc += 2;
		}

		public static void Self(CallInfo ci, LuaState luaState, byte A, bool k, byte B, byte C)
		{
			LuaStackTable table = (LuaStackTable)luaState.stack[ci.stackBase + B];

			luaState.stack[ci.stackBase + A + 1] = table;
			var val = (LuaStackString) (k switch
			{
				true => ConstantToValue(ci.closure.proto.constants[C]),
				false => luaState.stack[ci.stackBase + C]
			});
			luaState.stack[ci.stackBase + A] = table.GetValue(val.value);
		}

		public static void AddI(CallInfo ci, LuaState luaState, byte A, byte B, sbyte sC)
		{
			IRuntimeValue res = luaState.stack[ci.stackBase + B] switch
			{
				LuaStackNumber n => new LuaStackNumber(n.value + sC),
				LuaStackInteger i => new LuaStackInteger(i.value + sC),
				_ => throw new InvalidInstructionParam(ci.closure.proto.code[ci.pc], $"Stack value B ({B}) should be a number")
			};
			luaState.stack[ci.stackBase + A] = res;
		}

		public static void AddK(CallInfo ci, LuaState luaState, byte A, byte B, byte C)
		{

		}
	}
}
