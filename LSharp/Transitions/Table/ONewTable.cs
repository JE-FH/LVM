using LSharp.LTypes;
using System.Diagnostics;

namespace LSharp.Transitions.Table
{
	[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
	internal class ONewTable(byte a, byte  b, byte c, bool k, uint aX) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame)
		{
			//TODO: Use all the given information
			state.Stack[stackFrame.FrameBase + a] = new LTable();
			stackFrame.PC += 2;
		}

		private string GetDebuggerDisplay() =>
			$"R[{a}] = {{}} {{NEWTABLE {a} {b} {c} {k} {aX}}}";
	}
}
