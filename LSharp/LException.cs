using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp
{
	public class LException(string message) : Exception(message)
	{
		public List<IStackFrame> StackTraceBack { get; } = [];
	}
}
