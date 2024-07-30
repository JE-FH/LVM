using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LVM.RuntimeType
{
	public class LuaCSClosure<T> : IRuntimeValue 
	{
		public LuaType TypeName => LuaType.Closure;

		public uint LuaHash => unchecked((uint)RuntimeHelpers.GetHashCode(this));

		public bool LuaEqual(IRuntimeValue other)
		{
			return ReferenceEquals(this, other);
		}
	}
}
