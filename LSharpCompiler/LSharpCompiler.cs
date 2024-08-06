using LSharp;
using System.Runtime.InteropServices;

namespace LSharpCompiler
{
	internal class NativeLuaState() : SafeHandle(0, true)
	{
		public override bool IsInvalid => handle == IntPtr.Zero;

		protected override bool ReleaseHandle()
		{
			throw new NotImplementedException();
		}

		public unsafe static int StreamWriter(NativeLuaState L, void* dataPointer, nuint size, nint userData)
		{
			var streamHandle = GCHandle.FromIntPtr(userData);
			try
			{
				if (streamHandle.Target is Stream stream)
				{
					stream.Write(new Span<byte>(dataPointer, (int) size));
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

		public void Dump(Stream output)
		{
			GCHandle handle = GCHandle.Alloc(output, GCHandleType.Pinned);
			try
			{
				unsafe
				{
					lua.LuaDump(this, &StreamWriter, handle.AddrOfPinnedObject(), 0);
				}
			} 
			finally {
				handle.Free();
			}
		}
	}

	internal static partial class lua {
		[LibraryImport(nameof(lua), EntryPoint = "luaL_newstate")]
		internal static partial NativeLuaState LuaLNewState();
		[LibraryImport(nameof(lua), EntryPoint = "lua_dump ")]
		internal unsafe static partial int LuaDump(NativeLuaState L, delegate* unmanaged<NativeLuaState, void*, nuint, nint, int> luaWriter, nint userData, int strip);
	}

	public class LSharpCompiler : ILuaCompiler
	{
		public Stream Compile(byte[] data)
		{
			var state = lua.LuaLNewState();
			
		}
	}
}
