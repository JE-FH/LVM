using LSharp.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.Conditional {
	public class OTest(byte a, bool k) : ITransition {
		public void Transfer(LState state, LStackFrame stackFrame) {
			if (ArithmeticHelper.IsTruthy(state.Stack[stackFrame.FrameBase + a]) != k)
				stackFrame.PC += 2;
			else
				stackFrame.PC += 1;
		}
	}
}
