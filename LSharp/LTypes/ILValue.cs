using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.LTypes
{
	public interface ILValue
	{
		public bool LEqual(ILValue other);
		public uint LHash { get; }
		public string Type { get; }
	}
}
