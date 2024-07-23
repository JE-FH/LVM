using System.Diagnostics;
using System.Text;

namespace LVM.RuntimeType
{
	[DebuggerDisplay("{Decoded}")]
	public class LuaString(byte[] _value) : IRuntimeValue
	{
		public byte[] value = _value;
		public LuaType TypeName => LuaType.String;

		public uint LuaHash => unchecked((uint)value
			.Aggregate(0, (acc, next) => (acc + next).GetHashCode()));

		public bool LuaEqual(IRuntimeValue other) =>
			other is LuaString otherString
				&& Enumerable.SequenceEqual(value, otherString.value);

		public static LuaString From(string v)
		{
			return new LuaString(Encoding.UTF8.GetBytes(v));
		}

		internal string Decoded => Encoding.UTF8.GetString(value);
	}
}
