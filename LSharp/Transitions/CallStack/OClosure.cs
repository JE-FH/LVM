﻿using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.CallStack
{
	[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
	public class OClosure(byte a, LPrototype prototype) : ITransition
    {
        public void Transfer(LState state, LStackFrame stackFrame)
        {
            var upValues = prototype.UpValues
                .Select(upValue => upValue.InStack
                    ? state.Stack.GetAsUpValue(stackFrame.FrameBase + upValue.Index)
                    : stackFrame.Closure.UpValues[upValue.Index]
                )
                .ToArray();

            state.Stack[stackFrame.FrameBase + a] = new LClosure(
                prototype,
                upValues
            );

            stackFrame.PC += 1;
        }

        private string GetDebuggerDisplay() =>
            $"R[{a}] = Closure() {{CLOSURE {a}}}";
	}
}
