using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.Stack
{
    public class OLoadConstant(byte a, ILValue val) : ITransition
    {
        public void Transfer(LState state, LStackFrame stackFrame)
        {
            state.Stack[stackFrame.FrameBase + a] = val;
            stackFrame.PC += 1;
        }
    }
}
