using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	public class OSetFieldR(byte a, LString kB, byte c) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame) {
			MetaMethodHelper.TableSetMM(
				state, stackFrame,
				() => (LTable)state.Stack[stackFrame.FrameBase + a],
				() => kB,
				() => state.Stack[stackFrame.FrameBase + c]
			);
		}
	}
}
