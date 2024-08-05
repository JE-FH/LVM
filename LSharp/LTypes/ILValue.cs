namespace LSharp.LTypes
{
	public interface ILValue
	{
		public bool LEqual(ILValue other);
		public uint LHash { get; }
		public string Type { get; }
	}
}
