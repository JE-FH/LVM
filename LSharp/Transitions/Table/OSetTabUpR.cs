using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;

namespace LSharp.Transitions.Table
{
	public class OSetTabUpK(byte a, LString kB, ILValue kC) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame) {
			MetaMethodHelper.TableSet(
				state, stackFrame,
				() => (LTable)stackFrame.Closure.UpValues[a].Value,
				() => kB,
				() => kC
			);
		}
	}
}
