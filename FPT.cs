using System;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace fptTool
{
    internal class FPT
    {
        internal string Magic;
        internal int Zero;
        internal int Count;
        internal int UnknownBoolean;
        internal int bodystart;
        internal string hash;

        public struct Entry
        {
            public string Name;
            public uint HashCode;
            public int Offset;
            public int Size;
            public string Text;
        }

        internal Entry ReadEntry(BinaryReader reader)
        {
            Entry entry = new Entry();
            entry.Name = Regex.Replace(Encoding.ASCII.GetString(reader.ReadBytes(0x10)), "\0", "").Substring(1);
            entry.HashCode = reader.ReadUInt32();
            entry.Offset = reader.ReadInt32() + bodystart;
            entry.Size = reader.ReadInt32();
            reader.BaseStream.Position += 4;
            return entry;
        }

        public List<Entry> ReadFromTextFile(string inputTextFile)
        {
            StreamReader sr = new StreamReader(inputTextFile);
            List<string> names = new List<string>();
            List<string> text = new List<string>();
            List<uint> hashes = new List<uint>();
            int count = 0;
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (!line.StartsWith("@"))
                {
                    Console.WriteLine("Строка {0} должна начинаться со знака '@'!", count);
                    throw new Exception();
                }
                names.Add(line.Substring(1, line.IndexOf('(')-1));
                hash = line.Substring(line.IndexOf('(') + 1, line.IndexOf(')') - 12);
                hashes.Add(UInt32.Parse(hash));
                text.Add(line.Substring(line.IndexOf(')') + 1).Replace("{CLRF}", "\r\n").Replace("{CL}", "\r").Replace("{RF}", "\n"));
            }
            sr.Close();

            List<Entry> entries = new List<Entry>();
            int localoffset = 0;
            for(int i = 0; i < text.Count; i++)
            {
                Entry entry = new Entry();
                entry.Name = names[i];
                entry.HashCode = hashes[i];
                entry.Offset = localoffset;
                entry.Size = text[i];
                entry.Text = text[i];
                entries.Add(entry);
                localoffset += entry.Size;
            }
            return entries;
        } 

        public void Create(string inputTextFile)
        {
            List<Entry> entries = ReadFromTextFile(inputTextFile);
            string newFile = inputTextFile.Split('.')[0] + "_new.fpt";
            BinaryWriter writer = new BinaryWriter(File.Create(newFile));
            //header
            writer.Write(Encoding.ASCII.GetBytes("FPT0"));
            writer.Write(new int());
            writer.Write(entries.Count);
            writer.Write(1);
            //entries
            foreach(Entry entry in entries)
            {
                writer.Write(Encoding.UTF8.GetBytes("#"+entry.Name));
                writer.Write(new byte[Utils.GetAlignedLength(writer)]);
                writer.Write(entry.HashCode);
                writer.Write(entry.Offset);
                writer.Write(entry.Size);
                writer.Write(0);
            }

            //unknown block TEMP/STEP1
            writer.Write(Encoding.UTF8.GetBytes("TEMP/STEP1"));
            writer.Write(new byte[0x32]);
            writer.Write(177381026);

            //text
            foreach (Entry entry in entries)
            {
                writer.Write(Encoding.UTF8.GetBytes(entry.Text));
            }
        }

        public void Extract(string inputFile)
        {
            BinaryReader reader = new BinaryReader(File.OpenRead(inputFile));
            Magic = Encoding.UTF8.GetString(reader.ReadBytes(4));
            reader.BaseStream.Position += 4;
            Count = reader.ReadInt32();
            reader.BaseStream.Position += 4;
            bodystart = Count * 0x20 + 0x50;
            List<Entry> entries = new List<Entry>();
            List<string> allText = new List<string>();
            for(int i = 0; i < Count; i++)
            {
                entries.Add(ReadEntry(reader));
            }
            foreach (Entry entry in entries)
            {
                reader.BaseStream.Position = entry.Offset;
                string data = Encoding.UTF8.GetString(reader.ReadBytes(entry.Size));
                allText.Add(data.Replace("\r\n", "{CLRF}").Replace("\n", "{RF}").Replace("\r", "{CL}"));
            }
            Combine(allText, inputFile + ".txt", entries);
        }

        static void SplitText(string inputFile)
        {
            List<string> filenames = new List<string>();
            StreamReader sr = new StreamReader(inputFile);
            string dir = Path.GetFileNameWithoutExtension(inputFile) + Path.DirectorySeparatorChar;
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
            int c = -1;
            while (!sr.EndOfStream)
            {
                string line = sr.ReadLine();
                if (line.StartsWith("----------"))
                {
                    if (line == "") continue;
                    filenames.Add(line.Substring(10, line.IndexOf(".txt") - 6));
                    c++;
                }
                else
                    File.AppendAllText(dir + filenames[c], line + "\r\n");
            }
        }

        static void Combine(List<string> array, string outTxt, List<Entry> entries)
        {
            using (var output = File.Create(outTxt))
            {
                int c = 0;
                foreach (var file in array)
                {
                    byte[] line = Encoding.UTF8.GetBytes($"@{entries[c].Name}({entries[c].HashCode})" + file + "\n");
                    output.Write(line, 0, line.Length);
                    c++;
                }
            }
        }
    }
}
