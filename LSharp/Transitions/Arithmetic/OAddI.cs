using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.Arithmetic
{
	public class OAddI(byte a, byte b, sbyte sC) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame)
		{
			stackFrame.PC += 1;
			var lhs = state.Stack[stackFrame.FrameBase + b];
			if (lhs is LInteger i)
				state.Stack[stackFrame.FrameBase + a] = new LInteger(i.Value + sC);
			else if (lhs is LNumber f)
				state.Stack[stackFrame.FrameBase + a] = new LNumber(f.Value + sC);
			else
				return;
			stackFrame.PC += 1;
		}
	}
}
