using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	public class OGetTabUp(byte a, byte b, string kC) : MetaMethodTransition(a)
	{
		public override bool NormalOrMetaAccess(LuaState state, LStackFrame stackFrame)
		{
			var table = (LTable)stackFrame.Closure.UpValues[b].Value;
			var val = table.GetValue(kC);

			if (val is LNil) 
				return CallMetaMethod(state, stackFrame, [table, new LString(kC)], MetaMethodTag.Index);
			
			state.Stack[stackFrame.FrameBase + a] = val;
			return false;

		}
	}
}
