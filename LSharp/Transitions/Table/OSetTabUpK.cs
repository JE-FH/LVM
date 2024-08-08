using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	public class OSetTabUpR(byte a, LString kB, byte c) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame) {
			MetaMethodHelper.TableSet(
				state, stackFrame,
				() => (LTable)stackFrame.Closure.UpValues[a].Value,
				() => kB,
				() => state.Stack[stackFrame.FrameBase + c]
			);
		}
	}
}
