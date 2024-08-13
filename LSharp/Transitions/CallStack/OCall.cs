using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.CallStack
{
	[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
	public class OCall(byte a, byte b, byte c) : ITransition
    {
        public void Transfer(LState state, LStackFrame stackFrame)
        {
            var closure = (IClosure)state.Stack[stackFrame.FrameBase + a];
            var newFrameBase = stackFrame.FrameBase + a;

            var parameterCount = b == 0
                ? stackFrame.FrameTop - (stackFrame.FrameBase + a)
                : b - 1;

            List<ILValue> parameters = [];

            for (int i = 0; i < parameterCount; i++)
            {
                parameters.Add(state.Stack[stackFrame.FrameBase + a + i]);
            }

            state.CallAt(newFrameBase, closure, 0, [.. parameters]);

            stackFrame.PC += 1;
        }

        private string GetDebuggerDisplay() =>
            $"R[{a}] ... R[{a + c - 2}] = R[{a}](R[{a + 1}] ... R[{a + b - 1}]) {{CALL {a} {b} {c}}}";
	}
}
