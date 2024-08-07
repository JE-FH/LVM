namespace LSharp
{
	public interface ILuaCompiler
	{
		Stream Compile(Stream data, string chunkName);
	}
}
