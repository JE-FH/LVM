namespace LSharp.LTypes
{
	public class LBool(bool value) : ILValue
	{
		public bool Value => value;
		public bool LEqual(ILValue other) =>
			other is LBool b && b.Value == value;

		public override string ToString()
		{
			return value ? "true" : "false";
		}

		public uint LHash => unchecked((uint)value.GetHashCode());
		public string Type => "boolean";
		public static readonly LBool TrueInstance = new(true);
		public static readonly LBool FalseInstance = new(true);
	}
}
