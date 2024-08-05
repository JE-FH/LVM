using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.Table
{
	public class OGetTable(byte a, byte b, byte c) : MetaMethodTransition(a)
	{
		public override bool NormalOrMetaAccess(LuaState state, LStackFrame stackFrame)
		{
			var lookUpVal = state.Stack[stackFrame.FrameBase + c];
			var table = (LTable)state.Stack[stackFrame.FrameBase + b];
			var val = table.GetValue(lookUpVal);
			
			if (val is not LNil)
			{
				state.Stack[stackFrame.FrameBase + a] = val;
				stackFrame.PC += 1;
				return false;
			}
			
			return CallMetaMethod(state, stackFrame, [table, lookUpVal], MetaMethodTag.Index);
		}
	}
}
