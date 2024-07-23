using System.Diagnostics;

namespace LVM.RuntimeType
{
	[DebuggerDisplay("nil")]
	public struct LuaNil() : IRuntimeValue
	{
		public readonly LuaType TypeName => LuaType.Nil;

		public readonly uint LuaHash => 9523949;

		public bool LuaEqual(IRuntimeValue other)
		{
			return other is LuaNil;
		}
	}
}
