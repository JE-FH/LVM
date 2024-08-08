using LSharp.Helpers;
using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.Conditional {
	public class OGtI(byte a, LInteger sB, bool k) : ITransition {
		public void Transfer(LState state, LStackFrame stackFrame) {
			MetaMethodHelper.OrderMM(
				state, stackFrame, k, MetaMethodTag.Le,
				(_, a) => ArithmeticHelper.GreaterThan(a, sB),
				() => (sB, state.Stack[stackFrame.FrameBase + a])
			);
		}
	}
}
