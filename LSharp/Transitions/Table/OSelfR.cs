using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	internal class OSelfR(byte a, byte b, byte c) : MetaMethodTransition(a)
	{
		public override bool NormalOrMetaAccess(LState state, LStackFrame stackFrame)
		{
			state.Stack[stackFrame.FrameBase + a + 1] = 
				state.Stack[stackFrame.FrameBase + b];

			var table = (LTable)state.Stack[stackFrame.FrameBase + b];
			var key = (LString)state.Stack[stackFrame.FrameBase + c];
			var val = table.GetValue(key);

			if (val is LNil)
				return CallMetaMethod(state, stackFrame, [table, key], MetaMethodTag.Index);

			state.Stack[stackFrame.FrameBase + a] = val;
			return false;
		}
	}
}
