using LSharp.LTypes;

namespace LSharp.Transitions.CallStack
{
	public class OReturn1(byte A) : ITransition
    {
        public void Transfer(LuaState state, LStackFrame stackFrame)
        {
            state.CallStack.RemoveAt(state.CallStack.Count - 1);
            if (state.CallStack.Count == 0)
            {
                return;
            }

            var lowerStackFrame = state.CallStack[^1];
            if (lowerStackFrame is LStackFrame)
            {
                state.Stack[stackFrame.FrameBase] = state.Stack[stackFrame.FrameBase + A];

                state.Stack.SetTop(lowerStackFrame.FrameTop);

                for (int i = 1; i < lowerStackFrame.MandatoryReturnCount; i++)
                {
                    state.Stack[stackFrame.FrameBase + i] = LNil.Instance;
                }
            }
            else if (lowerStackFrame is CSStackFrame lowerCSStackFrame)
            {
                lowerCSStackFrame.ActionGenerator.Current.Return = [state.Stack[stackFrame.FrameBase]];
                state.Stack.SetTop(lowerCSStackFrame.FrameTop);
            }
        }
    }
}
