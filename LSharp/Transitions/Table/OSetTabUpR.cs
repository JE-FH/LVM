using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	public class OSetTabUpK(byte a, string b, ILValue kC) : MetaMethodTransition(-1)
	{
		public override bool NormalOrMetaAccess(LuaState state, LStackFrame stackFrame)
		{
			var table = (LTable)stackFrame.Closure.UpValues[a].Value;
			var updateCtx = table.HasValueMaybeUpdate(b);

			if (!updateCtx.HasValue)
				return CallMetaMethod(state, stackFrame, [table, new LString(b), kC], MetaMethodTag.NewIndex);
			
			table.UpdateValue(updateCtx, kC);
			return false;
		}
	}
}
