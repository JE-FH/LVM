namespace LVM.RuntimeType
{
	public class LuaBool(bool _value) : IRuntimeValue
	{
		public bool value = _value;
		public LuaType TypeName => LuaType.Bool;

		public uint LuaHash => unchecked((uint)value.GetHashCode());

		public bool LuaEqual(IRuntimeValue other) =>
			other is LuaBool otherBool &&
				value == otherBool.value;
	}
}
