using System.Reflection.Metadata;

namespace LVM
{
	public class ExtraArgInstructionReachedException(int _pc) : Exception("Extra arg instruction was reached")
	{
		public int pc = _pc;
	}

	public interface IStateTransition
	{
		void Execute(CallInfo ci, LuaState luaState);
	}

	public class TrMove(byte A, byte B) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			ci[A] = ci[B];
			ci.pc++;
		}
	}

	public class TrLoadI(byte A, int sBx) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			ci[A] = new LuaInteger(sBx);
			ci.pc++;
		}
	}

	public class TrLoadF(byte A, int sBx) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			ci[A] = new LuaNumber(sBx);
			ci.pc++;
		}
	}

	public class TrLoadK(byte A, IRuntimeValue constant) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			ci[A] = constant;
			ci.pc++;
		}
	}

	public class TrLoadKx(byte A, IRuntimeValue constant) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			ci[A] = constant;
			ci.pc += 2;
		}
	}

	public class TrLoadFalse(byte A) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			ci[A] = new LuaBool(false);
			ci.pc++;
		}
	}

	public class TrLFalseSkip(byte A) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			ci[A] = new LuaBool(false);
			ci.pc += 2;
		}
	}

	public class TrLoadTrue(byte A) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			ci[A] = new LuaBool(true);
			ci.pc++;
		}
	}

	public class TrLoadNil(byte A, byte B) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			for (int i = A; i < A + B; ++i) {
				ci[i] = new LuaNil();
			}
			ci.pc++;
		}
	}

	public class TrGetUpVal(byte A, byte B) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			throw new NotImplementedException();
		}
	}

	public class TrSetUpVal(byte A, byte B) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			throw new NotImplementedException();
		}
	}

	public class TrGetTabUp(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			throw new NotImplementedException();
		}
	}

	public class TrGetTable(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(B);
			ci[A] = table.GetValue(ci[C]);
			ci.pc += 1;
		}
	}

	public class TrGetI(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(B);
			ci[A] = table.GetValue(C);
			ci.pc += 1;
		}
	}

	//The constant can only be a short string to conform with the native lua VM
	public class TrGetField(byte A, byte B, byte[] KC) : IStateTransition {
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(B);
			ci[A] = table.GetValue(KC);
			ci.pc++;
		}
	}

	public class TrSetTabUp(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			throw new NotImplementedException();
		}
	}

	public class TrSetTableK(byte A, byte B, IRuntimeValue KC) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(A);
			table.SetValue(ci[B], KC);
			ci.pc += 1;
		}
	}

	public class TrSetTableC(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(A);
			table.SetValue(ci[B], ci[C]);
			ci.pc += 1;
		}
 	}

	public class TrSetIK(byte A, byte B, IRuntimeValue KC) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(A);
			table.SetValue(B, KC);
			ci.pc += 1;
		}
	}

	public class TrSetIR(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(A);
			table.SetValue(B, ci[C]);
			ci.pc += 1;
		}
	}

	//The constant can only be a short string to conform with the native lua VM
	public class TrSetFieldK(byte A, byte[] KB, IRuntimeValue KC) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(A);
			table.SetValue(KB, KC);
			ci.pc += 1;
		}
	}

	//The constant can only be a short string to conform with the native lua VM
	public class TrSetFieldR(byte A, byte[] KB, byte C) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(A);
			table.SetValue(KB, ci[C]);
			ci.pc += 1;
		}
	}

	//Use all the arguments, including extraarg to construct the table
	public class TrNewTable(byte A) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			ci[A] = new LuaTable();
		}
	}

	public class TrSelfK(byte A, byte B, byte[] KC) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(B);
			ci[A + 1] = table;
			ci[A] = table.GetValue(KC);
		}
	}

	public class TrSelfR(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaTable table = ci.GetRegister<LuaTable>(B);
			LuaString RC = ci.GetRegister<LuaString>(C);
			ci[A + 1] = table;
			ci[A] = table.GetValue(RC.value);
		}
	}


	public class TrAddI(byte A, byte B, sbyte sC) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			ci[A] = ci[B] switch
			{
				LuaNumber n => new LuaNumber(n.value + sC),
				LuaInteger i => new LuaInteger(i.value + sC),
				_ => throw new Exception("Some error")
			};
		}
	}

	public abstract class TrOpKNum(byte A, byte B) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			ci[A] = new LuaNumber(Operation(ci.GetRegister<LuaNumber>(A).value));
		}

		protected abstract double Operation(double a);
	}

	public class TrAddK(byte A, byte B, double K) : TrOpKNum(A, B)
	{
		protected override double Operation(double a)
		{
			return a + K;
		}
	}

	public class TrSubK(byte A, byte B, double K) : TrOpKNum(A, B)
	{
		protected override double Operation(double a)
		{
			return a - K;
		}
	}

	public class TrMulK(byte A, byte B, double K) : TrOpKNum(A, B)
	{
		protected override double Operation(double a)
		{
			return a * K;
		}
	}

	public class TrModK(byte A, byte B, double K) : TrOpKNum(A, B)
	{
		protected override double Operation(double a)
		{
			return a % K;
		}
	}

	public class TrPowK(byte A, byte B, double K) : TrOpKNum(A, B)
	{
		protected override double Operation(double a)
		{
			return Math.Pow(a, K);
		}
	}

	public class TrDivK(byte A, byte B, double K) : TrOpKNum(A, B)
	{
		protected override double Operation(double a)
		{
			return a / K;
		}
	}

	public class TrIDivK(byte A, byte B, double K) : TrOpKNum(A, B)
	{
		protected override double Operation(double a)
		{
			return ((int)a) / ((int)K);
		}
	}

	public abstract class TrOpKInt(byte A, byte B) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			ci[A] = new LuaNumber(Operation(ci[B] switch
			{

			}));
		}

		protected abstract double Operation(long a);
	}

	public class TrAndKInt(byte A, byte B, long K) : TrOpKInt(A, B)
	{
		protected override double Operation(long a)
		{
			return a & K;
		}
	}

	public class TrOrKInt(byte A, byte B, long K) : TrOpKInt(A, B)
	{
		protected override double Operation(long a)
		{
			return a | K;
		}
	}

	public class TrXorKInt(byte A, byte B, long K) : TrOpKInt(A, B)
	{
		protected override double Operation(long a)
		{
			return a ^ K;
		}
	}


	public class TrExtraArg() : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			throw new ExtraArgInstructionReachedException(ci.pc);
		}
	}
}
