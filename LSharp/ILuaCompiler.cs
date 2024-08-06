using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp
{
	public interface ILuaCompiler
	{
		Stream Compile(byte[] data);
	}
}
