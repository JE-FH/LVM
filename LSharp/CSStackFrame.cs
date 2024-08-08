using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp
{
	public class CSStackFrame : IStackFrame
	{
		private readonly int _frameBase;
		private readonly CSClosure _closure;
		private readonly ILValue[] _args;
		private readonly IEnumerator<CallYield> _actionGenerator;
		private readonly int _returnCount;

		public CSStackFrame(LState luaState, int frameBase, CSClosure closure, int returnCount, ILValue[] args)
		{
			_frameBase = frameBase;
			_closure = closure;
			_args = args;
			ReturnValues = [];
			_returnCount = returnCount;
			_actionGenerator = closure.Action(luaState, this).GetEnumerator();
		}

		public int FrameBase => _frameBase;
		public int FrameTop => _frameBase;
		public ILValue[] Args => _args;
		public CSClosure Closure => _closure;
		public IEnumerator<CallYield> ActionGenerator => _actionGenerator;
		public ILValue[] ReturnValues { get; set; }

		public int MandatoryReturnCount => _returnCount;
	}
}
