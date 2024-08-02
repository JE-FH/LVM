using System.Runtime.CompilerServices;

namespace LVM.RuntimeType
{
	public class LuaCsClosure(object handler) : ILuaClosure 
	{
		public LuaType TypeName => LuaType.Closure;

		public uint LuaHash => unchecked((uint)RuntimeHelpers.GetHashCode(this));

		public bool LuaEqual(IRuntimeValue other)
		{
			return ReferenceEquals(this, other);
		}

		public IRuntimeValue[] Call(LuaState state, IRuntimeValue[] args)
		{
			throw new NotImplementedException();
		}

		public int ParamCount => 0;
	}
}
