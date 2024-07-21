namespace LVM.RuntimeType
{
	public struct LuaInteger(long _value) : IRuntimeValue
	{
		public long value = _value;
		public LuaType TypeName => LuaType.Integer;

		public uint LuaHash => unchecked((uint)value.GetHashCode());

		public bool LuaEqual(IRuntimeValue other) =>
			other is LuaInteger otherNumber
				&& otherNumber.value == value;
	}
}
