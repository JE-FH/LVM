namespace LSharp
{
	public enum MetaMethodTag : byte
	{
		Index,
		NewIndex,
		Gc,
		Mode,
		Len,
		Eq,  /* last tag method with fast access */
		Add,
		Sub,
		Mul,
		Mod,
		Pow,
		Div,
		IDiv,
		BAand,
		BOr,
		BXor,
		ShiftLeft,
		ShiftRight,
		UnaryMinus,
		BNot,
		Lt,
		Le,
		Concat,
		Call,
		Close,
		N
	}
}
