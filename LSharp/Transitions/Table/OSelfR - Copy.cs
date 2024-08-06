﻿using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	internal class OSelfK(byte a, byte b, LString kC) : MetaMethodTransition(a)
	{
		public override bool NormalOrMetaAccess(LuaState state, LStackFrame stackFrame)
		{
			state.Stack[stackFrame.FrameBase + a + 1] = 
				state.Stack[stackFrame.FrameBase + b];

			var table = (LTable)state.Stack[stackFrame.FrameBase + b];
			var val = table.GetValue(kC.Value);

			if (val is LNil)
				return CallMetaMethod(state, stackFrame, [table, kC], MetaMethodTag.Index);

			state.Stack[stackFrame.FrameBase + a] = val;
			return false;
		}
	}
}
