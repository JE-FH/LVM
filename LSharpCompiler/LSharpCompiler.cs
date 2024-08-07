using LSharp;
using Shared;
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

		public unsafe int Load(Stream input, string chunkName, string mode)
		{
			var streamBuffer = input.ReadAll();

			fixed (byte* pStreamBuffer = streamBuffer)
			{
				var context = new ReadFileContext(pStreamBuffer, streamBuffer.Length);
				GCHandle ctxHandle = GCHandle.Alloc(context, GCHandleType.Pinned);
				int res = lua.lua_load(this, new lua.LuaReader(ReadFileContext.StreamReader), (nint) ctxHandle, chunkName, mode);

				ctxHandle.Free();

				return res;
			}
		}

		public unsafe int Dump(Stream output)
		{
			nint size = 4096;
			var accumulator = Marshal.AllocHGlobal(size);
			var context = new WriteFileContext(accumulator, size);
			GCHandle ctxHandle = GCHandle.Alloc(context, GCHandleType.Pinned);
			int res = lua.lua_dump(this, new lua.LuaWriter(WriteFileContext.StreamWriter), (nint) ctxHandle, 0);

			context = (WriteFileContext) ctxHandle.Target!;
			
			var dataSpan = new Span<byte>((void*)context._accumulator, (int)context._accumulatorEnd);
			
			output.Write(dataSpan);

			Marshal.FreeHGlobal(context._accumulator);
			ctxHandle.Free();
			return res;
		}
	}

	internal static partial class lua
	{
		[LibraryImport(nameof(lua), EntryPoint = nameof(luaL_newstate))]
		internal static partial NativeLuaState luaL_newstate();

		[LibraryImport(nameof(lua), EntryPoint = nameof(lua_close))]
		internal static partial void lua_close(IntPtr state);

		[LibraryImport(nameof(lua), EntryPoint = nameof(lua_dump))]
		internal static unsafe partial int lua_dump(NativeLuaState L, LuaWriter luaWriter, nint userData, int strip);

		[LibraryImport(
			nameof(lua),
			EntryPoint = nameof(lua_load),
			StringMarshalling = StringMarshalling.Utf8
		)]
		internal static unsafe partial int lua_load(NativeLuaState L, LuaReader luaReader, nint userData, string chunkName, string mode);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public unsafe delegate byte* LuaReader(nint L, nint userData, nuint* readAmount);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		public unsafe delegate int LuaWriter(nint L, void* dataPointer, nuint size, nint userData);
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

			stream.Seek(0, SeekOrigin.Begin);

			if (res != 0)
			{
				throw new Exception("adfdfdf");
			}
			return stream;
		}
	}
}
