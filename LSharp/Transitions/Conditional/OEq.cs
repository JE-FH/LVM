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
	internal class OEq(byte a, byte b, bool k) : EqualityTransition<ILValue, ILValue>(k)
	{
		protected override bool Comparison(ILValue lhs, ILValue rhs) =>
			ArithmeticHelper.Equal(lhs, rhs);

		protected override (ILValue, ILValue) GetComparedValues(LState state, LStackFrame stackFrame)
		{
			return (
				state.Stack[stackFrame.FrameBase + a],
				state.Stack[stackFrame.FrameBase + b]
			);
		}
	}
}
