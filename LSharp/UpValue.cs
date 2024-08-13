using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp
{
	[DebuggerDisplay("[{Value}]")]
	public class UpValue(ILValue value)
	{
		public ILValue Value { get; set; } = value;
	}
}
