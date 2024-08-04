using LSharp.Helpers;
using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions
{
	public class OForPrep(byte a, uint bx) : ITransition
	{
		public void Transfer(LuaState state, LStackFrame stackFrame)
		{
			ILValue counter = state.Stack[stackFrame.FrameBase + a];
			ILValue limit = state.Stack[stackFrame.FrameBase + a + 1];
			if (ArithmeticHelper.LessThanOrEqual(counter, limit))
			{
				stackFrame.PC += 1;
			}
			else
			{
				stackFrame.PC += (int) (bx + 1);
			}
		}
	}

	public class OForLoop(byte a, uint bx) : ITransition
	{
		public void Transfer(LuaState state, LStackFrame stackFrame)
		{
			ILValue counter = state.Stack[stackFrame.FrameBase + a];
			ILValue limit = state.Stack[stackFrame.FrameBase + a + 1];
			ILValue step = state.Stack[stackFrame.FrameBase + a + 2];

			state.Stack[stackFrame.FrameBase + a] = ArithmeticHelper.Add(counter, step);
			if (ArithmeticHelper.LessThanOrEqual(counter, limit))
			{
				stackFrame.PC -= (int) bx;
			}
			else
			{
				stackFrame.PC += 1;
			}
		}
	}
}
