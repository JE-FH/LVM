using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	internal class OSetIR(byte a, byte b, byte c) : MetaMethodTransition(-1)
	{
		public override bool NormalOrMetaAccess(LuaState state, LStackFrame stackFrame)
		{
			var table = (LTable)state.Stack[stackFrame.FrameBase + a];
			var val = state.Stack[stackFrame.FrameBase + c];
			var updateCtx = table.HasValueMaybeUpdate(b);

			if (!updateCtx.HasValue)
				return CallMetaMethod(state, stackFrame, [table, new LInteger(b), val], MetaMethodTag.NewIndex);

			table.UpdateValue(updateCtx, val);
			return false;
		}
	}
}
