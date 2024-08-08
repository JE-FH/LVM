using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.UpVal
{
    public class OGetUpVal(byte a, byte b) : ITransition
    {
        public void Transfer(LState state, LStackFrame stackFrame)
        {
            state.Stack[stackFrame.FrameBase + a] = stackFrame.Closure.UpValues[b].Value;
            stackFrame.PC += 1;
        }
    }
}
