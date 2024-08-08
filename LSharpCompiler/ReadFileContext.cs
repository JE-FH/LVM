using System.Runtime.InteropServices;

namespace LuaNativeCompiler
{
	unsafe struct ReadFileContext(byte* buffer, int bufferSize)
	{
		private readonly byte* _buffer = buffer;
		private readonly int _bufferSize = bufferSize;
		private bool _called = false;
		public static unsafe byte* StreamReader(nint L, nint userData, nuint* readAmount)
		{
			var ctxHandle = GCHandle.FromIntPtr(userData);
			if (ctxHandle.Target is not ReadFileContext ctx)
				return null;

			if (ctx._called)
			{
				ctxHandle.Free();
				*readAmount = 0;
				return null;
			}
			ctx._called = true;

			*readAmount = (nuint)ctx._bufferSize;
			var pBuffer = ctx._buffer;

			ctxHandle.Target = ctx;

			return pBuffer;
		}
	}
}
