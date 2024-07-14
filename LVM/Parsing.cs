using System;
using System.Collections.Generic;

namespace LVM
{
	using LuaNumber = double;
	using LuaInteger = long;

	public class InvalidFileSignatureException(byte[] _readSignature, string? message) : Exception(message)
	{
		public byte[] readSignature = _readSignature;
	}

	public class UnsupportedLuaCVersionException(byte _readVersion, string? message) : Exception(message)
	{
		public byte readVersion = _readVersion;
	}

	public class UnsupportedLuaCFormatException(byte _readFormat, string? message) : Exception(message)
	{
		public byte readFormat = _readFormat;
	}

	public class IncorrectLuaCDataException(byte[] _readLuaCData, string? message) : Exception(message)
	{
		public byte[] readLuaCData = _readLuaCData;
	}
	public class UnexpectedInstructionSizeException(byte _readSize, string? message) : Exception(message)
	{
		public byte readSize = _readSize;
	}
	public class UnexpectedIntegerSizeException(byte _readSize, string? message) : Exception(message)
	{
		public byte readSize = _readSize;
	}
	public class UnexpectedNumberSizeException(byte _readSize, string? message) : Exception(message)
	{
		public byte readSize = _readSize;
	}

	public class UnexpectedCheckIntegerException(LuaInteger _readInteger, string? message) : Exception(message)
	{
		public LuaInteger readInteger = _readInteger;
	}

	public class UnexpectedCheckNumberException(LuaNumber _readNumber, string? message) : Exception(message)
	{
		public LuaNumber readNumber = _readNumber;
	}

	public class UnknownConstantTypeException(byte _readConstantType, string? message) : Exception(message)
	{
		public LuaNumber readConstantType = _readConstantType;
	}

	public enum LuaType : byte
	{
		Nil = 0,
		Boolean = 1,
		LightUserData = 2,
		Number = 3,
		String = 4,
		Table = 5,
		Function = 6,
		UserData = 7,
		Thread = 8,
		None = 0xFF
	}

	public enum LuaVariantTag : byte
	{
		LuaVNil = 0,
		LuaVFalse = LuaType.Boolean | (0 << 4),
		LuaVTrue = LuaType.Boolean | (1 << 4),
		LuaVNumInt = LuaType.Number | (0 << 4),
		LuaVNumFlt = LuaType.Number | (1 << 4),
		LuaVShrStr = LuaType.String | (0 << 4),
		LuaVLngStr = LuaType.String | (1 << 4),
	}

	public class LuaCReader()
	{
		private byte[] expectedFileSignature = [0x1b, (byte)'L', (byte)'u', (byte)'a'];
		private byte supportedVersion = 0x54;
		private byte supportedFormat = 0;
		private byte[] expectedLuaCData = [0x19, 0x93, (byte)'\r', (byte)'\n', 0x1a, (byte)'\n'];
		private byte expectedIntegerSize = 8;
		private byte expectedNumberSize = 8;
		private byte instructionSize = 4;
		private long luaCInt = 0x5678;
		private double luaCNumber = 370.5;

		public LuaCHeader ParseHeader(Stream byteStream)
		{
			Span<byte> buffer = stackalloc byte[16];
			Span<byte> fileSignature = byteStream.ReadOrThrow(buffer[..4], "File signature");
			if (!fileSignature.SequenceEqual(expectedFileSignature))
			{
				throw new InvalidFileSignatureException(fileSignature.ToArray(), $"Invalid file signature {fileSignature.ToHumanReadable()}, expected: {fileSignature.ToHumanReadable()}");
			}

			byte version = byteStream.ReadOrThrow("LuaC version");
			if (version != supportedVersion)
			{
				throw new UnsupportedLuaCVersionException(version, $"Unsupported lua version 0x{version:X}, expected: 0x{supportedVersion:X}");
			}

			byte format = byteStream.ReadOrThrow("LuaC format");
			if (format != supportedFormat)
			{
				throw new UnsupportedLuaCFormatException(format, $"Unsupported lua format 0x{format:X}, expected 0x{supportedFormat:X}");
			}

			Span<byte> data = byteStream.ReadOrThrow(buffer[..expectedLuaCData.Length], "LuaC Data");
			if (!data.SequenceEqual(expectedLuaCData))
			{
				throw new IncorrectLuaCDataException(data.ToArray(), $"Invalid LuaCData {data.ToHumanReadable()}, expected: {expectedLuaCData.ToHumanReadable()}");
			}

			byte readInstructionSize = byteStream.ReadOrThrow("instruction size");
			if (readInstructionSize != instructionSize)
			{
				throw new UnexpectedInstructionSizeException(instructionSize, $"Unexpected instruction size {readInstructionSize}, expected: {instructionSize}");
			}

			byte integerSize = byteStream.ReadOrThrow("Integer size");
			if (integerSize != expectedIntegerSize)
			{
				throw new UnexpectedIntegerSizeException(integerSize, $"Unexpected instruction size {integerSize}, expected: {expectedIntegerSize}");
			}

			byte numberSize = byteStream.ReadOrThrow("Number size");
			if (numberSize != expectedNumberSize)
			{
				throw new UnexpectedNumberSizeException(numberSize, $"Unexpected number size {numberSize}, expected: {expectedNumberSize}");
			}

			LuaInteger checkInteger = byteStream.ReadLongOrThrow("Check integer");
			if (checkInteger != luaCInt)
			{
				throw new UnexpectedCheckIntegerException(checkInteger, $"Unexpected check integer value 0x{checkInteger:X}, expected 0x{luaCInt:X}");
			}

			LuaNumber checkNumber = byteStream.ReadDoubleOrThrow("Check number");
			if (checkNumber != luaCNumber)
			{
				throw new UnexpectedCheckNumberException(checkNumber, $"Unexpected check number value 0x{checkNumber}, expected {luaCNumber}");
			}

			return new LuaCHeader
			{
				luaVersion = version,
				luaFormat = format,
				instructionSize = instructionSize,
				integerSize = integerSize,
				numberSize = numberSize
			};
		}

		private ILuaConstant[] LoadConstants(Stream byteStream)
		{
			int constantCount = byteStream.ReadSignedSizeOrThrow("amount of constants");
			var container = new ILuaConstant[constantCount];
			for (int i = 0; i < constantCount; i++)
			{
				byte tag = byteStream.ReadOrThrow("constant tag");
				container[i] = (LuaVariantTag)tag switch
				{
					LuaVariantTag.LuaVNil => new LuaNilConstant(),
					LuaVariantTag.LuaVFalse => new LuaBoolConstant(false),
					LuaVariantTag.LuaVTrue => new LuaBoolConstant(true),
					LuaVariantTag.LuaVNumFlt => new LuaFloatConstant(byteStream.ReadDoubleOrThrow("float variatn number constant")),
					LuaVariantTag.LuaVNumInt => new LuaIntegerConstant(byteStream.ReadLongOrThrow("integer variant number constant")),
					LuaVariantTag.LuaVShrStr or LuaVariantTag.LuaVLngStr => new LuaStringConstant(byteStream.ReadSizedByteString("srting constant")),
					_ => throw new UnknownConstantTypeException(tag, $"Unknown constant type encountered while reading constants 0x{tag:X}")
				};
			}

			return container;
		}

		private LuaUpValue[] ParseUpValues(Stream byteStream)
		{
			int upValueCount = byteStream.ReadSignedSizeOrThrow("amount of upvalues");
			var container = new LuaUpValue[upValueCount];
			for (int i = 0; i < upValueCount; i++)
			{
				var inStack = byteStream.ReadOrThrow("in stack value for upvalue");
				var index = byteStream.ReadOrThrow("index value for upvalue");
				var kind = byteStream.ReadOrThrow("kind value for upvalue");
				container[i] = new LuaUpValue
				{
					inStack = inStack,
					index = index,
					kind = kind
				};
			}
			return container;
		}

		private LuaProto[] ParseProtos(Stream byteStream)
		{
			int protoCount = byteStream.ReadSignedSizeOrThrow("amount of protos");
			var container = new LuaProto[protoCount];
			for (int i = 0; i < protoCount; i++)
			{
				container[i] = ParseFunction(byteStream);
			}
			return container;
		}

		public LuaCFunctionDebugInfo ParseProtoDebugInfo(Stream byteStream)
		{
			var lineInfoCount = byteStream.ReadSignedSizeOrThrow("line info count");
			var lineInfo = new byte[lineInfoCount];
			byteStream.ReadOrThrow(lineInfo, "line info");

			var absLineInfoCount = byteStream.ReadSignedSizeOrThrow("abs line info count");
			var absLineInfo = new LuaCAbsLineInfo[absLineInfoCount];
			for (var i = 0; i < absLineInfoCount; i++)
			{
				var pc = byteStream.ReadSignedSizeOrThrow("abs line info pc");
				var line = byteStream.ReadSignedSizeOrThrow("abs line info line");
				absLineInfo[i] = new LuaCAbsLineInfo
				{
					pc = pc,
					line = line
				};
			}

			var locVarCount = byteStream.ReadSignedSizeOrThrow("loc vars count");
			var locVars = new LuaCLocVar[locVarCount];
			for (var i = 0; i < locVarCount; i++)
			{
				var varName = byteStream.ReadSizedByteString("loc var, var name");
				var startPc = byteStream.ReadSignedSizeOrThrow("loc var start pc");
				var endPc = byteStream.ReadSignedSizeOrThrow("loc var end pc");
				locVars[i] = new LuaCLocVar
				{
					varName = varName,
					startPc = startPc,
					endPc = endPc
				};
			}

			// This differs from the c lua implementation, there it uses the previous described upvalue count
			// So it ignores the number if its equal to zero and just reads the amount of upvalues it knows
			// exists https://www.lua.org/source/5.4/lundump.c.html#loadDebug This means some LuaC files can
			// be valid in the original lua but will not be valid here. Also an interesting attack vector
			// since it will appear invalid to some "wrongly" implemented decompilers but it will still run 

			var upValueNameCount = byteStream.ReadSignedSizeOrThrow("upvalue name count");
			var upValueNames = new byte[upValueNameCount][];
			for (var i = 0; i < upValueNameCount; i++)
			{
				upValueNames[i] = byteStream.ReadSizedByteString("upvalue name");
			}

			return new LuaCFunctionDebugInfo
			{
				lineInfo = lineInfo,
				absLineInfo = absLineInfo,
				locVars = locVars,
				upValueNames = upValueNames,
			};
		}

		public LuaProto ParseFunction(Stream byteStream)
		{
			var source = byteStream.ReadSizedByteString("File source");
			var lineDefined = (int)byteStream.ReadUnsignedSizeOrThrow("line defined");
			var lastLineDefined = (int)byteStream.ReadUnsignedSizeOrThrow("last line defined");
			var numParams = byteStream.ReadOrThrow("number of params");
			bool isVararg = byteStream.ReadOrThrow("is vararg") != 0;
			var maxStackSize = byteStream.ReadOrThrow("max stack size");
			
			var codeLength = byteStream.ReadSignedSizeOrThrow("bytecode length");
			var code = new LuaInstruction[codeLength];
			
			Span<byte> instructionBytes = stackalloc byte[4];

			for (int i = 0; i < codeLength; i++)
			{
				byteStream.ReadOrThrow(instructionBytes, "instruction");
				code[i] = new LuaInstruction(instructionBytes);
			}
			
			var constants = LoadConstants(byteStream);
			var upValues = ParseUpValues(byteStream);
			var protos = ParseProtos(byteStream);
			var debugInfo = ParseProtoDebugInfo(byteStream);
			return new LuaProto
			{
				source = source,
				lineDefined = lineDefined,
				lastLineDefined = lastLineDefined,
				numParams = numParams,
				isVararg = isVararg,
				maxStackSize = maxStackSize,
				code = code,
				constants = constants,
				upValues = upValues,
				protos = protos,
				debugInfo = debugInfo
			};
		}

		public LuaCFile ParseLuaCFile(Stream byteStream)
		{
			var header = ParseHeader(byteStream);
			var upValueSize = byteStream.ReadOrThrow("top level upvalue size");
			var topLevelFunction = ParseFunction(byteStream);
			return new LuaCFile
			{
				header = header,
				upValueSize = upValueSize,
				topLevelFunction = topLevelFunction
			};
		}
	}
	public struct LuaCFile
	{
		public required LuaCHeader header;
		public required byte upValueSize;
		public required LuaProto topLevelFunction;
	}

	public struct LuaCHeader
	{
		public required byte luaVersion;
		public required byte luaFormat;
		public required byte instructionSize;
		public required byte integerSize;
		public required byte numberSize;
	}

	public interface ILuaConstant
	{

	}

	public class LuaStringConstant(byte[] _data) : ILuaConstant
	{
		public readonly byte[] data = _data;
	}

	public class LuaBoolConstant(bool _value) : ILuaConstant
	{
		public readonly bool value = _value;
	}

	public class LuaIntegerConstant(LuaInteger _value) : ILuaConstant
	{
		public readonly LuaInteger value = _value;
	}

	public class LuaFloatConstant(LuaNumber _value) : ILuaConstant
	{
		public readonly LuaNumber value = _value;
	}

	public class LuaNilConstant : ILuaConstant
	{

	}

	public struct LuaUpValue
	{
		public required byte inStack;
		public required byte index;
		public required byte kind;
	}

	public struct LuaCAbsLineInfo
	{
		public required int pc;
		public required int line;
	}

	public struct LuaCLocVar
	{
		public required byte[] varName;
		public required int startPc;
		public required int endPc;
	}

	public struct LuaCFunctionDebugInfo
	{
		public required byte[] lineInfo;
		public required LuaCAbsLineInfo[] absLineInfo;
		public required LuaCLocVar[] locVars;
		public required byte[][] upValueNames;
	}

	public class LuaProto
	{
		public required byte[] source;
		public required int lineDefined;
		public required int lastLineDefined;
		public required byte numParams;
		public required bool isVararg;
		public required byte maxStackSize;
		public required LuaInstruction[] code;
		public required ILuaConstant[] constants;
		public required LuaUpValue[] upValues;
		public required LuaProto[] protos;
		public required LuaCFunctionDebugInfo debugInfo;
	}
}
