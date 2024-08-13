using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;
using System.Diagnostics;

namespace LSharp.Transitions.Table
{
	[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
	public class OGetTabUp(byte a, byte b, LString kC) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame) {
			MetaMethodHelper.TableGetMM(
				state, stackFrame,
				() => (LTable)stackFrame.Closure.UpValues[b].Value,
				() => kC,
				(val) => state.Stack[stackFrame.FrameBase + a] = val
			);
		}

		private string GetDebuggerDisplay() =>
			$"R[{a}] = U[{b}][\"{kC.Value}\"] {{GETTABUP {a} {b} \"{kC.Value}\"}}";
	}
}
