using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.LTypes
{
	[DebuggerDisplay("{Value}")]
	public class LInteger(long value) : ILValue, IAnyNumber<long> {
		public long Value => value;

		public uint LHash => unchecked((uint)value.GetHashCode());

		public string Type => "number";

		public bool LEqual(ILValue other) =>
			other is LInteger lInteger && lInteger.Value == value;

		public override string ToString()
		{
			return value.ToString();
		}
	}
}
