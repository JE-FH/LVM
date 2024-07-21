using LVM;
using LVM.RuntimeType;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace LVMTest
{
	public class TableTest
	{
		public TableTest()
		{

		}

		private IRuntimeValue ObjectToRuntimeValue(object value)
		{
			return value switch
			{
				"nil" => new LuaNil(),
				string s => new LuaString(Encoding.UTF8.GetBytes(s)),
				int n => new LuaInteger(n),
				long n => new LuaInteger(n),
				double n => new LuaNumber(n),
				bool b => new LuaBool(b),
				_ => throw new NotImplementedException()
			};
		}

		[Theory]
		[InlineData("someString", "otherString")]
		[InlineData(2113, 53, 3301)]
		[InlineData(5, 20, 5021)]
		[InlineData(true, false)]
		[InlineData(32.431, 5214.52, 6023.42)]
		[InlineData("nil")]
		[InlineData("someString", "otherString", 2113, 53, 3301, 5, 20, 5021, true, false, 32.431, 5214.52, 6023.42, "nil")]
		public void LuaTable_InsertionGet(params object[] keys)
		{
			long i = 0;
			LuaTable table = new();
			foreach (var key in keys) {
				table.SetValue(ObjectToRuntimeValue(key), new LuaInteger(i));
				i++;
			}

			i = 0;
			foreach (var key in keys)
			{
				Assert.Equal(i, Assert.IsType<LuaInteger>(table.GetValue(ObjectToRuntimeValue(key))).value);
				i++;
			}
		}
	}
}
