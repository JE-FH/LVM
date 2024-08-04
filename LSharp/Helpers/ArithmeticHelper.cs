using LSharp.LTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LSharp.Helpers
{
	public static class ArithmeticHelper
	{
		public static ILValue Add(ILValue valueA, ILValue valueB)
		{
			return (valueA, valueB) switch
			{
				(LInteger a, LInteger b) => new LInteger(a.Value + b.Value),
				(LInteger a, LNumber b) => new LNumber(a.Value + b.Value),
				(LNumber a, LNumber b) => new LNumber(a.Value + b.Value),
				(LNumber a, LInteger b) => new LNumber(a.Value + b.Value),
				_ => throw new LException($"Attempt to add a '{valueA.Type}' with a '{valueB.Type}'")
			};
		}

		public static ILValue Sub(ILValue valueA, ILValue valueB)
		{
			return (valueA, valueB) switch
			{
				(LInteger a, LInteger b) => new LInteger(a.Value - b.Value),
				(LInteger a, LNumber b) => new LNumber(a.Value - b.Value),
				(LNumber a, LNumber b) => new LNumber(a.Value - b.Value),
				(LNumber a, LInteger b) => new LNumber(a.Value - b.Value),
				_ => throw new LException($"Attempt to add a '{valueA.Type}' with a '{valueB.Type}'")
			};
		}

		public static ILValue Mul(ILValue valueA, ILValue valueB)
		{
			return (valueA, valueB) switch
			{
				(LInteger a, LInteger b) => new LInteger(a.Value * b.Value),
				(LInteger a, LNumber b) => new LNumber(a.Value * b.Value),
				(LNumber a, LNumber b) => new LNumber(a.Value * b.Value),
				(LNumber a, LInteger b) => new LNumber(a.Value * b.Value),
				_ => throw new LException($"Attempt to add a '{valueA.Type}' with a '{valueB.Type}'")
			};
		}
		public static ILValue Div(ILValue valueA, ILValue valueB)
		{
			return (valueA, valueB) switch
			{
				(LInteger a, LInteger b) => new LInteger(a.Value / b.Value),
				(LInteger a, LNumber b) => new LNumber(a.Value / b.Value),
				(LNumber a, LNumber b) => new LNumber(a.Value / b.Value),
				(LNumber a, LInteger b) => new LNumber(a.Value / b.Value),
				_ => throw new LException($"Attempt to add a '{valueA.Type}' with a '{valueB.Type}'")
			};
		}

		public static ILValue Mod(ILValue valueA, ILValue valueB)
		{
			return (valueA, valueB) switch
			{
				(LInteger a, LInteger b) => new LInteger(a.Value % b.Value),
				(LInteger a, LNumber b) => new LNumber(a.Value % b.Value),
				(LNumber a, LNumber b) => new LNumber(a.Value % b.Value),
				(LNumber a, LInteger b) => new LNumber(a.Value % b.Value),
				_ => throw new LException($"Attempt to add a '{valueA.Type}' with a '{valueB.Type}'")
			};
		}

		public static ILValue IDiv(ILValue valueA, ILValue valueB)
		{
			return (valueA, valueB) switch
			{
				(LInteger a, LInteger b) => new LInteger(a.Value / b.Value),
				(LInteger a, LNumber b) => new LInteger((long)Math.Floor(a.Value / b.Value)),
				(LNumber a, LNumber b) => new LInteger((long)Math.Floor(a.Value / b.Value)),
				(LNumber a, LInteger b) => new LInteger((long)Math.Floor(a.Value / b.Value)),
				_ => throw new LException($"Attempt to add a '{valueA.Type}' with a '{valueB.Type}'")
			};
		}

		public static bool LessThan(ILValue valueA, ILValue valueB)
		{
			return (valueA, valueB) switch
			{
				(LInteger a, LInteger b) => a.Value < b.Value,
				(LInteger a, LNumber b) => a.Value < b.Value,
				(LNumber a, LNumber b) => a.Value < b.Value,
				(LNumber a, LInteger b) => a.Value < b.Value,
				_ => throw new LException($"attempt to compare ${valueA.Type} with ${valueB.Type}")
			};
		}

		public static bool LessThanOrEqual(ILValue valueA, ILValue valueB)
		{
			return (valueA, valueB) switch
			{
				(LInteger a, LInteger b) => a.Value <= b.Value,
				(LInteger a, LNumber b) => a.Value <= b.Value,
				(LNumber a, LNumber b) => a.Value <= b.Value,
				(LNumber a, LInteger b) => a.Value <= b.Value,
				_ => throw new LException($"attempt to compare ${valueA.Type} with ${valueB.Type}")
			};
		}

		public static bool GreaterThanOrEqual(ILValue valueA, ILValue valueB) =>
			!LessThan(valueA, valueB);

		public static bool GreaterThan(ILValue valueA, ILValue valueB) =>
			!LessThanOrEqual(valueA, valueB);

		public static bool Equal(ILValue valueA, ILValue valueB)
		{
			return valueA.LEqual(valueB);
		}
	}
}
