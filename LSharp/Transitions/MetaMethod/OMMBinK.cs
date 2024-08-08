using LSharp.LTypes;

namespace LSharp.Transitions.MetaMethod
{
	public class OMMBinK(byte a, ILValue b, MetaMethodTag c, byte originalA) : MetaMethodTransition(originalA)
	{
		public override bool NormalOrMetaAccess(LState state, LStackFrame stackFrame)
		{
			var lhs = state.Stack[stackFrame.FrameBase + a];
			return CallMetaMethod(state, stackFrame, [lhs, b], c);
		}
	}
}
