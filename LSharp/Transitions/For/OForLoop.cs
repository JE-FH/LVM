using LSharp.Helpers;
using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.For {
	public class OForLoop(byte a, uint bx) : ITransition {
		public void Transfer(LState state, LStackFrame stackFrame) {
			ILValue counter = state.Stack[stackFrame.FrameBase + a];
			ILValue limit = state.Stack[stackFrame.FrameBase + a + 1];
			ILValue step = state.Stack[stackFrame.FrameBase + a + 2];

			state.Stack[stackFrame.FrameBase + a] = ArithmeticHelper.Add(counter, step)
				?? throw new LException($"Attempt to add a '{counter.Type}' with a '{step.Type}'");
			var res = ArithmeticHelper.LessThanOrEqual(counter, limit);

			if (res == TernaryBool.Unknown)
				throw new LException($"attempt to compare ${counter.Type} with ${limit.Type}");

			if (res == TernaryBool.True) {
				stackFrame.PC -= (int)bx;
			} else {
				stackFrame.PC += 1;
			}
		}
	}
}
