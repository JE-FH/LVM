using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp
{
	public interface IStackFrame
	{
		public int FrameBase { get; }
		public int FrameTop { get; }
		public int MandatoryReturnCount { get; }
	}
}
