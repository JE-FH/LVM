using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.For
{
	public class OTForLoop(byte a, uint bx) : ITransition
    {
        public void Transfer(LState state, LStackFrame stackFrame)
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
