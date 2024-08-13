using LSharp.Helpers;
using LSharp.LTypes;
using System.Diagnostics;

namespace LSharp.Transitions.For {
	[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
	public class OForPrep(byte a, uint bx) : ITransition {
		public void Transfer(LState state, LStackFrame stackFrame) {
			ILValue init = state.Stack[stackFrame.FrameBase + a];
			ILValue limit = state.Stack[stackFrame.FrameBase + a + 1];
			var res = ArithmeticHelper.LessThanOrEqual(init, limit);

			if (res == TernaryBool.Unknown)
				throw new LException($"attempt to compare ${init.Type} with ${limit.Type}");

			if (ArithmeticHelper.LessThanOrEqual(init, limit) == TernaryBool.True) {
				state.Stack[stackFrame.FrameBase + a + 3] = init;
				stackFrame.PC += 1;
			} else {
				stackFrame.PC += (int)(bx + 1);
			}
		}

		private string GetDebuggerDisplay() =>
			$"{{FORPREP {a} {bx}}}";
	}
}
