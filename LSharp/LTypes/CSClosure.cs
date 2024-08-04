using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.LTypes
{
	public class CSClosure(Func<LuaState, CSStackFrame, IEnumerable<CallYield>> action) : IClosure, ILValue
	{
		public int MinStackSize => 0;
		public Func<LuaState, CSStackFrame, IEnumerable<CallYield>> Action => action;

		public uint LHash => unchecked((uint)RuntimeHelpers.GetHashCode(this));

		public string Type => "function";

		public bool LEqual(ILValue other) =>
			ReferenceEquals(this, other);
	}
}
