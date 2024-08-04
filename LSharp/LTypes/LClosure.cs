using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.LTypes
{
	public class LClosure(LPrototype prototype, UpValue[] upValues) : IClosure, ILValue
	{
		public LPrototype Prototype => prototype;
		public UpValue[] UpValues => upValues;
		public int ParamCount => prototype.ParamCount;
		public int MinStackSize => prototype.MinStackSize;

		public uint LHash => unchecked((uint)RuntimeHelpers.GetHashCode(this));

		public string Type => "function";

		public bool LEqual(ILValue other) =>
			ReferenceEquals(this, other);
	}
}
