using LuaByteCode.LuaCConstructs;
using LuaByteCode.LuaCConstructs.Types;

namespace LuaByteCode
{
	public class LuaByteCodeReader
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

			long checkInteger = byteStream.ReadLongOrThrow("Check integer");
			if (checkInteger != luaCInt)
			{
				throw new UnexpectedCheckIntegerException(checkInteger, $"Unexpected check integer value 0x{checkInteger:X}, expected 0x{luaCInt:X}");
			}

			double checkNumber = byteStream.ReadDoubleOrThrow("Check number");
			if (checkNumber != luaCNumber)
			{
				throw new UnexpectedCheckNumberException(checkNumber, $"Unexpected check number value 0x{checkNumber}, expected {luaCNumber}");
			}

			return new LuaCHeader
			{
				LuaVersion = version,
				LuaFormat = format,
				InstructionSize = instructionSize,
				IntegerSize = integerSize,
				NumberSize = numberSize
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
					LuaVariantTag.LuaVNil => new NilConstant(),
					LuaVariantTag.LuaVFalse => new BoolConstant(false),
					LuaVariantTag.LuaVTrue => new BoolConstant(true),
					LuaVariantTag.LuaVNumFlt => new FloatConstant(byteStream.ReadDoubleOrThrow("float variatn number constant")),
					LuaVariantTag.LuaVNumInt => new IntegerConstant(byteStream.ReadLongOrThrow("integer variant number constant")),
					LuaVariantTag.LuaVShrStr or LuaVariantTag.LuaVLngStr => new StringConstant(byteStream.ReadSizedByteString("srting constant")),
					_ => throw new UnknownConstantTypeException(tag, $"Unknown constant type encountered while reading constants 0x{tag:X}")
				};
			}

			return container;
		}

		private LuaCUpValue[] ParseUpValues(Stream byteStream)
		{
			int upValueCount = byteStream.ReadSignedSizeOrThrow("amount of upvalues");
			var container = new LuaCUpValue[upValueCount];
			for (int i = 0; i < upValueCount; i++)
			{
				var inStack = byteStream.ReadOrThrow("in stack value for upvalue") != 0;
				var index = byteStream.ReadOrThrow("index value for upvalue");
				var kind = byteStream.ReadOrThrow("kind value for upvalue");
				container[i] = new LuaCUpValue
				{
					InStack = inStack,
					Index = index,
					Kind = kind
				};
			}
			return container;
		}

		private LuaCPrototype[] ParseProtos(Stream byteStream)
		{
			int protoCount = byteStream.ReadSignedSizeOrThrow("amount of protos");
			var container = new LuaCPrototype[protoCount];
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
					Pc = pc,
					Line = line
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
					VarName = varName,
					StartPc = startPc,
					EndPc = endPc
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
				LineInfo = lineInfo,
				AbsLineInfo = absLineInfo,
				LocVars = locVars,
				UpValueNames = upValueNames,
			};
		}

		public LuaCPrototype ParseFunction(Stream byteStream)
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
			return new LuaCPrototype
			{
				Source = source,
				LineDefined = lineDefined,
				LastLineDefined = lastLineDefined,
				NumParams = numParams,
				IsVarArg = isVararg,
				MaxStackSize = maxStackSize,
				Code = code,
				Constants = constants,
				UpValues = upValues,
				Prototypes = protos,
				DebugInfo = debugInfo
			};
		}

		public LuaCFile ParseLuaCFile(Stream byteStream)
		{
			var header = ParseHeader(byteStream);
			var upValueSize = byteStream.ReadOrThrow("top level upvalue size");
			var topLevelFunction = ParseFunction(byteStream);
			return new LuaCFile
			{
				Header = header,
				UpValueSize = upValueSize,
				TopLevelFunction = topLevelFunction
			};
		}
	}
}
