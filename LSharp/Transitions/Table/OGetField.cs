using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	public class OGetField(byte a, byte b, LString kC) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame) {
			MetaMethodHelper.TableGetMM(
				state, stackFrame,
				() => (LTable)state.Stack[stackFrame.FrameBase + b],
				() => kC,
				(val) => state.Stack[stackFrame.FrameBase + a] = val
			);
		}
	}
}
