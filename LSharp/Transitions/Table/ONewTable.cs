using LSharp.LTypes;

namespace LSharp.Transitions.Table
{
	internal class ONewTable(byte a, byte  b, byte c, bool k, uint aX) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame)
		{
			//TODO: Use all the given information
			state.Stack[stackFrame.FrameBase + a] = new LTable();
			stackFrame.PC += 1;
		}
	}
}
