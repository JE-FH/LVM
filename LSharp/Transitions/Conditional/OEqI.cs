using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.Conditional
{
	public class OEqI(byte a, LInteger sB, bool k) : EqualityTransition<ILValue, LInteger>(k)
	{
		protected override bool Comparison(ILValue lhs, LInteger rhs)
			=> sB.LEqual(lhs);

		protected override (ILValue, LInteger) GetComparedValues(LState state, LStackFrame stackFrame)
		{
			return (state.Stack[stackFrame.FrameBase + a], sB);
		}
	}
}
