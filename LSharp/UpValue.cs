using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp
{
	public class UpValue(ILValue value)
	{
		public ILValue Value { get; set; } = value;
	}
}
