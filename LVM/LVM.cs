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

	public interface IRuntimeValue
	{
		public string TypeName { get; }
	}

	internal class LuaNumber(double _value) : IRuntimeValue
	{
		public double value = _value;
		public string TypeName => "number";
	}

	internal class LuaInteger(long _value) : IRuntimeValue
	{
		public long value = _value;
		public string TypeName => "integer";
	}

	internal class LuaNil() : IRuntimeValue {
		public string TypeName => "nil";
	}

	public class LuaClosure(LuaRuntimeProto _proto) : IRuntimeValue
	{
		public LuaRuntimeProto proto = _proto;
		public string TypeName => "closure";
	}

	public class LuaString(byte[] _value) : IRuntimeValue
	{
		public byte[] value = _value;
		public string TypeName => "string";
	}

	internal class LuaBool(bool _value) : IRuntimeValue
	{
		public bool value = _value;
		public string TypeName => "bool";
	}

	internal class LuaTable() : IRuntimeValue
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
		public string TypeName => "table";
	}

	public class LuaRuntimeProto {
		public required byte[] source;
		public required int lineDefined;
		public required int lastLineDefined;
		public required byte numParams;
		public required bool isVarArg;
		public required byte maxStackSize;
		public required IStateTransition[] transitions;
		public required LuaUpValue[] upValues;
		public required LuaRuntimeProto[] protos;
		public required LuaCFunctionDebugInfo debugInfo;
	}

	public class CallInfo(LuaClosure _closure)
	{
		public int pc = 0;
		public LuaClosure closure = _closure;
		public int stackBase = 0;
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
			var closure = new LuaClosure(MapProtoToRuntime(file.topLevelFunction));
			state.stack.Add(closure);
			Call(state, 0);
		}

		private static void Call(LuaState luaState, int index)
		{
			var closure = (LuaClosure)luaState.stack[index];
			for (int i = luaState.stack.Count; i < index + 1 + closure.proto.maxStackSize; i++)
			{
				luaState.stack.Add(new LuaNil());
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

			var nextTransition = callInfo.closure.proto.transitions[callInfo.pc];

			nextTransition.Execute(callInfo, luaState);

			return true;
		}

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

		private static T CheckedConstant<T>(ILuaConstant constant)
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

		private static LuaRuntimeProto MapProtoToRuntime(LuaProto proto)
		{
			return new LuaRuntimeProto
			{
				source = proto.source,
				lineDefined = proto.lineDefined,
				lastLineDefined = proto.lastLineDefined,
				numParams = proto.numParams,
				isVarArg = proto.isVarArg,
				maxStackSize = proto.maxStackSize,
				transitions = InstructionToTransitions(
					proto,
					proto.code
				).ToArray(),
				upValues = proto.upValues,
				protos = proto.protos
					.Select(MapProtoToRuntime)
					.ToArray(),
				debugInfo = proto.debugInfo
			};
		}


		private static IEnumerable<IStateTransition> InstructionToTransitions(LuaProto proto, IEnumerable<LuaInstruction> instruction)
		{
			var enumerator = instruction.GetEnumerator();
			while (enumerator.MoveNext())
			{
				var ins = enumerator.Current;
				yield return ins.DecodeOpCode() switch
				{
					InstructionEnum.Move => new TrMove(ins.DecodeA(), ins.DecodeB()),
					InstructionEnum.LoadI => new TrLoadI(ins.DecodeA(), ins.DecodeSBx()),
					InstructionEnum.LoadF => new TrLoadF(ins.DecodeA(), ins.DecodeSBx()),
					InstructionEnum.LoadK => new TrLoadK(
						ins.DecodeA(),
						(LuaString) ConstantToValue(proto.constants[ins.DecodeBx()])
					),
					InstructionEnum.LoadKX => new TrLoadKx(
						ins.DecodeA(),
						ConstantToValue(proto.constants[GetExtraArg(enumerator)])
					),
					InstructionEnum.LoadFalse => new TrLoadFalse(ins.DecodeA()),
					InstructionEnum.LFalseSkip => new TrLFalseSkip(ins.DecodeA()),
					InstructionEnum.LoadTrue => new TrLoadTrue(ins.DecodeA()),
					InstructionEnum.LoadNil => new TrLoadNil(ins.DecodeA(), ins.DecodeB()),
					InstructionEnum.GetUpVal => new TrGetUpVal(ins.DecodeA(), ins.DecodeB()),
					InstructionEnum.SetUpVal => new TrSetUpVal(ins.DecodeA(), ins.DecodeB()),
					InstructionEnum.GetTabUp => new TrGetTabUp(ins.DecodeA(), ins.DecodeB(), ins.DecodeC()),
					InstructionEnum.GetTable => new TrGetTable(ins.DecodeA(), ins.DecodeB(), ins.DecodeC()),
					InstructionEnum.GetI => new TrGetI(ins.DecodeA(), ins.DecodeB(), ins.DecodeC()),
					InstructionEnum.GetField => new TrGetField(
						ins.DecodeA(),
						ins.DecodeB(),
						CheckedConstant<LuaString>(proto.constants[GetExtraArg(enumerator)]).value
					),
					InstructionEnum.SetTabUp => new TrSetTabUp(ins.DecodeA(), ins.DecodeB(), ins.DecodeC()),
					InstructionEnum.SetTable => ins.DecodeK() switch
					{
						true => new TrSetTableK(
							ins.DecodeA(),
							ins.DecodeB(),
							ConstantToValue(proto.constants[ins.DecodeC()])
						),
						false => new TrSetTableC(
							ins.DecodeA(),
							ins.DecodeB(),
							ins.DecodeC()
						)
					},
					InstructionEnum.SetI => ins.DecodeK() switch
					{
						true => new TrSetIK(
							ins.DecodeA(),
							ins.DecodeB(),
							ConstantToValue(proto.constants[ins.DecodeC()])
						),
						false => new TrSetIR(
							ins.DecodeA(),
							ins.DecodeB(),
							ins.DecodeC()
						)
					},
					InstructionEnum.SetField => ins.DecodeK() switch
					{
						true => new TrSetFieldK(
							ins.DecodeA(),
							CheckedConstant<LuaString>(proto.constants[ins.DecodeB()]).value,
							ConstantToValue(proto.constants[ins.DecodeC()])
						),
						false => new TrSetFieldR(
							ins.DecodeA(),
							CheckedConstant<LuaString>(proto.constants[ins.DecodeB()]).value,
							ins.DecodeC()
						)
					},
					InstructionEnum.NewTable => new TrNewTable(ins.DecodeA())
				};
			}
		}

		private static uint GetExtraArg(IEnumerator<LuaInstruction> iterator)
		{
			if (!iterator.MoveNext())
			{
				throw new ExpectedExtraArgException("Expected extra arg after LoadKx");
			}
			if (iterator.Current.DecodeOpCode() != InstructionEnum.ExtraArg)
			{
				throw new ExpectedExtraArgException("Expected extra arg after LoadKx");
			}
			return iterator.Current.DecodeAx();
		}
	}
}
