using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	public class OSetTabUpK(byte a, LString kB, ILValue kC) : MetaMethodTransition(-1)
	{
		public override bool NormalOrMetaAccess(LState state, LStackFrame stackFrame)
		{
			var table = (LTable)stackFrame.Closure.UpValues[a].Value;
			return UpsertTable(state, stackFrame, table, kB, kC);
		}
	}
}
