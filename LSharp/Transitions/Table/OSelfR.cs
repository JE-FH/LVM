using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	internal class OSelfR(byte a, byte b, byte c) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame) {
			if (!stackFrame.MetaMethodStalled) {
				state.Stack[stackFrame.FrameBase + a + 1] =
					state.Stack[stackFrame.FrameBase + b];
			}

			MetaMethodHelper.TableGetMM(
				state, stackFrame,
				() => (LTable)state.Stack[stackFrame.FrameBase + b],
				() => (LString)state.Stack[stackFrame.FrameBase + c],
				(val) => state.Stack[stackFrame.FrameBase + a] = val
			);
		}
	}
}
