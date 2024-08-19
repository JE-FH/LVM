using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharpTest
{
	public class TableTest
	{
		public TableTest()
		{

		}

		private ILValue ObjectToRuntimeValue(object value)
		{
			return value switch
			{
				"nil" => LNil.Instance,
				string s => new LString(s),
				int n => new LInteger(n),
				long n => new LInteger(n),
				double n => new LNumber(n),
				bool b => new LBool(b),
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
		[InlineData(1, 2, 3, 4, 5)]
		[InlineData(1, 3, 5, 2, 4)]
		[InlineData(5, 4, 3, 2, 1)]
		public void LuaTable_InsertionGet(params object[] keys)
		{
			long i = 0;
			LTable table = new();
			foreach (var key in keys)
			{
				table.SetValue(ObjectToRuntimeValue(key), new LInteger(i));
				i++;
			}

			i = 0;
			foreach (var key in keys)
			{
				Assert.Equal(i, Assert.IsType<LInteger>(table.GetValue(ObjectToRuntimeValue(key))).Value);
				i++;
			}
		}

		[Theory]
		[InlineData("someString", "otherString")]
		[InlineData(2113, 53, 3301)]
		[InlineData(5, 20, 5021)]
		[InlineData(true, false)]
		[InlineData(32.431, 5214.52, 6023.42)]
		[InlineData("nil")]
		[InlineData("someString", "otherString", 2113, 53, 3301, 5, 20, 5021, true, false, 32.431, 5214.52, 6023.42, "nil")]
		[InlineData(1, 2, 3, 4, 5)]
		[InlineData(1, 3, 5, 2, 4)]
		[InlineData(5, 4, 3, 2, 1)]
		public void LuaTable_Update(params object[] keys)
		{
			long i = 0;
			LTable table = new();
			foreach (var key in keys)
			{
				table.SetValue(ObjectToRuntimeValue(key), new LInteger(i));
				i++;
			}

			var startI = i;

			foreach (var key in keys.Reverse())
			{
				table.SetValue(ObjectToRuntimeValue(key), new LInteger(i));
				i++;
			}

			foreach (var key in keys.Reverse())
			{
				Assert.Equal(startI, Assert.IsType<LInteger>(table.GetValue(ObjectToRuntimeValue(key))).Value);
				startI++;
			}
		}

		[Theory]
		[InlineData("someString", "otherString")]
		[InlineData(2113, 53, 3301)]
		[InlineData(5, 20, 5021)]
		[InlineData(true, false)]
		[InlineData(32.431, 5214.52, 6023.42)]
		[InlineData("nil")]
		[InlineData("someString", "otherString", 2113, 53, 3301, 5, 20, 5021, true, false, 32.431, 5214.52, 6023.42, "nil")]
		[InlineData(1, 2, 3, 4, 5)]
		[InlineData(1, 3, 5, 2, 4)]
		[InlineData(5, 4, 3, 2, 1)]
		public void LuaTable_MaybeUpdate(params object[] keys)
		{
			long i = 0;
			LTable table = new();
			foreach (var key in keys)
			{
				table.SetValue(ObjectToRuntimeValue(key), new LInteger(i));
				i++;
			}

			var startI = i;

			foreach (var key in keys.Reverse())
			{
				var ctx = table.HasValueMaybeUpdate(ObjectToRuntimeValue(key));
				table.UpdateValue(ctx, new LInteger(i));
				i++;
			}

			foreach (var key in keys.Reverse())
			{
				Assert.Equal(startI, Assert.IsType<LInteger>(table.GetValue(ObjectToRuntimeValue(key))).Value);
				startI++;
			}
		}

		[Theory]
		[InlineData(5, 1, 2, 3, 4, 5)]
		[InlineData(4, 0, 1, 2, 3, 4)]
		[InlineData(5, 5, 4, 3, 2, 1)]
		[InlineData(0, 5, 4, 3, 2)]
		public void LuaTable_Length(int expectedLength, params object[] keys)
		{
			long i = 0;
			LTable table = new();
			foreach (var key in keys)
			{
				table.SetValue(ObjectToRuntimeValue(key), new LInteger(i));
				i++;
			}

			Assert.Equal(expectedLength, table.GetLength());
		}
	}
}
