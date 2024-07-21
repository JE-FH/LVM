namespace LVM.RuntimeType
{
	public interface IRuntimeValue
	{
		public LuaType TypeName { get; }
		public bool LuaEqual(IRuntimeValue other);
		public uint LuaHash { get; }
	}
}
