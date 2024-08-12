using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.LTypes {
	public interface IAnyNumber<T> : ILValue where T : INumber<T> {
		T Value { get; }
	}
}
