using Shared;
using TestHelper;

namespace LuaCompilerTest
{
	public class CompilerTest(TestFileFixture testFiles) : IClassFixture<TestFileFixture>
	{
		[Fact]
		public void tgt()
		{
			using var testFile = testFiles.GetTestFile("simple.lua");

			var compiler = new LSharpCompiler.LSharpCompiler();
			using var outStream = compiler.Compile(testFile, "simple.lua");

			byte[] compiled = outStream.ReadAll();
		}
	}
}