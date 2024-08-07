using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace LSharpCompiler
{
	internal struct WriteFileContext(IntPtr accumulator, nint accumulatorSize)
	{
		public IntPtr _accumulator = accumulator;
		public nint _accumulatorSize = accumulatorSize;
		public nint _accumulatorEnd = 0;

		public static unsafe int StreamWriter(nint L, void* dataPointer, nuint size, nint userData)
		{
			var ctxHandle = GCHandle.FromIntPtr(userData);
			var dataSpan = new Span<byte>(dataPointer, (int) size);
			if (ctxHandle.Target is WriteFileContext ctx)
			{
				var newEnd = ctx._accumulatorEnd + (nint)size;
				if (ctx._accumulatorSize < newEnd)
				{
					var newSize = ((newEnd / 4096) + 1) * 4096 ;
					if (newSize < newEnd)
					{
						ctxHandle.Target = ctx;
						return 1;
					}
					Marshal.ReAllocHGlobal(ctx._accumulator, newSize);
					ctx._accumulatorSize = newSize;
				}

				var accumulatorSpan = new Span<byte>((void*) ctx._accumulator, (int)ctx._accumulatorSize)[(int)ctx._accumulatorEnd..];
				dataSpan.CopyTo(accumulatorSpan);
				ctx._accumulatorEnd = newEnd;

				ctxHandle.Target = ctx;
				
				return 0;
			}
			else
			{
				return 1;
			}
		}
	}
}
