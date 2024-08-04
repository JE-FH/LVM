using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions
{
	public class ONop : ITransition
	{
		public void Transfer(LuaState state, LStackFrame stackFrame)
		{
			stackFrame.PC += 1;
		}
	}
}
