using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp
{
	public class LException : Exception
	{
		public List<IStackFrame> StackTraceBack { get; } = [];
		public ILValue LuaException { get; }
		public LException(string message) : base(message)
		{
			LuaException = new LString(message);
		}

		public LException(ILValue errorValue) : base(errorValue is LString str ? str.Value : $"(error object is a {errorValue.Type} value)")
		{
			LuaException = errorValue;
		}
	}
}
