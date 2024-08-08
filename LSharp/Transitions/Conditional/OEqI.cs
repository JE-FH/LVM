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
	public class OEqI(byte a, LInteger sB, bool k) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame) {
			MetaMethodHelper.EqualityMM(
				state, stackFrame, k,
				(a, _) => sB.LEqual(a),
				() => (
					state.Stack[stackFrame.FrameBase + a],
					sB
				)
			);
		}
	}
}
