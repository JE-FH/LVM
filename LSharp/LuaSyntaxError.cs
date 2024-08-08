using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp
{
	public class LuaSyntaxError(string syntaxErrorDescription, string? message) : Exception(message)
	{
		public string SyntaxErrorDescription => syntaxErrorDescription;
	}
}
