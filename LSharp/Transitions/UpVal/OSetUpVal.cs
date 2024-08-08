using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.UpVal
{
	public class OSetUpVal(byte a, byte b) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame)
		{
			stackFrame.Closure.UpValues[b].Value = state.Stack[stackFrame.FrameBase + a];
			stackFrame.PC += 1;
		}
	}
}
