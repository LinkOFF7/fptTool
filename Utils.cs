using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fptTool
{
    public static class Utils
    {
		public static void AlignPosition(BinaryReader reader)
		{
			long pos = reader.BaseStream.Position;
			if (pos % 0x10 != 0)
				reader.BaseStream.Position = (0x10 - pos % 0x10) + pos;
		}

		public static long GetAlignedLength(BinaryWriter writer)
        {
			long length = 0;
			long pos = writer.BaseStream.Position;
			if (pos % 0x10 != 0)
				length = ((0x10 - pos % 0x10) + pos) - pos;
			return length;
		}

		public static void AlignPosition(BinaryWriter writer)
		{
			long pos = writer.BaseStream.Position;
			if (pos % 0x10 != 0)
				writer.BaseStream.Position = (0x10 - pos % 0x10) + pos;
		}


		public static long GetAlignedLength(BinaryReader reader)
		{
			long length = 0;
			long pos = reader.BaseStream.Position;
			if (pos % 0x10 != 0)
				length = ((0x10 - pos % 0x10) + pos) - pos;
			return length;
		}
		public static string ReadString(this BinaryReader binaryReader, Encoding encoding)
		{
			if (binaryReader == null) throw new ArgumentNullException("binaryReader");
			if (encoding == null) throw new ArgumentNullException("encoding");

			List<byte> data = new List<byte>();

			while (binaryReader.BaseStream.Position < binaryReader.BaseStream.Length)
			{
				data.Add(binaryReader.ReadByte());

				string partialString = encoding.GetString(data.ToArray(), 0, data.Count);

				if (partialString.Length > 0 && partialString.Last() == '\0')
					return encoding.GetString(data.SkipLast(encoding.GetByteCount("\0")).ToArray());
			}

			throw new InvalidDataException("Hit end of stream while reading null-terminated string.");
		}
		private static IEnumerable<TSource> SkipLast<TSource>(this IEnumerable<TSource> source, int count)
		{
			if (source == null) throw new ArgumentNullException("source");

			Queue<TSource> queue = new Queue<TSource>();

			foreach (TSource item in source)
			{
				queue.Enqueue(item);

				if (queue.Count > count) yield return queue.Dequeue();
			}
		}
    }
}
