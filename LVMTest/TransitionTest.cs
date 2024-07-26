using LVM;
using LVM.RuntimeType;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LVMTest
{
	public class TransitionTest(TestFileFixture testFiles) : IClassFixture<TestFileFixture>
	{
		[Fact]
		public void AssignmentTest()
		{
			using Stream fileStream = testFiles.GetTestFile("assignment.out");

			var parser = new LuaCReader();
			var parsed = parser.ParseLuaCFile(fileStream);

			var state = new LuaState();
			state.RunFunction(parsed);

			Assert.Equal(
				state.envTable.GetValue(LuaString.From("b")),
				new LuaInteger(1)
			);
		}
	}
}
