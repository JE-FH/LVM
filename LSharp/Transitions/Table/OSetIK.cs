using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	internal class OSetIK(byte a, byte b, ILValue kC) : MetaMethodTransition(-1)
	{
		public override bool NormalOrMetaAccess(LuaState state, LStackFrame stackFrame)
		{
			var table = (LTable)state.Stack[stackFrame.FrameBase + a];
			var updateCtx = table.HasValueMaybeUpdate(b);

			if (!updateCtx.HasValue)
				return CallMetaMethod(state, stackFrame, [table, new LInteger(b), kC], MetaMethodTag.NewIndex);

			table.UpdateValue(updateCtx, kC);
			return false;
		}
	}
}
