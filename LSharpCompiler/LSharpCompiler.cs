using LSharp;
using System.Runtime.InteropServices;

namespace LSharpCompiler
{
	internal class NativeLuaState() : SafeHandle(0, true)
	{
		public override bool IsInvalid => handle == IntPtr.Zero;

		protected override bool ReleaseHandle()
		{
			lua.lua_close(handle);
			return true;
		}

		public static unsafe int StreamWriter(NativeLuaState L, void* dataPointer, nuint size, nint userData)
		{
			var streamHandle = GCHandle.FromIntPtr(userData);
			try
			{
				if (streamHandle.Target is Stream stream)
				{
					stream.Write(new Span<byte>(dataPointer, (int)size));
					return 0;
				}
				else
				{
					return 1;
				}
			}
			finally
			{
				streamHandle.Free();
			}
		}

		public static unsafe byte* StreamReader(NativeLuaState L, nint userData, nuint* readAmount)
		{
			var ctxHandle = GCHandle.FromIntPtr(userData);
			if (ctxHandle.Target is not ReadFileContext ctx)
				return null;

			var streamHandle = GCHandle.FromIntPtr(ctx.streamPtr);
			if (streamHandle.Target is not Stream stream)
			{
				ctxHandle.Free();
				return null;
			}

			int amountRead = stream.Read(new Span<byte>(ctx.buffer, ctx.bufferSize));
			if (amountRead <= 0)
			{
				*readAmount = 0;
				streamHandle.Free();
				ctxHandle.Free();
				return null;
			}

			*readAmount = (nuint)amountRead;
			
			streamHandle.Free();
			ctxHandle.Free();
			
			return ctx.buffer;
		}

		public unsafe int Load(Stream input, string chunkName, string mode)
		{
			var buffer = new byte[4096];
			fixed (byte* rawBuffer = buffer)
			{
				GCHandle streamHandle = GCHandle.Alloc(input, GCHandleType.Pinned);
				var context = new ReadFileContext
				{
					buffer = rawBuffer,
					bufferSize = buffer.Length,
					streamPtr = streamHandle.AddrOfPinnedObject()
				};

				GCHandle ctxHandle = GCHandle.Alloc(context, GCHandleType.Pinned);
				
				int res = lua.lua_load(this, &StreamReader, ctxHandle.AddrOfPinnedObject(), chunkName, mode);
				
				streamHandle.Free();
				ctxHandle.Free();
				
				return res;
			}
		}

		public int Dump(Stream output)
		{
			GCHandle handle = GCHandle.Alloc(output, GCHandleType.Pinned);
			try
			{
				unsafe
				{
					return lua.lua_dump(this, &StreamWriter, handle.AddrOfPinnedObject(), 0);
				}
			}
			finally
			{
				handle.Free();
			}
		}
	}

	unsafe struct ReadFileContext
	{
		public required nint streamPtr;
		public required byte* buffer;
		public required int bufferSize;
	}

	internal static partial class lua
	{
		[LibraryImport(nameof(lua), EntryPoint = nameof(luaL_newstate))]
		internal static partial NativeLuaState luaL_newstate();

		[LibraryImport(nameof(lua), EntryPoint = nameof(lua_close))]
		internal static partial void lua_close(IntPtr state);

		[LibraryImport(nameof(lua), EntryPoint = nameof(lua_dump))]
		internal static unsafe partial int lua_dump(NativeLuaState L, delegate* managed<NativeLuaState, void*, nuint, nint, int> luaWriter, nint userData, int strip);

		[LibraryImport(
			nameof(lua),
			EntryPoint = nameof(lua_load),
			StringMarshalling = StringMarshalling.Utf8
		)]
		internal static unsafe partial int lua_load(NativeLuaState L, delegate* managed<NativeLuaState, nint, nuint*, byte*> luaReader, nint userData, string chunkName, string mode);
	}

	public class LSharpCompiler : ILuaCompiler
	{
		public Stream Compile(Stream data, string chunkName)
		{
			var state = lua.luaL_newstate();
			var res = state.Load(data, chunkName, "t");
			if (res != 0)
			{
				throw new Exception("Ahhh");
			}

			var stream = new MemoryStream();
			res = state.Dump(stream);
			if (res != 0)
			{
				throw new Exception("adfdfdf");
			}
			return stream;
		}
	}
}
