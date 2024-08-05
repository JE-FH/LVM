using LSharp.LTypes;

namespace LSharp.Transitions.CallStack
{
	public class OReturn0() : ITransition
    {
        public void Transfer(LuaState state, LStackFrame stackFrame)
        {
            state.CallStack.RemoveAt(state.CallStack.Count - 1);
            if (state.CallStack.Count == 0)
            {
                return;
            }


            var lowerStackFrame = state.CallStack[^1];

            state.Stack.SetTop(lowerStackFrame.FrameTop);

            if (lowerStackFrame is LStackFrame)
            {
                for (int i = 0; i < lowerStackFrame.MandatoryReturnCount; i++)
                {
                    state.Stack[stackFrame.FrameBase + i] = LNil.Instance;
                }
            }
        }
    }
}
