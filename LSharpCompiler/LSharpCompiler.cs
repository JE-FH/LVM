using LSharp;
using Shared;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Unicode;

namespace LuaNativeCompiler
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

		public unsafe byte[]? ToString(int index)
		{
			nint size;
			byte* buffer = lua.lua_tolstring(this, index, &size);
			if (buffer == null)
			{
				return null;
			}
			else
			{
				return (new Span<byte>(buffer, (int)size)).ToArray();
			}
		}
	}

	internal static partial class lua
	{
		internal enum StatusCode : int
		{
			LUA_OK = 0,
			LUA_YIELD = 1,
			LUA_ERRRUN = 2,
			LUA_ERRSYNTAX = 3,
			LUA_ERRMEM = 4,
			LUA_ERRERR = 5,
		}

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

		[LibraryImport(nameof(lua), EntryPoint = nameof(lua_tolstring))]
		internal static unsafe partial byte* lua_tolstring(NativeLuaState L, int index, nint* stringSize);

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
			if (res == (int)lua.StatusCode.LUA_ERRSYNTAX)
			{
				var errorMessageBytes = state.ToString(-1);
				if (errorMessageBytes == null)
				{
					throw new LuaSyntaxError(
						"unknown",
						"lua_load failed with LUA_ERRSYNTAX, however no error message was given"
					);
				}
				var message = Encoding.UTF8.GetString(errorMessageBytes);
				throw new LuaSyntaxError(message, $"syntax error {message}");
			}
			else if (res != (int)lua.StatusCode.LUA_OK)
			{
				throw new Exception("Unknown compilation error");
			}

			var stream = new MemoryStream();
			res = state.Dump(stream);
			
			stream.Seek(0, SeekOrigin.Begin);

			if (res != 0)
			{
				throw new Exception("Dump function failed");
			}

			return stream;
		}
	}
}
