

namespace Patcher
{
    public class HeaderTable
    {
        public static string HeaderTableFilePath = "C:/patcher/data/haaderTable.txt";

        public List<Header> Headers = new List<Header>();
        public Dictionary<int, List<Header>> Map = new Dictionary<int, List<Header>>();

        public void Add(Header header)
        {
            Headers.Add(header);
        }

        public void Save()
        {
            var fileStream = File.OpenWrite(HeaderTableFilePath);
            var binaryWriter = new BinaryWriter(fileStream);
            foreach (var header in Headers) 
            {
                header.Save(binaryWriter);
            }
            fileStream.Close();
        }

        public void Load()
        {
            var fileStream = File.OpenRead(HeaderTableFilePath);
            var size = new FileInfo(HeaderTableFilePath).Length;
            var binaryReader = new BinaryReader(fileStream);
            while (fileStream.Position < size)
            {
                Add(Header.Load(binaryReader));
            }
            fileStream.Close();

            foreach (var header in Headers)
            {
                List<Header> list;
                if (!Map.TryGetValue(header.FastChecksum, out list))
                {
                    list = new List<Header>();
                    Map[header.FastChecksum] = list;
                }
                list.Add(header);
            }
        }

        public void Collect(string rootFolder, int step)
        {
            var fileNames = FileSystem.ListFiles(rootFolder);
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
                    Add(new Header(byteQueue.ToArray(), fastCheckSum, stream.Position, fileName));
                }
            }
        }
    }
}
