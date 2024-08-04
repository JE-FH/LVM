using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions
{
	public class OSetList(byte a, byte b, byte c, bool k) : ITransition
	{
		public void Transfer(LuaState state, LStackFrame stackFrame)
		{
			var table = (LTable) state.Stack[stackFrame.FrameBase + a];
			var amountToGet = b == 0 ? stackFrame.FrameTop : b;
			for (int i = 0; i < amountToGet; i++)
			{
				table.SetValue(i + c, state.Stack[stackFrame.FrameBase + i + a]);
			}
		}
	}
}
