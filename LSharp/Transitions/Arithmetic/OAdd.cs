using LSharp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.Arithmetic
{
	internal class OAdd(byte a, byte b, byte c) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame)
		{
			stackFrame.PC += 1;
			var lhs = state.Stack[stackFrame.FrameBase + b];
			var rhs = state.Stack[stackFrame.FrameBase + c];

			var res = ArithmeticHelper.Add(lhs, rhs);
			if (res == null)
				return;

			state.Stack[stackFrame.FrameBase + a] = res;
			stackFrame.PC += 1;
		}
	}
}
