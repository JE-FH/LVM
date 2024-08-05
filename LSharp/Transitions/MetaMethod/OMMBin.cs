namespace LSharp.Transitions.MetaMethod
{
	public class OMMBin(byte a, byte b, MetaMethodTag c, byte originalA) : MetaMethodTransition(originalA)
    {
		public override bool NormalOrMetaAccess(LuaState state, LStackFrame stackFrame)
		{
			var lhs = state.Stack[stackFrame.FrameBase + a];
			var rhs = state.Stack[stackFrame.FrameBase + b];
			return CallMetaMethod(state, stackFrame, [lhs, rhs], c);
		}
    }
}
