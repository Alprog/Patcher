

namespace Patcher
{
    public class BlockInfo
    {
        public static string BlockInfoFilepath = "C:\\Users\\alpro\\source\\repos\\ConsoleApp1\\ConsoleApp1\\blockInfo.txt";

        public List<BlockStart> BlockStarts = new List<BlockStart>();
        public Dictionary<int, List<BlockStart>> Dict = new Dictionary<int, List<BlockStart>>();

        public void Add(BlockStart blockStart)
        {
            BlockStarts.Add(blockStart);
        }

        public void Save()
        {
            var fileStream = File.OpenWrite(BlockInfoFilepath);
            var binaryWriter = new BinaryWriter(fileStream);
            foreach (var blockStart in BlockStarts) 
            {
                blockStart.Save(binaryWriter);
            }
            fileStream.Close();
        }

        public void Load()
        {
            var fileStream = File.OpenRead(BlockInfoFilepath);
            var size = new FileInfo(BlockInfoFilepath).Length;
            var binaryReader = new BinaryReader(fileStream);
            while (fileStream.Position < size)
            {
                Add(BlockStart.Load(binaryReader));
            }
            fileStream.Close();

            foreach (var blockStart in BlockStarts)
            {
                List<BlockStart> list;
                if (!Dict.TryGetValue(blockStart.FastChecksum, out list))
                {
                    list = new List<BlockStart>();
                    Dict[blockStart.FastChecksum] = list;
                }
                list.Add(blockStart);
            }
        }

        public void Collect(string rootFolder, int step)
        {
            var fileNames = new FileSystem().List(rootFolder);
            Console.WriteLine(fileNames.Count);

            int i = 0;
            foreach (var fileName in fileNames)
            {
                Console.WriteLine(++i + " " + fileName);
                Collect(rootFolder, fileName, step);
            }
        }

        public void Collect(string rootFolder, string fileName, int step)
        {
            var fullPath = Path.Combine(rootFolder, fileName);
            var stream = File.OpenRead(fullPath);
            var size = new FileInfo(fullPath).Length;

            int fastCheckSum = 0;
            Queue<byte> byteQueue = new Queue<byte>();
            for (int j = 0; j < 1024; j++)
            {
                var b = (byte)stream.ReadByte();
                byteQueue.Enqueue(b);
                fastCheckSum += b;
            }

            while (stream.Position < size)
            {
                fastCheckSum -= byteQueue.Dequeue();
                var b = (byte)stream.ReadByte();
                fastCheckSum += b;
                byteQueue.Enqueue(b);

                if (stream.Position % step == 0)
                {
                    Add(new BlockStart(byteQueue.ToArray(), fastCheckSum, stream.Position, fileName));
                }
            }
        }
    }
}
