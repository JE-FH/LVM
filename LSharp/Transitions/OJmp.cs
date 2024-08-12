using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions
{
	[DebuggerDisplay("Jmp {sJ}")]
	internal class OJmp(int sJ) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame)
		{
			stackFrame.PC += sJ + 1;
		}
	}
}
