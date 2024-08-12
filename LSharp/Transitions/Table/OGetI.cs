using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	internal class OGetI(byte a, byte b, LInteger c) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame) {
			MetaMethodHelper.TableGetMM(
				state, stackFrame,
				() => (LTable)state.Stack[stackFrame.FrameBase + b],
				() => c,
				(val) => state.Stack[stackFrame.FrameBase + a] = val
			);
		}
	}
}
