using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

			if (updateCtx.HasValue)
			{
				table.UpdateValue(updateCtx, val);
				stackFrame.PC += 1;
				return false;
			}

			return CallMetaMethod(state, stackFrame, [table, key, val], MetaMethodTag.NewIndex);
		}
	}
}
