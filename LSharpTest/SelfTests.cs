using LSharp;
using LSharp.LTypes;
using LuaByteCode;
using LuaNativeCompiler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TestHelper;

namespace LSharpTest
{
	public class SelfTests(TestFileFixture testFiles) : IClassFixture<TestFileFixture>
	{
		[Fact]
		public void SelfTest()
		{
			using var file = testFiles.GetTestFile("selfTestMain.lua");

			var compiler = new LSharpCompiler();
			using var compiled = compiler.Compile(file, "@selfTestMain.lua");

			var byteCodeReader = new LuaByteCodeReader();
			var byteCode = byteCodeReader.ParseLuaCFile(compiled);

			var lState = new LState();
			var closure = lState.ByteCodeToClosure(byteCode);

			lState.Call(closure, []);

			while (lState.Step()) { }

			var val = lState.EnvironmentTable.GetValue(new LString("c"));

			Assert.Equal(2, Assert.IsType<LInteger>(val).Value);
		}
	}
}
