﻿using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.CallStack
{
	public class OCall(byte a, byte b, byte c) : ITransition
    {
        public void Transfer(LuaState state, LStackFrame stackFrame)
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
    }
}
