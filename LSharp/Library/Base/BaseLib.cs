using LSharp.Helpers;
using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Library.Base
{
	public class BaseLib
	{
		public static void LoadLibrary(LState state)
		{
			state.EnvironmentTable.SetValue(new LString("assert"), new CSClosure(Assert));
		}

		private static IEnumerable<CallYield> Assert(LState state, CSStackFrame stackFrame)
		{
			if (stackFrame.Args.Length == 0)
			{
				throw new LException("Bad argument #1 to 'assert' (value expected)");
			}

			if (!ArithmeticHelper.IsTruthy(stackFrame.Args[0]))
			{
				if (stackFrame.Args.Length > 1)
				{
					throw new LException(stackFrame.Args[1]);
				}
				else
				{
					throw new LException("assertion failed!");
				}
			}
			stackFrame.ReturnValues = [stackFrame.Args[0]];
			return [];
		}
	}
}
