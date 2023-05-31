
namespace Patcher
{
    public class HashTable
    {
        public List<FileDesc> Files = new List<FileDesc>();
        public Dictionary<string, int> PathToIndexMap = new Dictionary<string, int>();
        public Dictionary<string, string> OldHashToFilePathMap = new Dictionary<string, string>();

        public HashTable(string filePath = null)
        {
            if (filePath != null)
            {
                Load(filePath);
            }
        }

        public FileDesc FromFilePath(string filePath)
        {
            return Files[PathToIndexMap[filePath]];
        }

        public FileDesc FromIndex(int index)
        {
            return Files[index];
        }

        public int ToIndex(string subPath)
        {
            return PathToIndexMap[subPath];
        }

        public void Collect(string folder, bool isOld)
        {
            var fileNames = FileSystem.ListFiles(folder);
            int i = 0;
            foreach (var fileName in fileNames)
            {
                var hash = FileSystem.CalcMD5(Path.Combine(folder, fileName));
                if (isOld)
                {
                    AddNew(fileName, hash, Constants.NoneHash);
                }
                else if (PathToIndexMap.TryGetValue(fileName, out int index))
                {
                    Files[index].NewHash = hash;
                }
                else
                {
                    AddNew(fileName, Constants.NoneHash, hash);
                }
                Console.WriteLine(i++);
            }
        }

        public void AddNew(string filePath, string oldHash, string newHash)
        {
            var desc = new FileDesc(filePath, oldHash, newHash);
            Files.Add(desc);
            PathToIndexMap.Add(filePath, Files.Count - 1);
            OldHashToFilePathMap.TryAdd(oldHash, filePath);
        }

        public void Save(string table)
        {
            var list = new List<string>(); 
            foreach (var desc in Files)
            {
                list.Add(desc.Path);
                list.Add(desc.OldHash);
                list.Add(desc.NewHash);
            }
            File.WriteAllLines(table, list);
        }

        public void Load(string tablePath)
        {
            var list = File.ReadAllLines(tablePath).ToList();
            for (int i = 0; i < list.Count; i += 3) 
            {
                var path = list[i];
                var oldHash = list[i + 1];
                var newHash = list[i + 2];
                AddNew(path, oldHash, newHash);            
            }
        }
    }
}
