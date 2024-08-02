using System.Text;

namespace LuaByteCode
{
	public class UnexpectedEndOfStreamException(string? message) : Exception(message)
	{
	}

	public class SizeValueTooLargeException(string? message) : Exception(message)
	{

	}
	internal static class Extensions
	{
		public static string ToHumanReadable(this IEnumerable<byte> bytes)
		{
			return $"{{{string.Join(", ", bytes.Select(x => $"0x{x:X}"))}}}";
		}

		public static string ToHumanReadable(this ref Span<byte> bytes)
		{
			StringBuilder builder = new();

			builder.Append('{');
			var it = bytes.GetEnumerator();
			if (it.MoveNext())
			{
				while (true)
				{
					builder.Append($"0x{it.Current:X}");
					if (it.MoveNext())
					{
						builder.Append(',');
					}
					else
					{
						break;
					}
				}
			}

			builder.Append('}');
			return builder.ToString();
		}

		public static Span<byte> ReadOrThrow(this Stream stream, Span<byte> toSpan, string reading)
		{
			if (stream.ReadAtLeast(toSpan, toSpan.Length, false) < toSpan.Length)
			{
				throw new UnexpectedEndOfStreamException("Unexpected end of stream while trying to read " + reading);
			}

			return toSpan;
		}


		public static byte ReadOrThrow(this Stream stream, string reading)
		{
			int read = stream.ReadByte();
			if (read == -1)
			{
				throw new UnexpectedEndOfStreamException("Unexpected end of stream while trying to read " + reading);
			}

			return (byte)read;
		}

		public static long ReadLongOrThrow(this Stream stream, string reading)
		{
			Span<byte> buffer = stackalloc byte[8];
			Span<byte> integerBuffer = ReadOrThrow(stream, buffer, reading);
			return BitConverter.ToInt64(integerBuffer);
		}

		public static double ReadDoubleOrThrow(this Stream stream, string reading)
		{
			Span<byte> buffer = stackalloc byte[8];
			Span<byte> integerBuffer = ReadOrThrow(stream, buffer, reading);
			return BitConverter.ToDouble(integerBuffer);
		}

		public static uint ReadUnsignedSizeOrThrow(this Stream stream, string reading, ulong limit)
		{
			ulong accumulator = 0;
			limit >>= 7;
			byte b = 0;
			while ((b & 0x80) == 0)
			{
				b = stream.ReadOrThrow(reading);
				if (accumulator >= limit)
				{
					throw new SizeValueTooLargeException($"size value is too large while reading {reading}");
				}
				accumulator <<= 7;
				accumulator |= (b & 0x7Fu);
			}
			return (uint)accumulator;
		}

		public static uint ReadUnsignedSizeOrThrow(this Stream stream, string reading)
		{
			return stream.ReadUnsignedSizeOrThrow(reading, uint.MaxValue);
		}

		public static int ReadSignedSizeOrThrow(this Stream stream, string reading)
		{
			return unchecked((int) stream.ReadUnsignedSizeOrThrow(reading, int.MaxValue));
		}

		public static byte[] ReadSizedByteString(this Stream stream, string reading)
		{
			//TODO: Possibly add extra error information here to the reading var
			var len = stream.ReadUnsignedSizeOrThrow(reading);
			if (len == 0)
			{
				return [];
			}
			//Subtract one from length since do not need to store terminating zero
			byte[] bytes = new byte[len - 1];
			stream.ReadOrThrow(bytes, reading);
			return bytes;
		}
	}
}
