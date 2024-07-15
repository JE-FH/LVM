namespace LVM
{
	interface IStateTransition
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
			luaState.stack[ci.stackBase + A] = new LuaStackInteger(sBx);
			ci.pc++;
		}
	}

	public class TrLoadF(byte A, int sBx) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			luaState.stack[ci.stackBase + A] = new LuaStackNumber(sBx);
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

	public class TrLoadKX(byte A, IRuntimeValue constant) : IStateTransition
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
			luaState.stack[ci.stackBase + A] = new LuaStackBool(false);
			ci.pc++;
		}
	}

	public class TrLFalseSkip(byte A) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			luaState.stack[ci.stackBase + A] = new LuaStackBool(false);
			ci.pc += 2;
		}
	}

	public class TrLoadTrue(byte A) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			luaState.stack[ci.stackBase + A] = new LuaStackBool(true);
			ci.pc++;
		}
	}

	public class TrLoadNil(byte A, byte B) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			for (int i = A; i < A + B; ++i) {
				luaState.stack[ci.stackBase + i] = new LuaStackNil();
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
			LuaStackTable table = (LuaStackTable) luaState.stack[ci.stackBase + B];
			luaState.stack[ci.stackBase + A] = table.GetValue(luaState.stack[ci.stackBase + C]);
			ci.pc += 1;
		}
	}

	public class TrGetI(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaStackTable table = (LuaStackTable) luaState.stack[ci.stackBase + B];
			luaState.stack[ci.stackBase + A] = table.GetValue(C);
			ci.pc += 1;
		}
	}

	//The constant can only be a short string to conform with the native lua VM
	public class TrGetField(byte A, byte B, LuaStackString constant) : IStateTransition {
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaStackTable table = (LuaStackTable)luaState.stack[ci.stackBase + B];
			luaState.stack[ci.stackBase + A] = table.GetValue(constant.value);
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

	public class TrSetTableK(byte A, byte B, IRuntimeValue constant) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaStackTable table = (LuaStackTable)luaState.stack[ci.stackBase + A];
			table.SetValue(luaState.stack[ci.stackBase + B], constant);
			ci.pc += 1;
		}
	}

	public class TrSetTable(byte A, byte B, byte C) : IStateTransition
	{
		public void Execute(CallInfo ci, LuaState luaState)
		{
			LuaStackTable table = (LuaStackTable)luaState.stack[ci.stackBase + A];
			table.SetValue(luaState.stack[ci.stackBase + B], luaState.stack[ci.stackBase + C]);
			ci.pc += 1;
		}
 	}
}
