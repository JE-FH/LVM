using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LVMTest
{
	internal static class Extensions
	{
		public static byte[] ReadAll(this Stream stream)
		{
			using var memoryStream = new MemoryStream();
			stream.CopyTo(memoryStream);
			return memoryStream.ToArray();
		}
	}
}
