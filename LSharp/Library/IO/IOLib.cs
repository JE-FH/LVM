using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Library.IO {
	public static class IOLib {
		public static void LoadLibrary(LState state) {
			state.EnvironmentTable.SetValue(new LString("print"), new CSClosure(Print));
		}

		private static IEnumerable<CallYield> Print(LState state, CSStackFrame stackFrame) {
			for (int i = 0; i < stackFrame.Args.Length; i++) {
				Console.Write(stackFrame.Args[i]);
			}
			return [];
		}

	}
}
