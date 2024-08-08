using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	public class OGetTable(byte a, byte b, byte c) : MetaMethodTransition(a)
	{
		public override bool NormalOrMetaAccess(LState state, LStackFrame stackFrame)
		{
			var lookUpVal = state.Stack[stackFrame.FrameBase + c];
			var table = (LTable)state.Stack[stackFrame.FrameBase + b];
			var val = table.GetValue(lookUpVal);

			if (val is LNil)
				return CallMetaMethod(state, stackFrame, [table, lookUpVal], MetaMethodTag.Index);
			
			state.Stack[stackFrame.FrameBase + a] = val;
			return false;
		}
	}
}
