using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	internal class OSetIR(byte a, LInteger b, byte c) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame) {
			MetaMethodHelper.TableSet(
				state, stackFrame,
				() => (LTable)state.Stack[stackFrame.FrameBase + a],
				() => b,
				() => state.Stack[stackFrame.FrameBase + c]
			);
		}
	}
}
