namespace LSharp.Transitions.For
{
	public class OTForPrep(byte a, uint bx) : ITransition
    {
        public void Transfer(LState state, LStackFrame stackFrame)
        {
            stackFrame.PC += (int)bx + 1;
        }
    }
}
