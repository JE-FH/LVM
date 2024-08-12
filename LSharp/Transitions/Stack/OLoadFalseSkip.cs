using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.Stack
{
	[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
	public class OLoadFalseSkip(byte a) : ITransition
    {
        public void Transfer(LState state, LStackFrame stackFrame)
        {
            state.Stack[stackFrame.FrameBase + a] = LBool.FalseInstance;
            stackFrame.PC += 2;
        }

        private string GetDebuggerDisplay() =>
            $"LFalseSkip R[{a}]";
	}
}
