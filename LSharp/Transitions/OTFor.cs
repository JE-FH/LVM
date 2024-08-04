using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions
{
	public class OTForPrep(byte a, uint bx) : ITransition
	{
		public void Transfer(LuaState state, LStackFrame stackFrame)
		{
			stackFrame.PC += ((int) bx) + 1;
		}
	}

	public class OTForCall(byte a, byte c) : ITransition
	{
		public void Transfer(LuaState state, LStackFrame stackFrame)
		{
			var iteratorClosure = (IClosure)state.Stack[stackFrame.FrameBase + a];
			var iteratorState = state.Stack[stackFrame.FrameBase + a + 1];
			var controlVariable = state.Stack[stackFrame.FrameBase + a + 2];
			state.Stack[stackFrame.FrameBase + a + 4] =
				state.Stack[stackFrame.FrameBase + a];

			state.CallAt(stackFrame.FrameBase + a + 4, iteratorClosure, c,
				[iteratorState, controlVariable]);

			stackFrame.PC += 1;
		}
	}

	public class OTForLoop(byte a, uint bx) : ITransition
	{
		public void Transfer(LuaState state, LStackFrame stackFrame)
		{
			if (state.Stack[stackFrame.FrameBase + a + 4].LEqual(LNil.Instance))
			{
				stackFrame.PC += 1;
				return;
			}
			state.Stack[stackFrame.FrameBase + a + 2] = state.Stack[stackFrame.FrameBase + a + 4];
			stackFrame.PC -= (int)bx + 1;
		}
	}
}
