using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions
{
	public class OMMBin(byte a, byte b, byte c, byte originalA) : ITransition
	{
		public void Transfer(LuaState state, LStackFrame stackFrame)
		{
			if (!stackFrame.MetaMethodCalled)
			{
				IClosure closure = null;
				ILValue[] args = [
					state.Stack[stackFrame.FrameBase + a],
					state.Stack[stackFrame.FrameBase + b]
				];
				state.Stack.SetTop(stackFrame.FrameTop + 1);
				stackFrame.FrameTop += 1;
				state.CallAt(stackFrame.FrameTop - 1, closure, 1, args);
				stackFrame.MetaMethodCalled = true;
			}
			else
			{
				stackFrame.MetaMethodCalled = false;
				state.Stack[stackFrame.FrameBase + originalA] =
					state.Stack[stackFrame.FrameTop - 1];
				stackFrame.FrameTop -= 1;
				state.Stack.SetTop(stackFrame.FrameTop);
			}
		}
	}
}
