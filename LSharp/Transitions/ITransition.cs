using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions
{
	public interface ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame);
	}
}
