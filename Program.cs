
using System.Linq;

namespace Patcher
{
    public class Program
    {
        static string SteamFolder = "C:\\Steam\\steamapps\\common\\Encased";
        static string PatchFolder = "C:\\Users\\alpro\\Downloads\\v1.4.713.1655";

        static string SteamHashTable = "C:\\patcher\\steam.txt";
        static string PatchHashTable = "C:\\patcher\\patch.txt";

        //static string SubPath = "Encased_Data\\StreamingAssets\\Master.bank";
        //static string SubPath = "Encased_Data\\StreamingAssets\\AssetBundles\\misc";
        //static string SubPath = "Encased_Data\\sharedassets76.assets.resS";
        static string SubPath = "Encased_Data\\level178";

        static int mb = 1024 * 1024;

        static void Main()
        {
            //var info = new BlockInfo();
            //info.Collect(SteamFolder, mb / 16);
            //info.Save();

            var info = new BlockInfo();
            info.Load();

            ProcessFile(Path.Combine(PatchFolder, SubPath), info);
        }



        static public void ProcessFile(string fullPath, BlockInfo info)
        {
            var stream = File.OpenRead(fullPath);
            long size = new FileInfo(fullPath).Length;

            int fastCheckSum = 0;
            Queue<byte> byteQueue = new Queue<byte>();
            for (int j = 0; j < 1024; j++)
            {
                var b = (byte)stream.ReadByte();
                byteQueue.Enqueue(b);
                fastCheckSum += b;
            }

            var matches = new List<Match>();

            long hiden = 0;
            while (stream.Position < size)
            {
                fastCheckSum -= byteQueue.Dequeue();
                var b = (byte)stream.ReadByte();
                fastCheckSum += b;
                byteQueue.Enqueue(b);

                if (stream.Position > hiden)
                {
                    if (info.Dict.TryGetValue(fastCheckSum, out List<BlockStart> list))
                    {
                        var curBytes = byteQueue.ToArray();                        
                        foreach (var blockStart in list)
                        {
                            if (curBytes.SequenceEqual(blockStart.Bytes))
                            {
                                matches.Add(new Match(stream.Position, blockStart));
                                hiden = stream.Position + 10 * 1024;
                            }
                        }
                    }
                }

            }

            Console.WriteLine("Matches: " + matches.Count);

            var matchBlocks = new List<MatchBlock>();

            long last = 0;
            foreach (var match in matches)
            {
                if (match.Position + 10 * mb < last)
                {
                    continue;
                }

                stream.Position = match.Position;

                var otherStream = File.OpenRead(Path.Combine(SteamFolder, match.BlockStart.Path));
                otherStream.Position = match.BlockStart.Position;

                int i = 0;
                while (stream.ReadByte() == otherStream.ReadByte())
                {
                    i++;
                    if (stream.Position == size)
                    {
                        break;
                    }
                }

                var dstStartOffset = match.Position - 1024;
                var srcStartOffst = match.BlockStart.Position - 1024;
                var length = (i + 1024);

                var block = new MatchBlock(dstStartOffset, srcStartOffst, length, match.BlockStart.Path);
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
