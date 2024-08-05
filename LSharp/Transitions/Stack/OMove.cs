using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.Stack
{
    public class OMove(byte a, byte b) : ITransition
    {
        public void Transfer(LuaState state, LStackFrame stackFrame)
        {
            state.Stack[stackFrame.FrameBase + a] = state.Stack[stackFrame.FrameBase + b];
            stackFrame.PC += 1;
        }
    }
}
