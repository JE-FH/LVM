﻿using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	internal class OSetTableR(byte a, byte b, byte c) : MetaMethodTransition(-1)
	{
		public override bool NormalOrMetaAccess(LuaState state, LStackFrame stackFrame)
		{
			var table = (LTable)state.Stack[stackFrame.FrameBase + a];
			var key = state.Stack[stackFrame.FrameBase + b];
			var val = state.Stack[stackFrame.FrameBase + c];
			var updateCtx = table.HasValueMaybeUpdate(key);

			if (!updateCtx.HasValue)
				return CallMetaMethod(state, stackFrame, [table, key, val], MetaMethodTag.NewIndex);
			
			table.UpdateValue(updateCtx, val);
			return false;
		}
	}
}
