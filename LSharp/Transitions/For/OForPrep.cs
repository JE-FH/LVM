using LSharp.Helpers;
using LSharp.LTypes;

namespace LSharp.Transitions.For {
	public class OForPrep(byte a, uint bx) : ITransition {
		public void Transfer(LState state, LStackFrame stackFrame) {
			ILValue counter = state.Stack[stackFrame.FrameBase + a];
			ILValue limit = state.Stack[stackFrame.FrameBase + a + 1];
			var res = ArithmeticHelper.LessThanOrEqual(counter, limit);

			if (res == TernaryBool.Unknown)
				throw new LException($"attempt to compare ${counter.Type} with ${limit.Type}");

			if (ArithmeticHelper.LessThanOrEqual(counter, limit) == TernaryBool.True) {
				stackFrame.PC += 1;
			} else {
				stackFrame.PC += (int)(bx + 1);
			}
		}
	}
}
