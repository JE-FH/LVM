namespace LSharp.Transitions.For
{
	public class OTForPrep(byte a, uint bx) : ITransition
    {
        public void Transfer(LuaState state, LStackFrame stackFrame)
        {
            stackFrame.PC += (int)bx + 1;
        }
    }
}
