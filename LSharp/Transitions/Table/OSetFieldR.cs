using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	public class OSetFieldR(byte a, LString kB, byte c) : MetaMethodTransition(-1)
	{
		public override bool NormalOrMetaAccess(LuaState state, LStackFrame stackFrame)
		{
			var table = (LTable)state.Stack[stackFrame.FrameBase + a];
			var updateCtx = table.HasValueMaybeUpdate(kB.Value);
			var val = state.Stack[stackFrame.FrameBase + c];

			if (!updateCtx.HasValue)
				return CallMetaMethod(state, stackFrame, [table, kB, val], MetaMethodTag.NewIndex);
			
			table.UpdateValue(updateCtx, val);
			return false;
		}
	}
}
