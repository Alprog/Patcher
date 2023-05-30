
namespace Patcher
{
    public class Differ
    {
        public void CreateDiff(string fullPath, HeaderTable info)
        {
            var stream = File.OpenRead(fullPath);
            long size = new FileInfo(fullPath).Length;

            int fastCheckSum = 0;
            Queue<byte> byteQueue = new Queue<byte>();
            for (int j = 0; j < Constants.HeaderSize; j++)
            {
                var b = (byte)stream.ReadByte();
                byteQueue.Enqueue(b);
                fastCheckSum += b;
            }

            var matches = new List<HeaderMatch>();

            long skipMatchesUntil = 0;
            while (stream.Position < size)
            {
                fastCheckSum -= byteQueue.Dequeue();
                var b = (byte)stream.ReadByte();
                fastCheckSum += b;
                byteQueue.Enqueue(b);

                if (stream.Position < skipMatchesUntil)
                {
                    continue;
                }

                if (info.Map.TryGetValue(fastCheckSum, out List<Header> list))
                {
                    var curBytes = byteQueue.ToArray();
                    foreach (var blockStart in list)
                    {
                        if (curBytes.SequenceEqual(blockStart.Bytes))
                        {
                            matches.Add(new HeaderMatch(stream.Position, blockStart));
                            skipMatchesUntil = stream.Position + Constants.SkipAfterMatchSize;
                        }
                    }
                }
            }

            Console.WriteLine("Matches: " + matches.Count);

            var matchBlocks = new List<BlockMatch>();

            long last = 0;
            foreach (var match in matches)
            {
                if (match.Position + Constants.DeepSkipSize < last)
                {
                    continue;
                }

                stream.Position = match.Position;

                var otherStream = File.OpenRead(Path.Combine(Constants.OriginalFolder, match.Header.FilePath));
                otherStream.Position = match.Header.StartPosition;

                int i = 0;
                while (stream.ReadByte() == otherStream.ReadByte())
                {
                    i++;
                    if (stream.Position == size)
                    {
                        break;
                    }
                }

                var dstStartOffset = match.Position - Constants.HeaderSize;
                var srcStartOffst = match.Header.StartPosition - Constants.HeaderSize;
                var length = (i + Constants.HeaderSize);

                var block = new BlockMatch(dstStartOffset, srcStartOffst, length, match.Header.FilePath);
                last = Math.Max(last, block.DstEndPosition);
                matchBlocks.Add(block);

                otherStream.Close();
            }

            foreach (var block in matchBlocks)
            {
                Console.WriteLine(block.DstPosition + " " + block.Length + " " + block.DstEndPosition + " " + block.SrcFile);
            }

            long last2 = 0;
            for (int i = 0; i < matchBlocks.Count; i++)
            {
                var block = matchBlocks[i];
                if (block.DstEndPosition <= last2)
                {
                    matchBlocks.RemoveAt(i--);
                    continue;
                }

                var offset = last2 - block.DstPosition;
                if (offset > 0)
                {
                    block.SrcPosition += offset;
                    block.DstPosition += offset;
                    block.Length += offset;
                }

                last2 = block.DstEndPosition;
            }

            Console.WriteLine("--------------------------");

            Console.WriteLine(matchBlocks.Count);

            long total = 0;
            foreach (var block in matchBlocks)
            {
                total += block.Length;
                Console.WriteLine(block.DstPosition + " " + block.Length + " " + block.DstEndPosition + " " + block.SrcFile);
            }

            Console.WriteLine(total);
            Console.WriteLine((float)total / size * 100);

        }
    }
}
