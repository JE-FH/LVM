using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	internal class OSetTableR(byte a, byte b, byte c) : MetaMethodTransition(-1)
	{
		public override bool NormalOrMetaAccess(LState state, LStackFrame stackFrame)
		{
			var table = (LTable)state.Stack[stackFrame.FrameBase + a];
			var key = state.Stack[stackFrame.FrameBase + b];
			var val = state.Stack[stackFrame.FrameBase + c];
			return UpsertTable(state, stackFrame, table, key, val);

		}
	}
}
