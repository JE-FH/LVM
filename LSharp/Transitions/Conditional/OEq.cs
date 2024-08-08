using LSharp.Helpers;
using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.Conditional
{
	public class OEq(byte a, byte b, bool k) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame) {
			MetaMethodHelper.EqualityMM(
				state, stackFrame, k,
				ArithmeticHelper.Equal,
				() => (
					state.Stack[stackFrame.FrameBase + a],
					state.Stack[stackFrame.FrameBase + b]
				)
			);
		}
	}
}
