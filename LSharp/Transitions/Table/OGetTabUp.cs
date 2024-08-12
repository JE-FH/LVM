using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	public class OGetTabUp(byte a, byte b, LString kC) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame) {
			MetaMethodHelper.TableGetMM(
				state, stackFrame,
				() => (LTable)stackFrame.Closure.UpValues[b].Value,
				() => kC,
				(val) => state.Stack[stackFrame.FrameBase + a] = val
			);
		}
	}
}
