﻿using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	public class OGetField(byte a, byte b, LString kC) : MetaMethodTransition(a)
	{
		public override bool NormalOrMetaAccess(LuaState state, LStackFrame stackFrame)
		{
			var table = (LTable)state.Stack[stackFrame.FrameBase + b];
			var val = table.GetValue(kC.Value);

			if (val is LNil)
				return CallMetaMethod(state, stackFrame, [table, kC], MetaMethodTag.Index);
			
			state.Stack[stackFrame.FrameBase + a] = val;
			return false;
		}
	}
}
