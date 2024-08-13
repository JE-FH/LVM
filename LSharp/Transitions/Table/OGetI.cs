using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;
using System.Diagnostics;

namespace LSharp.Transitions.Table
{
	[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
	internal class OGetI(byte a, byte b, LInteger c) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame) {
			MetaMethodHelper.TableGetMM(
				state, stackFrame,
				() => (LTable)state.Stack[stackFrame.FrameBase + b],
				() => c,
				(val) => state.Stack[stackFrame.FrameBase + a] = val
			);
		}

		private string GetDebuggerDisplay() =>
			$"R[{a}] = R[{b}][{c.Value}]";
	}
}
