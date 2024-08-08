using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	public class OSetTabUpR(byte a, LString kB, byte c) : MetaMethodTransition(-1)
	{
		public override bool NormalOrMetaAccess(LState state, LStackFrame stackFrame)
		{
			var table = (LTable)stackFrame.Closure.UpValues[a].Value;
			var val = state.Stack[stackFrame.FrameBase + c];
			return UpsertTable(state, stackFrame, table, kB, val);
		}
	}
}
