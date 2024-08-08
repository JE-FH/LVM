using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	internal class OSetTableR(byte a, byte b, byte c) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame) {
			MetaMethodHelper.TableSet(
				state, stackFrame,
				() => (LTable)state.Stack[stackFrame.FrameBase + a],
				() => state.Stack[stackFrame.FrameBase + b],
				() => state.Stack[stackFrame.FrameBase + c]
			);
		}
	}
}
