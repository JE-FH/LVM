using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	public class OGetTable(byte a, byte b, byte c) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame) {
			MetaMethodHelper.TableGetMM(
				state, stackFrame,
				() => (LTable)state.Stack[stackFrame.FrameBase + b],
				() => state.Stack[stackFrame.FrameBase + c],
				(val) => state.Stack[stackFrame.FrameBase + a] = val
			);
		}
	}
}
