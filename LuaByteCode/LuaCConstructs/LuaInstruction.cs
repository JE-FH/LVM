namespace LuaByteCode.LuaCConstructs;

public struct LuaInstruction(Span<byte> bytes)
{
	public readonly byte a = bytes[0];
	public readonly byte b = bytes[1];
	public readonly byte c = bytes[2];
	public readonly byte d = bytes[3];

	public InstructionEnum OpCode => (InstructionEnum)(a & 0b0111_1111u);
	public readonly byte A => unchecked((byte)(
		((a & 0b1000_0000u) >> 7) | ((b & 0b0111_1111u) << 1)
	));
		
	public readonly byte B => c;
	public readonly sbyte SB => unchecked((sbyte)c);
		
	public readonly byte C => d;
	public readonly sbyte SC => unchecked((sbyte)d);

	public readonly bool K => (b & 0b1000_0000u) != 0;
		
	public readonly uint Bx => ((b & 0b1000_0000u) >> 7) |
	                           (((uint)c) << 1) |
	                           (((uint)d) << 9);
		
	public readonly int SBx => unchecked((int)(
		((b & 0b1000_0000u) >> 7) |
		(((uint)c) << 1) |
		(((uint)d) << 9)
	) + 1) - (1 << 16);

	public readonly uint Ax => ((a & 0b1000_0000u) >> 7) |
	                           ((b & 0b1111_1111u) << 1) |
	                           (((uint)c) << 9) |
	                           (((uint)d) << 17);

	public readonly int SJ => unchecked((int)(
		((a & 0b1000_0000u) >> 7) |
		((b & 0b1111_1111u) << 1) |
		(((uint)c) << 9) |
		(((uint)d) << 17)
	) + 1) - (1 << 24);
}