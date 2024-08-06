using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	internal class OGetI(byte a, byte b, byte c) : MetaMethodTransition(a)
	{
		public override bool NormalOrMetaAccess(LuaState state, LStackFrame stackFrame)
		{
			var table = (LTable)state.Stack[stackFrame.FrameBase + b];
			var val = table.GetValue(c);

			if (val is LNil)
				return CallMetaMethod(state, stackFrame, [table, new LInteger(c)], MetaMethodTag.Index);
			
			state.Stack[stackFrame.FrameBase + a] = val;
			return false;
		}
	}
}
