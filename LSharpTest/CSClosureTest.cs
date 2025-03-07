﻿using LSharp;
using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharpTest
{
	public class CSClosureTest
	{
		private static CSClosure createConcatClosure()
		{
			return new((state, stackFrame) =>
			{
				if (stackFrame.Args.Length < 1)
				{
					throw new Exception("Less that one argument");
				}
				string acc = "";
				foreach (var arg in stackFrame.Args)
				{
					if (arg is LString sArg)
					{
						acc += sArg.Value;
					}
					else
					{
						throw new Exception("Expected every argument to be string");
					}
				}
				stackFrame.ReturnValues = [new LString(acc)];
				return [];
			});
		}

		private static IEnumerable<CallYield> mainFunc(LState state, CSStackFrame stackFrame)
		{
			ILValue[] someStrings = [new LString("abc"), new LString("bca"), new LString("cba")];
			var concat = (IClosure) state.EnvironmentTable.GetValue(new LString("concat"));
			var call = new CallYield(concat, someStrings);
			yield return call;
			state.EnvironmentTable.SetValue(new LString("val"), call.Return[0]);
		}

		[Fact]
		public void TestCalls()
		{
			LState luaState = new();
			luaState.EnvironmentTable.SetValue(new LString("concat"), createConcatClosure());

			luaState.Call(new CSClosure(mainFunc), []);

			while (luaState.Step()) { }

			Assert.Equal("abcbcacba", Assert.IsType<LString>(luaState.EnvironmentTable.GetValue(new LString("val"))).Value);
		}
	}
}
