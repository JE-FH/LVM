using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;
using System.Diagnostics;

namespace LSharp.Transitions.Table
{
	[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
	internal class OSetTableR(byte a, byte b, byte c) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame) {
			MetaMethodHelper.TableSetMM(
				state, stackFrame,
				() => (LTable)state.Stack[stackFrame.FrameBase + a],
				() => state.Stack[stackFrame.FrameBase + b],
				() => state.Stack[stackFrame.FrameBase + c]
			);
		}

		private string GetDebuggerDisplay() =>
			$"R[{a}][R[{b}]] = R[{c}]";
	}
}
