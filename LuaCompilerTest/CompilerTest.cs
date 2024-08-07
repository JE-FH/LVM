using Shared;
using TestHelper;

namespace LuaCompilerTest
{
	public class CompilerTest(TestFileFixture testFiles) : IClassFixture<TestFileFixture>
	{
		[Theory]
		[InlineData("simple")]
		[InlineData("markov-chain")]
		public void tgt(string baseFileName)
		{
			using var testFile = testFiles.GetTestFile($"{baseFileName}.lua");
			var referenceBytes = testFiles.GetTestFileBytes($"{baseFileName}.bin");

			var compiler = new LSharpCompiler.LSharpCompiler();
			using var outStream = compiler.Compile(testFile, $"@{baseFileName}.lua");

			byte[] compiledBytes = outStream.ReadAll();
			Assert.Equal(referenceBytes, compiledBytes);
		}
	}
}