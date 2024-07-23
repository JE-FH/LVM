using System.Diagnostics;

namespace LVM.RuntimeType
{
	[DebuggerDisplay("{value}")]
	public struct LuaNumber(double _value) : IRuntimeValue
	{
		public double value = _value;
		public LuaType TypeName => LuaType.Number;

		public uint LuaHash => unchecked((uint)value.GetHashCode());

		public bool LuaEqual(IRuntimeValue other) =>
			other is LuaNumber otherNumber
				&& value == otherNumber.value;
	}
}
