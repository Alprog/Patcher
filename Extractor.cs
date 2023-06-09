﻿
using System.IO;

namespace Patcher
{
    public class Extractor
    {
        private HashTable HashTable;
        private string OriginalFolder;
        private string OutputFolder;
        private BinaryWriter Writer;
        private FileDesc WritingDesc;
        private Stream[] OriginalStreams;
        public int Errors = 0;

        public Extractor(HashTable hashTable, string originalFolder, string outputFolder)
        {
            this.HashTable = hashTable;
            this.OriginalFolder = originalFolder;
            this.OutputFolder = outputFolder;
            OriginalStreams = new Stream[hashTable.Files.Count];
        }

        public void ProcessFile(string diffFilePath)
        {
            var stream = File.OpenRead(diffFilePath);
            var reader = new BinaryReader(stream);
            var size = FileSystem.GetFileSize(diffFilePath);

            while (stream.Position != size)
            {
                var type = (BlockType)reader.ReadByte();
                switch (type)
                {
                    case BlockType.NewFile:
                        ProcessNewFile(reader);
                        break;

                    case BlockType.RawBytes:
                        ProcessRawBytes(reader);
                        break;

                    case BlockType.Indirect:
                        ProcessIndirect(reader);
                        break;

                    case BlockType.NewCopyFile:
                        ProcessNewCopyFile(reader);
                        break;
                }

            }

            EndWriting();
            reader.Close();
        }

        public void EndWriting()
        {
            if (WritingDesc != null)
            {
                if (Writer != null)
                {
                    Writer.Close();
                    Writer = null;
                }

                var fullPath = Path.Combine(OutputFolder, WritingDesc.Path);
                var generatedHash = FileSystem.CalcMD5(fullPath);
                
                
                if (generatedHash == WritingDesc.NewHash)
                {
                    Console.WriteLine("  done");
                }
                else
                {
                    Console.WriteLine("  ERROR! Hash is not the same!");
                    Errors++;
                }

                WritingDesc = null;
            }
        }

        void ProcessNewFile(BinaryReader reader)
        {
            EndWriting();

            var fileIndex = reader.ReadInt16();
            WritingDesc = HashTable.FromIndex(fileIndex);
            var fullPath = Path.Combine(OutputFolder, WritingDesc.Path);
            FileSystem.CreateFolderForPath(fullPath);
            var stream = File.OpenWrite(fullPath);
            Writer = new BinaryWriter(stream);

            Console.Write("Extracting file " + WritingDesc.Path + "...");
        }

        void ProcessNewCopyFile(BinaryReader reader)
        {
            EndWriting();

            var dstFileIndex = reader.ReadInt16();
            var srcFileIndex = reader.ReadInt16();
            var dstDesc = HashTable.FromIndex(dstFileIndex);
            var srcDesc = HashTable.FromIndex(srcFileIndex);
            WritingDesc = dstDesc;
            var dstPath = Path.Combine(OutputFolder, dstDesc.Path);
            var srcPath = Path.Combine(OriginalFolder, srcDesc.Path);
            FileSystem.CreateFolderForPath(dstPath);
            File.Copy(srcPath, dstPath, true);

            Console.Write("Extracting file " + WritingDesc.Path + "...");
        }

        void ProcessRawBytes(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            var bytes = reader.ReadBytes(count);
            Writer.Write(bytes);
        }

        void ProcessIndirect(BinaryReader reader)
        {
            var fileIndex = reader.ReadInt16();
            var position = reader.ReadInt32();
            var length = reader.ReadInt32();

            var bytes = new byte[length];

            var originalStream = GetOriginalStream(fileIndex);
            originalStream.Position = position;
            originalStream.Read(bytes, 0, length);

            Writer.Write(bytes);
        }

        Stream GetOriginalStream(Int16 fileIndex)
        {
            if (OriginalStreams[fileIndex] == null)
            {
                var filePath = Path.Combine(OriginalFolder, HashTable.Files[fileIndex].Path);
                OriginalStreams[fileIndex] = File.OpenRead(filePath);
            }
            return OriginalStreams[fileIndex];
        }

        public void CloseAllStream()
        {
            foreach (var originalStream in OriginalStreams)
            {
                if (originalStream != null)
                {
                    originalStream.Close();
                }
            }
            this.OriginalStreams = null;
        }
    }
}
