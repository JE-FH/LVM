using LSharp;
using LuaNativeCompiler;
using Shared;
using TestHelper;

namespace LuaCompilerTest
{
	public class CompilerTest(TestFileFixture testFiles) : IClassFixture<TestFileFixture>
	{
		[Theory]
		[InlineData("simple")]
		[InlineData("markov-chain")]
		public void CompileAndCompareTest(string baseFileName)
		{
			using var testFile = testFiles.GetTestFile($"{baseFileName}.lua");
			var referenceBytes = testFiles.GetTestFileBytes($"{baseFileName}.bin");

			var compiler = new LSharpCompiler();
			using var outStream = compiler.Compile(testFile, $"@{baseFileName}.lua");

			byte[] compiledBytes = outStream.ReadAll();
			Assert.Equal(referenceBytes, compiledBytes);
		}

		[Fact]
		public void bitch()
		{
			using var testFile = testFiles.GetTestFile("syntax-error.lua");
			var compiler = new LSharpCompiler();
			var error = Assert.Throws<LuaSyntaxError>(() => compiler.Compile(testFile, "@syntax-error.lua"));
			Assert.Equal("syntax-error.lua:1: unfinished string near '\"dfdf;'", error.SyntaxErrorDescription);
		}
	}
}