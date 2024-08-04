using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.LTypes
{
	internal class LNumber(double value) : ILValue
	{
		public double Value => value;
		public uint LHash => unchecked((uint)value.GetHashCode());
		public string Type => "number";

		public bool LEqual(ILValue other) =>
			other is LNumber lNumber && lNumber.Value == value;
	}
}
