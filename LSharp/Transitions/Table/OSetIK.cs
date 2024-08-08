using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	public class OSetIK(byte a, LInteger b, ILValue kC) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame) {
			MetaMethodHelper.TableSet(
				state, stackFrame,
				() => (LTable)state.Stack[stackFrame.FrameBase + a],
				() => b,
				() => kC
			);
		}
	}
}
