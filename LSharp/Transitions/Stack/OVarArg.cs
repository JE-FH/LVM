using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.Stack
{
    internal class OVarArg(byte a, byte c) : ITransition
    {
        public void Transfer(LuaState state, LStackFrame stackFrame)
        {
            var wantedArguments = c < 0
                ? stackFrame.ExtraArgs.Length
                : c - 1;

            for (int i = 0; i < wantedArguments; i++)
            {
                state.Stack[a + i] = i < stackFrame.ExtraArgs.Length
                    ? stackFrame.ExtraArgs[i]
                    : LNil.Instance;
            }
            stackFrame.PC += 1;
        }
    }
}
