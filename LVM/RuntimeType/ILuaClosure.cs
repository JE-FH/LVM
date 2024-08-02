namespace LVM.RuntimeType
{
	public interface ILuaClosure : IRuntimeValue
	{
		public int ParamCount { get; }
	}
}
