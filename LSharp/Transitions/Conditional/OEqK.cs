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
	public class OEqK(byte a, ILValue kB, bool k) : EqualityTransition<ILValue, ILValue>(k)
	{
		protected override bool Comparison(ILValue lhs, ILValue rhs)
			=> ArithmeticHelper.Equal(lhs, kB);

		protected override (ILValue, ILValue) GetComparedValues(LState state, LStackFrame stackFrame) =>
			(state.Stack[stackFrame.FrameTop + a], kB);
	}
}
