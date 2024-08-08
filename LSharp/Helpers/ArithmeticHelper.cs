using LSharp.LTypes;

namespace LSharp.Helpers
{
	public static class ArithmeticHelper
	{
		public static ILValue? Add(ILValue valueA, ILValue valueB)
		{
			return (valueA, valueB) switch
			{
				(LInteger a, LInteger b) => new LInteger(a.Value + b.Value),
				(LInteger a, LNumber b) => new LNumber(a.Value + b.Value),
				(LNumber a, LNumber b) => new LNumber(a.Value + b.Value),
				(LNumber a, LInteger b) => new LNumber(a.Value + b.Value),
				_ => null
			};
		}

		

		public static ILValue? Sub(ILValue valueA, ILValue valueB)
		{
			return (valueA, valueB) switch
			{
				(LInteger a, LInteger b) => new LInteger(a.Value - b.Value),
				(LInteger a, LNumber b) => new LNumber(a.Value - b.Value),
				(LNumber a, LNumber b) => new LNumber(a.Value - b.Value),
				(LNumber a, LInteger b) => new LNumber(a.Value - b.Value),
				_ => null
			};
		}

		public static ILValue? Mul(ILValue valueA, ILValue valueB)
		{
			return (valueA, valueB) switch
			{
				(LInteger a, LInteger b) => new LInteger(a.Value * b.Value),
				(LInteger a, LNumber b) => new LNumber(a.Value * b.Value),
				(LNumber a, LNumber b) => new LNumber(a.Value * b.Value),
				(LNumber a, LInteger b) => new LNumber(a.Value * b.Value),
				_ => null
			};
		}
		public static ILValue? Div(ILValue valueA, ILValue valueB)
		{
			return (valueA, valueB) switch
			{
				(LInteger a, LInteger b) => new LInteger(a.Value / b.Value),
				(LInteger a, LNumber b) => new LNumber(a.Value / b.Value),
				(LNumber a, LNumber b) => new LNumber(a.Value / b.Value),
				(LNumber a, LInteger b) => new LNumber(a.Value / b.Value),
				_ => null
			};
		}

		public static ILValue? Mod(ILValue valueA, ILValue valueB)
		{
			return (valueA, valueB) switch
			{
				(LInteger a, LInteger b) => new LInteger(a.Value % b.Value),
				(LInteger a, LNumber b) => new LNumber(a.Value % b.Value),
				(LNumber a, LNumber b) => new LNumber(a.Value % b.Value),
				(LNumber a, LInteger b) => new LNumber(a.Value % b.Value),
				_ => null
			};
		}

		public static ILValue? IDiv(ILValue valueA, ILValue valueB)
		{
			return (valueA, valueB) switch
			{
				(LInteger a, LInteger b) => new LInteger(a.Value / b.Value),
				(LInteger a, LNumber b) => new LInteger((long)Math.Floor(a.Value / b.Value)),
				(LNumber a, LNumber b) => new LInteger((long)Math.Floor(a.Value / b.Value)),
				(LNumber a, LInteger b) => new LInteger((long)Math.Floor(a.Value / b.Value)),
				_ => null
			};
		}

		public static bool? LessThan(ILValue valueA, ILValue valueB)
		{
			return (valueA, valueB) switch
			{
				(LInteger a, LInteger b) => a.Value < b.Value,
				(LInteger a, LNumber b) => a.Value < b.Value,
				(LNumber a, LNumber b) => a.Value < b.Value,
				(LNumber a, LInteger b) => a.Value < b.Value,
				_ => null
			};
		}

		public static bool? LessThanOrEqual(ILValue valueA, ILValue valueB)
		{
			return (valueA, valueB) switch
			{
				(LInteger a, LInteger b) => a.Value <= b.Value,
				(LInteger a, LNumber b) => a.Value <= b.Value,
				(LNumber a, LNumber b) => a.Value <= b.Value,
				(LNumber a, LInteger b) => a.Value <= b.Value,
				_ => null
			};
		}

		public static bool? GreaterThanOrEqual(ILValue valueA, ILValue valueB) =>
			!LessThan(valueA, valueB);

		public static bool? GreaterThan(ILValue valueA, ILValue valueB) =>
			!LessThanOrEqual(valueA, valueB);

		public static bool Equal(ILValue valueA, ILValue valueB)
		{
			return valueA.LEqual(valueB);
		}

		public static ILValue GetMetaMethod(ILValue aValue, ILValue bValue, MetaMethodTag metaMethodTag)
		{
			return (aValue, bValue) switch
			{
				(LTable a, LTable b) => a.MetaTable?.GetMetaMethod(metaMethodTag) ?? b.MetaTable?.GetMetaMethod(metaMethodTag) ?? LNil.Instance,
				(LTable a, _) => a.MetaTable?.GetMetaMethod(metaMethodTag) ?? LNil.Instance,
				(_, LTable b) => b.MetaTable?.GetMetaMethod(metaMethodTag) ?? LNil.Instance,
				_ => throw new LException("Invalid arithmetic expression")
			};
		}

		public static bool IsTruthy(ILValue value)
		{
			return value is not LNil or LBool || (value is LBool b && b.Value);
		}
	}
}
