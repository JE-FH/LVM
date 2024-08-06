﻿using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	public class OGetTabUp(byte a, byte b, LString kC) : MetaMethodTransition(a)
	{
		public override bool NormalOrMetaAccess(LuaState state, LStackFrame stackFrame)
		{
			var table = (LTable)stackFrame.Closure.UpValues[b].Value;
			var val = table.GetValue(kC.Value);

			if (val is LNil) 
				return CallMetaMethod(state, stackFrame, [table, kC], MetaMethodTag.Index);
			
			state.Stack[stackFrame.FrameBase + a] = val;
			return false;

		}
	}
}
