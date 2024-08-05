using LSharp.LTypes;

namespace LSharp.Transitions.For
{
	public class OTForCall(byte a, byte c) : ITransition
    {
        public void Transfer(LuaState state, LStackFrame stackFrame)
        {
            var iteratorClosure = (IClosure)state.Stack[stackFrame.FrameBase + a];
            var iteratorState = state.Stack[stackFrame.FrameBase + a + 1];
            var controlVariable = state.Stack[stackFrame.FrameBase + a + 2];
            state.Stack[stackFrame.FrameBase + a + 4] =
                state.Stack[stackFrame.FrameBase + a];

            state.CallAt(stackFrame.FrameBase + a + 4, iteratorClosure, c,
                [iteratorState, controlVariable]);

            stackFrame.PC += 1;
        }
    }
}
