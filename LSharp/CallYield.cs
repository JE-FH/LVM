using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp
{
	public class CallYield(IClosure closure, ILValue[] parameters)
	{
		public ILValue[] Parameters => parameters;
		public ILValue[] Return { get; set; } = [];
		public IClosure Closure => closure;
	}
}
