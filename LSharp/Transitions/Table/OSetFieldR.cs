using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;
using System.Diagnostics;

namespace LSharp.Transitions.Table
{
	[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
	public class OSetFieldR(byte a, LString kB, byte c) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame) {
			MetaMethodHelper.TableSetMM(
				state, stackFrame,
				() => (LTable)state.Stack[stackFrame.FrameBase + a],
				() => kB,
				() => state.Stack[stackFrame.FrameBase + c]
			);
		}

		private string GetDebuggerDisplay() =>
			$"R[{a}][\"{kB.Value}\"] = R[{c}]";
	}
}
