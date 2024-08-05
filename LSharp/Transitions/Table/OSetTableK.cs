using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.Table
{
	internal class OSetTableK(byte a, byte b, ILValue kC) : MetaMethodTransition(-1)
	{
		public override bool NormalOrMetaAccess(LuaState state, LStackFrame stackFrame)
		{
			var table = (LTable)state.Stack[stackFrame.FrameBase + a];
			var key = state.Stack[stackFrame.FrameBase + b];
			var updateCtx = table.HasValueMaybeUpdate(key);

			if (updateCtx.HasValue)
			{
				table.UpdateValue(updateCtx, kC);
				stackFrame.PC += 1;
				return false;
			}

			return CallMetaMethod(state, stackFrame, [table, key, kC], MetaMethodTag.NewIndex);
		}
	}
}
