using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	internal class OSetTableK(byte a, byte b, ILValue kC) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame) {
			MetaMethodHelper.TableSetMM(
				state, stackFrame,
				() => (LTable)state.Stack[stackFrame.FrameBase + a],
				() => state.Stack[stackFrame.FrameBase + b],
				() => kC
			);
		}
	}
}
