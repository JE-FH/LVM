using LSharp.LTypes;
using LSharp.Transitions.MetaMethod;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Transitions.Arithmetic
{
	public class OLen(byte a, byte b) : ITransition
	{
		public void Transfer(LState state, LStackFrame stackFrame)
		{
			MetaMethodHelper.UnaryMM(
				state, stackFrame, MetaMethodTag.Len,
				(val) => {
					if (val is LTable table && table.GetMetaMethod(MetaMethodTag.Len) == null) {
						return new LInteger(table.GetLength());
					}
					if (val is LString str) {
						return new LInteger(str.Value.Length);
					}
					return null;
				},
				() => state.Stack[stackFrame.FrameBase + b],
				(val) => state.Stack[stackFrame.FrameBase + a] = val
			);
		}
	}
}
