using LSharp.LTypes;

namespace LSharp.Transitions.CallStack
{
    public class OTailCall(byte a, byte b) : ITransition
    {
        public void Transfer(LuaState state, LStackFrame stackFrame)
        {
            var closure = (IClosure)state.Stack[stackFrame.FrameBase + a];

            var parameterCount = b == 0
                ? stackFrame.FrameTop - (stackFrame.FrameBase + a)
                : b - 1;

            List<ILValue> parameters = [];

            for (int i = 0; i < parameterCount; i++)
            {
                parameters.Add(state.Stack[stackFrame.FrameBase + a + i]);
            }

            state.CallStack.RemoveAt(state.CallStack.Count - 1);

            state.Stack.SetTop(stackFrame.FrameBase);

            state.CallAt(stackFrame.FrameBase, closure, stackFrame.MandatoryReturnCount, [.. parameters]);
        }
    }
}
