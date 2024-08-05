using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.LTypes
{
	[DebuggerDisplay("nil")]
	public class LNil : ILValue
	{
		private LNil() { }
		public static readonly LNil Instance = new();

		public uint LHash => 8729810;

		public string Type => "nil";

		public bool LEqual(ILValue other) => other is LNil;
	}
}
