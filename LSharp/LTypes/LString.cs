using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.LTypes
{
	[DebuggerDisplay("{Value}")]
	public class LString(string value) : ILValue
	{
		public string Value => value;

		public uint LHash => unchecked((uint)value.GetHashCode());

		public string Type => "string"

		public bool LEqual(ILValue other) =>
			other is LString lString && lString.Value == value;
	}
}
