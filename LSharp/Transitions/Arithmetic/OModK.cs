using LSharp.Helpers;
using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.Arithmetic {
	public class OModK(byte a, byte b, LNumber kC) : ITransition {
		public void Transfer(LState state, LStackFrame stackFrame) {
			var lhs = state.Stack[stackFrame.FrameBase + b];
			var res = ArithmeticHelper.Mod(lhs, kC);
			if (res is not null) {
				state.Stack[stackFrame.FrameBase + a] = res;
				stackFrame.PC += 2;
				return;
			}
			stackFrame.PC += 1;
		}
	}
}
