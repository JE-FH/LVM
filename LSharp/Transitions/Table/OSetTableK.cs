using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	internal class OSetTableK(byte a, byte b, ILValue kC) : MetaMethodTransition(-1)
	{
		public override bool NormalOrMetaAccess(LState state, LStackFrame stackFrame)
		{
			var table = (LTable)state.Stack[stackFrame.FrameBase + a];
			var key = state.Stack[stackFrame.FrameBase + b];
			return UpsertTable(state, stackFrame, table, key, kC);

		}
	}
}
