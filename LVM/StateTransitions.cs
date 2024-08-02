using LVM.RuntimeType;
using System.Text;

namespace LVM
{
	public class ExtraArgInstructionReachedException(int _pc) : Exception("Extra arg instruction was reached")
	{
		public int pc = _pc;
	}

	public class WrongRegisterTypeException(int _registerIndex, IRuntimeValue _registerValue) : Exception
	{
		public int registerIndex = _registerIndex;
		public IRuntimeValue registerValue = _registerValue;
	}

	public class UndefinedBinaryOperationException(int _registerIndexA, int _registerIndexB, IRuntimeValue _valueA, IRuntimeValue _valueB) : Exception
	{
		public int registerIndexA = _registerIndexA;
		public int registerIndexB = _registerIndexB;
		public IRuntimeValue valueA = _valueA;
		public IRuntimeValue valueB = _valueB;
	}

	public interface IStateTransition
	{
		void Execute(LuaCallInfo ci, LuaState luaState);
	}

	public class TrMove(byte A, byte B) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			ci[A] = ci[B];
			ci.pc++;
		}
	}

	public class TrLoadI(byte A, int sBx) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			ci[A] = new LuaInteger(sBx);
			ci.pc++;
		}
	}

	public class TrLoadF(byte A, int sBx) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			ci[A] = new LuaNumber(sBx);
			ci.pc++;
		}
	}

	public class TrLoadK(byte A, IRuntimeValue constant) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			ci[A] = constant;
			ci.pc++;
		}
	}

	public class TrLoadKx(byte A, IRuntimeValue constant) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			ci[A] = constant;
			ci.pc += 2;
		}
	}

	public class TrLoadFalse(byte A) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			ci[A] = new LuaBool(false);
			ci.pc++;
		}
	}

	public class TrLFalseSkip(byte A) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			ci[A] = new LuaBool(false);
			ci.pc += 2;
		}
	}

	public class TrLoadTrue(byte A) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			ci[A] = new LuaBool(true);
			ci.pc++;
		}
	}

	public class TrLoadNil(byte A, byte B) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			for (int i = A; i < A + B; ++i) {
				ci[i] = new LuaNil();
			}
			ci.pc++;
		}
	}

	public class TrGetUpVal(byte A, byte B) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			ci[A] = ci.Closure.upValues[B].value;
		}
	}

	public class TrSetUpVal(byte A, byte B) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			ci.Closure.upValues[A].value = ci[B];
		}
	}

	//TODO: Call metamethod
	public class TrGetTabUp(byte A, byte B, byte[] KC) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			if (ci.Closure.upValues[B].value is LuaTable realValue)
			{
				ci[A] = realValue.GetValue(KC);
				ci.pc += 1;
			}
			else
			{
				throw new Exception("some exception");
			}
		}
	}

	//TODO: Call metamethod
	public class TrGetTable(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(B);
			ci[A] = table.GetValue(ci[C]);
			ci.pc += 1;
		}
	}

	//TODO: Call metamethod
	public class TrGetI(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(B);
			ci[A] = table.GetValue(C);
			ci.pc += 1;
		}
	}

	//The constant can only be a short string to conform with the native lua VM
	//TODO: Call metamethod
	public class TrGetField(byte A, byte B, byte[] KC) : IStateTransition {
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(B);
			ci[A] = table.GetValue(KC);
			ci.pc++;
		}
	}

	//TODO: Call metamethod
	public class TrSetTabUpK(byte A, byte[] KB, IRuntimeValue C) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			if (ci.Closure.upValues[A].value is LuaTable realValue)
			{
				realValue.SetValue(KB, C);
				ci.pc += 1;
			}
			else
			{
				throw new Exception("some exception");
			}
		}
	}

	//TODO: Call metamethod
	public class TrSetTabUpR(byte A, byte[] KB, byte C) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			if (ci.Closure.upValues[A].value is LuaTable realValue)
			{
				realValue.SetValue(KB, ci[C]);
				ci.pc += 1;
			}
			else
			{
				throw new Exception("some exception");
			}
		}
	}

	//TODO: Call metamethod
	public class TrSetTableK(byte A, byte B, IRuntimeValue KC) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(A);
			table.SetValue(ci[B], KC);
			ci.pc += 1;
		}
	}

	//TODO: Call metamethod
	public class TrSetTableC(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(A);
			table.SetValue(ci[B], ci[C]);
			ci.pc += 1;
		}
	}

	//TODO: Call metamethod
	public class TrSetIK(byte A, byte B, IRuntimeValue KC) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(A);
			table.SetValue(B, KC);
			ci.pc += 1;
		}
	}

	//TODO: Call metamethod
	public class TrSetIR(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(A);
			table.SetValue(B, ci[C]);
			ci.pc += 1;
		}
	}

	//The constant can only be a short string to conform with the native lua VM
	//TODO: Call metamethod
	public class TrSetFieldK(byte A, byte[] KB, IRuntimeValue KC) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(A);
			table.SetValue(KB, KC);
			ci.pc += 1;
		}
	}

	//The constant can only be a short string to conform with the native lua VM
	//TODO: Call metamethod
	public class TrSetFieldR(byte A, byte[] KB, byte C) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(A);
			table.SetValue(KB, ci[C]);
			ci.pc += 1;
		}
	}

	//Use all the arguments, including extraarg to construct the table
	public class TrNewTable(byte A) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			ci[A] = new LuaTable();
			ci.pc += 2;
		}
	}

	public class TrSelfK(byte A, byte B, byte[] KC) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(B);
			ci[A + 1] = table;
			ci[A] = table.GetValue(KC);
			ci.pc += 1;
		}
	}

	public class TrSelfR(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(B);
			LuaString RC = ci.GetRegister<LuaString>(C);
			ci[A + 1] = table;
			ci[A] = table.GetValue(RC.value);
			ci.pc += 1;
		}
	}

	public class TrAddI(byte A, byte B, sbyte sC) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			IRuntimeValue? val = ci[B] switch
			{
				LuaNumber n => new LuaNumber(n.value + sC),
				LuaInteger i => new LuaInteger(i.value + sC),
				_ => null
			};
			if (val != null)
			{
				ci[A] = val;
				ci.pc += 1;
			}
			ci.pc += 1;
		}
	}

	public abstract class TrOpKNum(byte A, byte B) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			double? val = ci.CoerceToNumber(A);
			if (val != null)
			{
				ci[A] = new LuaNumber(Operation(val.Value));
				ci.pc += 1;
			}
			ci.pc += 1;
		}

		protected abstract double Operation(double a);
	}

	public class TrAddK(byte A, byte B, double K) : TrOpKNum(A, B)
	{
		protected override double Operation(double a) => a + K;
	}

	public class TrSubK(byte A, byte B, double K) : TrOpKNum(A, B)
	{
		protected override double Operation(double a) => a - K;
	}

	public class TrMulK(byte A, byte B, double K) : TrOpKNum(A, B)
	{
		protected override double Operation(double a) => a * K;
	}

	public class TrModK(byte A, byte B, double K) : TrOpKNum(A, B)
	{
		protected override double Operation(double a) => a % K;
	}

	public class TrPowK(byte A, byte B, double K) : TrOpKNum(A, B)
	{
		protected override double Operation(double a) => Math.Pow(a, K);
	}

	public class TrDivK(byte A, byte B, double K) : TrOpKNum(A, B)
	{
		protected override double Operation(double a) => a / K;
	}

	public class TrIDivK(byte A, byte B, double K) : TrOpKNum(A, B)
	{
		protected override double Operation(double a) => ((int)a) / ((int)K);
	}

	public abstract class TrOpKInt(byte A, byte B) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			long? val = ci.CoerceToInteger(B);
			if (val != null)
			{
				ci[A] = new LuaNumber(Operation(val.Value));
				ci.pc += 1;
			}
			ci.pc += 1;
		}

		protected abstract double Operation(long a);
	}

	public class TrBAndK(byte A, byte B, long K) : TrOpKInt(A, B)
	{
		protected override double Operation(long a) => a & K;
	}

	public class TrBOrK(byte A, byte B, long K) : TrOpKInt(A, B)
	{
		protected override double Operation(long a) => a | K;
	}

	public class TrBXorK(byte A, byte B, long K) : TrOpKInt(A, B)
	{
		protected override double Operation(long a) => a ^ K;
	}

	public class TrShrI(byte A, byte B, sbyte sC) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			long? val = ci.CoerceToInteger(B);
			if (val != null)
			{
				ci[A] = new LuaInteger(val.Value >> sC);
				ci.pc += 1;
			}
			ci.pc += 1;
		}
	}

	public class TrShlI(byte A, byte B, sbyte sC) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			long? val = ci.CoerceToInteger(B);
			if (val != null)
			{
				ci[A] = new LuaInteger(sC << (int)Math.Max(Math.Min(val.Value, 10), -10));
				ci.pc += 1;
			}
			ci.pc += 1;
		}
	}

	public abstract class TrBinaryRealOperation(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			IRuntimeValue? val = (ci[B], ci[C]) switch
			{
				(LuaInteger a, LuaInteger b) => new LuaInteger(Operation(a.value, b.value)),
				(LuaNumber a, LuaInteger b) => new LuaNumber(Operation(a.value, b.value)),
				(LuaInteger a, LuaNumber b) => new LuaNumber(Operation(a.value, b.value)),
				(LuaNumber a, LuaNumber b) => new LuaNumber(Operation(a.value, b.value)),
				_ => null
			};
			if (val != null)
			{
				ci[A] = val;
				ci.pc += 1;
			}
			ci.pc += 1;
		}

		public abstract double Operation(double a, double b);
		public abstract long Operation(long a, long b);
	}

	public abstract class TrBinaryNumOperation(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			double? a = ci.CoerceToNumber(B);
			double? b = ci.CoerceToNumber(C);
			if (a != null && b != null)
			{
				ci[A] = new LuaNumber(Operation(a.Value, b.Value));
				ci.pc += 1;
			}
			ci.pc += 1;
		}
		public abstract double Operation(double a, double b);
	}

	//TODO: Inaccurate, should error if both are double https://www.lua.org/source/5.4/ltm.c.html#luaT_trybinTM
	public abstract class TrBinaryIntOperation(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			long? a = ci.CoerceToInteger(B);
			long? b = ci.CoerceToInteger(C);
			if (a != null && b != null)
			{
				ci[A] = new LuaInteger(Operation(a.Value, b.Value));
				ci.pc += 1;
			}
			ci.pc += 1;
		}
		public abstract long Operation(long a, long b);
	}

	public class TrAdd(byte A, byte B, byte C) : TrBinaryRealOperation(A, B, C)
	{
		public override double Operation(double a, double b) => a + b;
		public override long Operation(long a, long b) => a + b;
	}

	public class TrSub(byte A, byte B, byte C) : TrBinaryRealOperation(A, B, C)
	{
		public override double Operation(double a, double b) => a - b;
		public override long Operation(long a, long b) => a - b;
	}

	public class TrMul(byte A, byte B, byte C) : TrBinaryRealOperation(A, B, C)
	{
		public override double Operation(double a, double b) => a * b;
		public override long Operation(long a, long b) => a * b;
	}

	public class TrMod(byte A, byte B, byte C) : TrBinaryRealOperation(A, B, C)
	{
		public override double Operation(double a, double b) => a % b;
		public override long Operation(long a, long b) => a % b;
	}

	public class TrPow(byte A, byte B, byte C) : TrBinaryNumOperation(A, B, C)
	{
		public override double Operation(double a, double b) => Math.Pow(a, b);
	}

	public class TrDiv(byte A, byte B, byte C) : TrBinaryNumOperation(A, B, C)
	{
		public override double Operation(double a, double b) => a / b;
	}

	public class TrIDiv(byte A, byte B, byte C) : TrBinaryIntOperation(A, B, C)
	{
		public override long Operation(long a, long b) => a / b;
	}

	public class TrBAnd(byte A, byte B, byte C) : TrBinaryIntOperation(A, B, C)
	{
		public override long Operation(long a, long b) => a & b;
	}

	public class TrBOr(byte A, byte B, byte C) : TrBinaryIntOperation(A, B, C)
	{
		public override long Operation(long a, long b) => a | b;
	}

	public class TrBXor(byte A, byte B, byte C) : TrBinaryIntOperation(A, B, C)
	{
		public override long Operation(long a, long b) => a ^ b;
	}

	public class TrShl(byte A, byte B, byte C) : TrBinaryIntOperation(A, B, C)
	{
		public override long Operation(long a, long b) => a << ((byte)b);
	}

	public class TrShr(byte A, byte B, byte C) : TrBinaryIntOperation(A, B, C)
	{
		public override long Operation(long a, long b) => a >> ((byte)b);
	}

	public class TrMetaMethodBinary(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			throw new NotImplementedException();
		}
	}

	public class TrMetaMethodBinaryI(byte A, sbyte B, byte C, bool k) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			throw new NotImplementedException();
		}
	}

	public class TrMetaMethodBinaryK(byte A, byte B, byte C, bool k) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			throw new NotImplementedException();
		}
	}

	public abstract class TrUnaryRealOperation(byte A, byte B) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			IRuntimeValue? val = ci[B] switch
			{
				LuaInteger a => new LuaInteger(Operation(a.value)),
				LuaNumber a => new LuaNumber(Operation(a.value)),
				_ => null
			};
			if (val != null)
			{
				ci[A] = val;
				ci.pc += 1;
			}
			ci.pc += 1;
		}

		public abstract double Operation(double a);
		public abstract long Operation(long a);
	}

	public abstract class TrUnaryIntOperation(byte A, byte B) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			long? val = ci.CoerceToInteger(B);
			if (val != null)
			{
				ci[A] = new LuaInteger(Operation(val.Value));
				ci.pc += 1;
			}
			ci.pc += 1;
		}

		public abstract long Operation(long a);
	}

	public class TrUnaryMinus(byte A, byte B) : TrUnaryRealOperation(A, B)
	{
		public override long Operation(long a) => -a;
		public override double Operation(double a) => -a;
	}

	public class TrBNot(byte A, byte B) : TrUnaryIntOperation(A, B)
	{
		public override long Operation(long a) => ~a;
	}

	public class TrNot(byte A, byte B) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			ci[A] = new LuaBool(ci[B] switch
			{
				LuaBool b => !b.value,
				LuaNil => true,
				_ => false
			});
		}
	}

	public class TrLen(byte A, byte B) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			ci[A] = ci[B] switch
			{
				LuaTable t => new LuaInteger(t.GetLength()),
				LuaString t => new LuaInteger(t.value.Length),
				//TODO: throw the actual error
				_ => throw new Exception("some error")
			};
			ci.pc += 1;
		}
	}

	public class TrConcat(byte A, byte B) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			var convertedByteArray = Enumerable.Range(A, A + B - 1)
				.SelectMany((i) => ci[i] switch
				{
					LuaNumber arg => Encoding.UTF8.GetBytes(arg.value.ToString()),
					LuaInteger arg => Encoding.UTF8.GetBytes(arg.value.ToString()),
					LuaString arg => arg.value,
					_ => throw new Exception("some error")
				});

			ci[A] = new LuaString(convertedByteArray.ToArray());
			ci.pc += 1;
		}
	}

	public class TrClose(byte A) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			throw new NotImplementedException();
		}
	}

	public class TrTBC(byte A) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			throw new NotImplementedException();
		}
	}

	public class TrJmp(int sJ) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			ci.pc += sJ;
		}
	}

	public class TrClosure(byte A, CompiledProto KProtoBx) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			var upValues = KProtoBx.upValues
				.Select((upValue) => 
					upValue.inStack
						? ci.GetStackReference(upValue.index)
						: ci.Closure.upValues[upValue.index]
				);
			ci[A] = new LuaClosure(KProtoBx, upValues.ToArray());
			ci.pc += 1;
		}
	}

	public class TrCall(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			//get the closure we are calling
			var closure = ci.GetRegister<ILuaClosure>(A);
			//set stack base to closure index + 1 eg. ci.stackBase + A + 1
			var stackBase = ci.stackBase + A + 1;
			//determine amount of arguments supplied, eg. B - 2
			var suppliedArgs = B - 1;
			if (suppliedArgs == -1) {
				//All arguments till the last stack item are supplied
				suppliedArgs = luaState.stack.StackLast - stackBase - 1;
			}
			//replace missing arguments by nil
			for (var i = stackBase + suppliedArgs; i < stackBase + closure.ParamCount; i++)
			{
				if (i > luaState.stack.StackLast)
				{
					luaState.stack.PushNils(1);
				}
				else
				{
					luaState.stack[i] = new LuaNil();
				}
			}

			var extraArgCount = suppliedArgs - closure.ParamCount;

			IRuntimeValue[] extraArgs;

			if (extraArgCount > 0)
			{
				extraArgs = Enumerable.Range(closure.ParamCount, extraArgCount)
					.Select(i => ci[i])
					.ToArray();
			}
			else
			{
				extraArgs = Array.Empty<IRuntimeValue>();
			}

			if (closure is LuaClosure luaClosure)
			{
				LuaCallInfo newCi = new LuaCallInfo(luaState, luaClosure, extraArgs)
				{
					stackBase = stackBase
				};

				luaState.callStack.Add(newCi);

				luaState.stack.ResizeTo((uint)(stackBase + luaClosure.proto.maxStackSize));
			}
			else if (closure is LuaCsClosure csClosure)
			{
				
			}

			

			ci.pc += 1;
		}
	}

	public class TrReturn(byte A, byte B, byte C, bool k) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			luaState.callStack.RemoveAt(luaState.callStack.Count - 1);
			luaState.stack.ResizeTo((uint)ci.stackBase - 1);
		}
	}

	public class TrReturn0 : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			luaState.callStack.RemoveAt(luaState.callStack.Count - 1);
			if (luaState.callStack.Count == 0)
			{
				luaState.stack.ResizeTo((uint)0);
				return;
			}
			var newCi = luaState.callStack.Last();
			luaState.stack.ResizeTo((uint)(newCi.stackBase + newCi.Closure.proto.maxStackSize));
		}
	}

	public class TrReturn1(byte A) : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			luaState.stack[ci.stackBase - 1] = ci[A];
			luaState.callStack.RemoveAt(luaState.callStack.Count - 1);
			if (luaState.callStack.Count == 0)
			{
				luaState.stack.ResizeTo((uint)1);
				return;
			}
			var newCi = luaState.callStack.Last();
			luaState.stack.ResizeTo((uint)(newCi.stackBase + newCi.Closure.proto.maxStackSize));
		}
	}

	public class TrNOP() : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			ci.pc += 1;
			return;
		}
	}

	public class TrExtraArg() : IStateTransition
	{
		public void Execute(LuaCallInfo ci, LuaState luaState)
		{
			throw new ExtraArgInstructionReachedException(ci.pc);
		}
	}
}
