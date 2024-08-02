namespace LuaByteCode
{
	public class InvalidFileSignatureException(byte[] readSignature, string? message) : Exception(message)
	{
		public byte[] ReadSignature => readSignature;
	}

	public class UnsupportedLuaCVersionException(byte readVersion, string? message) : Exception(message)
	{
		public byte ReadVersion => readVersion;
	}

	public class UnsupportedLuaCFormatException(byte readFormat, string? message) : Exception(message)
	{
		public byte ReadFormat => readFormat;
	}

	public class IncorrectLuaCDataException(byte[] readLuaCData, string? message) : Exception(message)
	{
		public byte[] ReadLuaCData => readLuaCData;
	}
	public class UnexpectedInstructionSizeException(byte readSize, string? message) : Exception(message)
	{
		public byte ReadSize => readSize;
	}
	public class UnexpectedIntegerSizeException(byte readSize, string? message) : Exception(message)
	{
		public byte ReadSize => readSize;
	}
	public class UnexpectedNumberSizeException(byte readSize, string? message) : Exception(message)
	{
		public byte ReadSize => readSize;
	}

	public class UnexpectedCheckIntegerException(long readInteger, string? message) : Exception(message)
	{
		public long ReadInteger => readInteger;
	}

	public class UnexpectedCheckNumberException(double readNumber, string? message) : Exception(message)
	{
		public double ReadNumber => readNumber;
	}

	public class UnknownConstantTypeException(byte readConstantType, string? message) : Exception(message)
	{
		public double ReadConstantType => readConstantType;
	}

}
