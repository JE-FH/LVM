using LuaByteCode;
using LuaByteCode.LuaCConstructs;
using LuaByteCode.LuaCConstructs.Types;
using LVMTest;
using System.Text;

namespace LuaByteCodeTest
{
	public class ParserTests(TestFileFixture testFiles) : IClassFixture<TestFileFixture>
	{
		[Fact]
		public void ParseHeader_Test()
		{
			LuaByteCodeReader reader = new();

			using var testFileStream = testFiles.GetTestFile("simple.out");

			var header = reader.ParseHeader(testFileStream);

			Assert.Equal(8, header.IntegerSize);
			Assert.Equal(8, header.NumberSize);
			Assert.Equal(4, header.InstructionSize);
			Assert.Equal(0, header.LuaFormat);
			Assert.Equal(0x54, header.LuaVersion);
		}

		[Theory]
		[InlineData(1, 0x00, typeof(InvalidFileSignatureException))]
		[InlineData(4, 0x51, typeof(UnsupportedLuaCVersionException))]
		[InlineData(5, 0x02, typeof(UnsupportedLuaCFormatException))]
		[InlineData(9, 0x69, typeof(IncorrectLuaCDataException))]
		[InlineData(0x0C, 0x69, typeof(UnexpectedInstructionSizeException))]
		[InlineData(0x0D, 0xDE, typeof(UnexpectedIntegerSizeException))]
		[InlineData(0x0E, 0xAD, typeof(UnexpectedNumberSizeException))]
		[InlineData(0x13, 0xBE, typeof(UnexpectedCheckIntegerException))]
		[InlineData(0x1A, 0xEF, typeof(UnexpectedCheckNumberException))]
		public void ParseHeader_TestInvalidFiles(int indexToChange, byte newValue, Type expectedException)
		{
			var testFile = testFiles.GetTestFileBytes("simple.out");
			testFile[indexToChange] = newValue;

			LuaByteCodeReader reader = new();

			Assert.Throws(
				expectedException,
				() =>
				{
					using MemoryStream memoryStream = new(testFile);
					reader.ParseHeader(memoryStream);
				}
			);
		}

		[Theory]
		[InlineData(1)]
		[InlineData(4)]
		[InlineData(5)]
		[InlineData(9)]
		[InlineData(0x0C)]
		[InlineData(0x0D)]
		[InlineData(0x0E)]
		[InlineData(0x13)]
		[InlineData(0x1A)]
		public void ParseHeader_TestUnexpectedEnd(int length)
		{
			var testFile = testFiles.GetTestFileBytes("simple.out")[0..length].ToArray();
			LuaByteCodeReader reader = new();

			Assert.Throws<UnexpectedEndOfStreamException>(
				() =>
				{
					using MemoryStream memoryStream = new(testFile);
					reader.ParseHeader(memoryStream);
				}
			);
		}

		[Fact]
		public void ParseHeader_ExactLength()
		{
			var testFile = testFiles.GetTestFileBytes("simple.out")[0..0x1F].ToArray();
			LuaByteCodeReader reader = new();

			using MemoryStream memoryStream = new(testFile);
			reader.ParseHeader(memoryStream);
		}

		[Fact]
		public void ParseFunction_Test()
		{
			var testFile = testFiles.GetTestFileBytes("simple.out")[0x20..].ToArray();
			LuaByteCodeReader reader = new();

			using MemoryStream memoryStream = new(testFile);
			var parsed = reader.ParseFunction(memoryStream);

			Assert.Equal(Encoding.UTF8.GetBytes("@test.lua"), parsed.Source);
			Assert.Equal(0, parsed.LineDefined);
			Assert.Equal(0, parsed.LastLineDefined);
			Assert.Equal(0, parsed.NumParams);
			Assert.True(parsed.IsVarArg);
			Assert.Equal(2, parsed.MaxStackSize);
			Assert.Equal(15, parsed.Code.Length);
			Assert.Equal(4, parsed.Constants.Length);
			Assert.Equal(Encoding.UTF8.GetBytes("a"), Assert.IsType<StringConstant>(parsed.Constants[0]).Data);
			Assert.Equal(Encoding.UTF8.GetBytes("hello"), Assert.IsType<StringConstant>(parsed.Constants[1]).Data);
			Assert.Equal(Encoding.UTF8.GetBytes("print"), Assert.IsType<StringConstant>(parsed.Constants[2]).Data);
			Assert.Equal(Encoding.UTF8.GetBytes("b"), Assert.IsType<StringConstant>(parsed.Constants[3]).Data);
			Assert.Empty(parsed.Prototypes);
		}

		[Fact]
		public void InstructionParser_Test()
		{
			var testFile = testFiles.GetTestFile("simple.out");
			LuaByteCodeReader reader = new();

			var parsed = reader.ParseLuaCFile(testFile);

			var instructions = parsed.TopLevelFunction.Code
				.Select(x => x.OpCode);

			Assert.Equal(instructions.ToArray(), [
				InstructionEnum.VarArgPrep,
				InstructionEnum.SetTabUp,
				InstructionEnum.GetTabUp,
				InstructionEnum.GetTabUp,
				InstructionEnum.Call,
				InstructionEnum.GetTabUp,
				InstructionEnum.SetTabUp,
				InstructionEnum.GetTabUp,
				InstructionEnum.LoadK,
				InstructionEnum.Concat,
				InstructionEnum.SetTabUp,
				InstructionEnum.GetTabUp,
				InstructionEnum.GetTabUp,
				InstructionEnum.Call,
				InstructionEnum.Return
			]);
			Assert.True(true);
		}

		[Theory]
		[InlineData(17, InstructionEnum.Call, 6, false, 1, 5)]
		[InlineData(59, InstructionEnum.Return, 13, true, 1, 1)]
		public void InstructionParser_TestABC(int pc, InstructionEnum expectedOpcode, byte expectedA, bool expectedK, byte expectedB, byte expectedC)
		{
			var testFile = testFiles.GetTestFile("markov-chain.out");
			LuaByteCodeReader reader = new();

			var parsed = reader.ParseLuaCFile(testFile);
			var instruction = parsed.TopLevelFunction.Code[pc];

			Assert.Equal(expectedOpcode, instruction.OpCode);

			Assert.Equal(expectedA, instruction.A);
			Assert.Equal(expectedK, instruction.K);
			Assert.Equal(expectedB, instruction.B);
			Assert.Equal(expectedC, instruction.C);
		}

		[Fact]
		public void InstructionParser_TestIABx()
		{
			var testFile = testFiles.GetTestFile("markov-chain.out");
			LuaByteCodeReader reader = new();

			var parsed = reader.ParseLuaCFile(testFile);
			var instruction = parsed.TopLevelFunction.Code[63];

			Assert.Equal(InstructionEnum.LoadK, instruction.OpCode);

			Assert.Equal(15, instruction.A);
			Assert.Equal(11u, instruction.Bx);
		}

		[Fact]
		public void InstructionParser_TestIAsBx()
		{
			var testFile = testFiles.GetTestFile("markov-chain.out");
			LuaByteCodeReader reader = new();

			var parsed = reader.ParseLuaCFile(testFile);
			var instruction = parsed.TopLevelFunction.Code[40];

			Assert.Equal(InstructionEnum.LoadI, instruction.OpCode);

			Assert.Equal(6, instruction.A);
			Assert.Equal(1, instruction.SBx);
		}

		[Fact]
		public void InstructionParser_TestIAx()
		{
			var testFile = testFiles.GetTestFile("markov-chain.out");
			LuaByteCodeReader reader = new();

			var parsed = reader.ParseLuaCFile(testFile);
			var instruction = parsed.TopLevelFunction.Code[12];

			Assert.Equal(InstructionEnum.ExtraArg, instruction.OpCode);

			Assert.Equal(0u, instruction.Ax);
		}

		[Fact]
		public void InstructionParser_TestIsJ()
		{
			var testFile = testFiles.GetTestFile("markov-chain.out");
			LuaByteCodeReader reader = new();

			var parsed = reader.ParseLuaCFile(testFile);
			var instruction = parsed.TopLevelFunction.Code[58];

			Assert.Equal(InstructionEnum.Jmp, instruction.OpCode);

			Assert.Equal(1, instruction.SJ);
		}
	}
}