
using System.Reflection.Metadata;
using System.Text.RegularExpressions;

namespace Patcher
{
    public class Differ
    {
        private HeaderSeacher Seacher;
        private string OriginalFolder;
        private HashTable HashTable;
        private HeaderTable HeaderTable;
        private List<HeaderMatch> HeaderMatches;
        private List<BlockMatch> BlockMatches;

        public Differ(string filePath, string originalFolder, HashTable hashTable, HeaderTable headerTable)
        {
            this.Seacher = new HeaderSeacher(filePath);
            this.OriginalFolder = originalFolder;
            this.HashTable = hashTable;
            this.HeaderTable = headerTable;
            this.HeaderMatches = new List<HeaderMatch>();
            this.BlockMatches = new List<BlockMatch>();
        }

        public void FullProcess()
        {
            FindHeaderMatches();
            FindBlockMatches();
            FilterBlockMatches();
            PrintStatistics();
        }

        public void FindHeaderMatches()
        {
            long skipMatchesUntil = 0;
            do
            {
                if (Seacher.Position % (3 * Constants.MiB) == 0)
                {
                    var percent = (float)Seacher.Position / Seacher.FileSize * 100;
                    Console.WriteLine(Math.Round(percent) + "% (matches: " + HeaderMatches.Count + ")");
                }

                if (Seacher.Position < skipMatchesUntil)
                {
                    continue;
                }

                if (HeaderTable.Map.TryGetValue(Seacher.FastCheckSum, out List<Header> headers))
                {
                    var curBytes = Seacher.GetBytes();
                    foreach (var header in headers)
                    {
                        if (curBytes.SequenceEqual(header.Bytes))
                        {
                            HeaderMatches.Add(new HeaderMatch(Seacher.Position, header));
                            skipMatchesUntil = Seacher.Position + Constants.SkipAfterMatchSize;
                        }
                    }
                }
            }
            while (Seacher.MoveNext());

            Console.WriteLine("100% (matches: " + HeaderMatches.Count + ")");
        }

        public void FindBlockMatches()
        {
            var stream = Seacher.Stream;

            long last = 0;
            for (int i = 0; i < HeaderMatches.Count; i++)
            {
                if (i % 200 == 0)
                {
                    Console.WriteLine(i + " / " + HeaderMatches.Count);
                }

                var match = HeaderMatches[i];
                if (match.Position + Constants.DeepSkipSize < last)
                {
                    continue;
                }

                int matchedLength = Constants.HeaderSize;

                stream.Position = match.Position + matchedLength;

                var otherStream = File.OpenRead(Path.Combine(Constants.OriginalFolder, match.Header.FilePath));
                otherStream.Position = match.Header.StartPosition + matchedLength;

                while (stream.ReadByte() == otherStream.ReadByte())
                {
                    matchedLength++;
                    if (stream.Position == Seacher.FileSize)
                    {
                        break;
                    }
                }

                var dstStartOffset = match.Position;
                var srcStartOffst = match.Header.StartPosition;

                var blockMatch = new BlockMatch(dstStartOffset, srcStartOffst, matchedLength, match.Header.FilePath);
                last = Math.Max(last, blockMatch.DstEndPosition);
                BlockMatches.Add(blockMatch);

                otherStream.Close();
            }

            Console.WriteLine("BlockMatches: " + BlockMatches.Count);
        }

        public void FilterBlockMatches()
        {
            for (int i = 1; i < BlockMatches.Count; i++)
            {
                var lastEnd = BlockMatches[i - 1].DstEndPosition;

                var match = BlockMatches[i];
                if (match.DstEndPosition <= lastEnd)
                {
                    // if end position is less or equal than previous => exclude me
                    // [............)
                    //       ....)
                    BlockMatches.RemoveAt(i--);
                    continue;
                }

                if (match.DstPosition == BlockMatches[i - 1].DstPosition)
                {
                    // if start position is same as previous => exclude previous (we know that our end is bigger)
                    // [............)
                    // [...............)
                    BlockMatches.RemoveAt(--i);
                    continue;
                }

                var offset = lastEnd - match.DstPosition;
                if (offset > 0)
                {
                    //        if intersection => correct it
                    // [............)         |     [............)
                    //       [...........)    |                  [....)   
                    match.SrcPosition += offset;
                    match.DstPosition += offset;
                    match.Length -= offset;
                }
            }
            Console.WriteLine("Filtered blockMatches: " + BlockMatches.Count);
        }

        void PrintStatistics()
        {
            long total = 0;
            foreach (var match in BlockMatches)
            {
                total += match.Length;
                Console.WriteLine(match.DstPosition + " " + match.Length + " " + match.DstEndPosition + " " + match.SrcFile);
            }

            Console.WriteLine(total);
            Console.WriteLine((float)total / Seacher.FileSize * 100);
        }

        public byte[] GetBytes(long startPosition, long endPosition)
        {
            var length = endPosition - startPosition;
            var bytes = new byte[length];
            Seacher.Stream.Position = startPosition;
            Seacher.Stream.Read(bytes, 0, (int)length);
            return bytes;
        }

        public void Save(string fullPath)
        {
            FileSystem.CreateFolderForPath(fullPath);

            var diffWriter = new DiffWriter(fullPath, HashTable);

            long processed = 0;
            foreach (var match in BlockMatches)
            {
                if (match.DstPosition > processed)
                {
                    var bytes = GetBytes(processed, match.DstPosition);
                    diffWriter.Write(bytes);
                    processed += bytes.Length;
                }

                diffWriter.Write(match);
                processed += match.Length;
            }

            if (Seacher.FileSize > processed)
            {
                var bytes = GetBytes(processed, Seacher.FileSize);
                diffWriter.Write(bytes);
            }
        }
    }
}
