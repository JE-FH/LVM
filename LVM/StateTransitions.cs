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
			luaState.stack[ci.stackBase + A] = luaState.stack[ci.stackBase + B];
			ci.pc++;
		}
	}

	public class TrLoadI(byte A, int sBx) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			luaState.stack[ci.stackBase + A] = new LuaInteger(sBx);
			ci.pc++;
		}
	}

	public class TrLoadF(byte A, int sBx) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			luaState.stack[ci.stackBase + A] = new LuaNumber(sBx);
			ci.pc++;
		}
	}

	public class TrLoadK(byte A, IRuntimeValue constant) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			luaState.stack[ci.stackBase + A] = constant;
			ci.pc++;
		}
	}

	public class TrLoadKx(byte A, IRuntimeValue constant) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			luaState.stack[ci.stackBase + A] = constant;
			ci.pc += 2;
		}
	}

	public class TrLoadFalse(byte A) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			luaState.stack[ci.stackBase + A] = new LuaBool(false);
			ci.pc++;
		}
	}

	public class TrLFalseSkip(byte A) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			luaState.stack[ci.stackBase + A] = new LuaBool(false);
			ci.pc += 2;
		}
	}

	public class TrLoadTrue(byte A) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			luaState.stack[ci.stackBase + A] = new LuaBool(true);
			ci.pc++;
		}
	}

	public class TrLoadNil(byte A, byte B) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			for (int i = A; i < A + B; ++i) {
				luaState.stack[ci.stackBase + i] = new LuaNil();
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
			LuaTable table = (LuaTable) luaState.stack[ci.stackBase + B];
			luaState.stack[ci.stackBase + A] = table.GetValue(luaState.stack[ci.stackBase + C]);
			ci.pc += 1;
		}
	}

	public class TrGetI(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaTable table = (LuaTable) luaState.stack[ci.stackBase + B];
			luaState.stack[ci.stackBase + A] = table.GetValue(C);
			ci.pc += 1;
		}
	}

	//The constant can only be a short string to conform with the native lua VM
	public class TrGetField(byte A, byte B, byte[] KC) : IStateTransition {
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaTable table = (LuaTable)luaState.stack[ci.stackBase + B];
			luaState.stack[ci.stackBase + A] = table.GetValue(KC);
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
			LuaTable table = (LuaTable)luaState.stack[ci.stackBase + A];
			table.SetValue(luaState.stack[ci.stackBase + B], KC);
			ci.pc += 1;
		}
	}

	public class TrSetTableC(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaTable table = (LuaTable)luaState.stack[ci.stackBase + A];
			table.SetValue(luaState.stack[ci.stackBase + B], luaState.stack[ci.stackBase + C]);
			ci.pc += 1;
		}
 	}

	public class TrSetIK(byte A, byte B, IRuntimeValue KC) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaTable table = (LuaTable)luaState.stack[ci.stackBase + A];
			table.SetValue(B, KC);
			ci.pc += 1;
		}
	}

	public class TrSetIR(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaTable table = (LuaTable)luaState.stack[ci.stackBase + A];
			table.SetValue(B, luaState.stack[ci.stackBase + C]);
			ci.pc += 1;
		}
	}

	//The constant can only be a short string to conform with the native lua VM
	public class TrSetFieldK(byte A, byte[] KB, IRuntimeValue KC) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaTable table = (LuaTable)luaState.stack[ci.stackBase + A];
			table.SetValue(KB, KC);
			ci.pc += 1;
		}
	}

	//The constant can only be a short string to conform with the native lua VM
	public class TrSetFieldR(byte A, byte[] KB, byte C) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaTable table = (LuaTable)luaState.stack[ci.stackBase + A];
			table.SetValue(KB, luaState.stack[ci.stackBase = C]);
			ci.pc += 1;
		}
	}

	//Use all the arguments, including extraarg to construct the table
	public class TrNewTable(byte A) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			luaState.stack[ci.stackBase + A] = new LuaTable();
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
