using LSharp.Helpers;
using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.Arithmetic {
	public class OMulK<T>(byte a, byte b, IAnyNumber<T> kC) : ITransition where T : INumber<T> {
		public void Transfer(LState state, LStackFrame stackFrame) {
			ArithmeticHelper.ArithK(state, stackFrame, a, b, kC, ArithmeticHelper.Mul);
		}
	}
}
