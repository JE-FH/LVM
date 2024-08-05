using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp
{
	public class LStackFrame(int stackBase, int stackTop, LClosure closure, int returnCount, ILValue[] extraArgs) : IStackFrame
	{
		public int FrameBase => stackBase;
		public int FrameTop { get; set; } = stackTop;
		public int PC { get; set; } = 0;
		public ILValue[] ExtraArgs => extraArgs;
		public LClosure Closure => closure;
		public int MandatoryReturnCount => returnCount;
		public bool MetaMethodStalled { get; set; } = false;
	}
}
