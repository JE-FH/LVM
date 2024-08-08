using LSharp.LTypes;

namespace LSharp.Helpers
{
	public enum TernaryBool {
		False,
		True,
		Unknown
	}

	public static class ArithmeticHelper
	{
		private static TernaryBool ToTern(bool val) => val ? TernaryBool.True : TernaryBool.False;
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

		public static TernaryBool LessThan(ILValue valueA, ILValue valueB)
		{
			return (valueA, valueB) switch
			{
				(LInteger a, LInteger b) => ToTern(a.Value < b.Value),
				(LInteger a, LNumber b) => ToTern(a.Value < b.Value),
				(LNumber a, LNumber b) => ToTern(a.Value < b.Value),
				(LNumber a, LInteger b) => ToTern(a.Value < b.Value),
				(LString a, LString b) => ToTern(string.Compare(a.Value, b.Value, StringComparison.Ordinal) < 0),
				_ => TernaryBool.Unknown
			};
		}

		public static TernaryBool LessThanOrEqual(ILValue valueA, ILValue valueB)
		{
			return (valueA, valueB) switch
			{
				(LInteger a, LInteger b) => ToTern(a.Value <= b.Value),
				(LInteger a, LNumber b) => ToTern(a.Value <= b.Value),
				(LNumber a, LNumber b) => ToTern(a.Value <= b.Value),
				(LNumber a, LInteger b) => ToTern(a.Value <= b.Value),
				(LString a, LString b) => ToTern(string.Compare(a.Value, b.Value, StringComparison.Ordinal) <= 0),
				_ => TernaryBool.Unknown
			};
		}

		public static TernaryBool GreaterThanOrEqual(ILValue valueA, ILValue valueB) {
			var val = LessThan(valueA, valueB);

			return val switch {
				TernaryBool.True => TernaryBool.False,
				TernaryBool.False => TernaryBool.True,
				TernaryBool.Unknown => TernaryBool.Unknown,
				_ => throw new NotImplementedException()
			};
		}

		public static TernaryBool GreaterThan(ILValue valueA, ILValue valueB) {
			var val = LessThanOrEqual(valueA, valueB);

			return val switch {
				TernaryBool.True => TernaryBool.False,
				TernaryBool.False => TernaryBool.True,
				TernaryBool.Unknown => TernaryBool.Unknown,
				_ => throw new NotImplementedException()
			};
		}

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
