using System.Runtime.CompilerServices;

namespace LVM.RuntimeType
{
	public class LuaClosure : IRuntimeValue
	{
		public LuaValueReference[] upValues;
		public LuaRuntimeProto proto;

		public LuaClosure(LuaRuntimeProto _proto, LuaValueReference[] _upValues)
		{
			if (_proto.upValues.Length != _upValues.Length)
			{
				throw new Exception("Inconsistent upvalue length");
			}
			upValues = _upValues;
			proto = _proto;
		}

		public LuaType TypeName => LuaType.Closure;

		public bool LuaEqual(IRuntimeValue other) =>
			ReferenceEquals(this, other);

		public uint LuaHash => unchecked((uint)RuntimeHelpers.GetHashCode(this));
	}
}
