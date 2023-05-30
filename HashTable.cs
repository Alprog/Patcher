
namespace Patcher
{
    public class HashTable
    {
        public List<string> FilePathes = new List<string>();
        public Dictionary<string, string> PathToHashMap = new Dictionary<string, string>();
        public Dictionary<string, int> PathToIndexMap = new Dictionary<string, int>();
        public Dictionary<string, string> HashToPathMap = new Dictionary<string, string>();

        public HashTable(string filePath = null)
        {
            if (filePath != null)
            {
                Load(filePath);
            }
        }

        public int ToIndex(string subPath)
        {
            return PathToIndexMap[subPath];
        }

        public void Collect(string folder)
        {
            var table = new HashTable();
            var fileNames = FileSystem.ListFiles(folder);
            int i = 0;
            foreach (var fileName in fileNames)
            {
                var hash = FileSystem.CalcMD5(Path.Combine(folder, fileName));
                table.Add(fileName, hash);
                Console.WriteLine(i++);
            }
        }        

        public void Add(string filePath, string hash) 
        {
            FilePathes.Add(filePath);
            PathToHashMap.Add(filePath, hash);
            PathToIndexMap.Add(filePath, FilePathes.Count - 1);
            HashToPathMap.TryAdd(hash, filePath);
        }

        public void Save(string table)
        {
            var list = new List<string>(); 
            foreach (var pair in PathToHashMap)
            {
                list.Add(pair.Key);
                list.Add(pair.Value);                           
            }
            File.WriteAllLines(table, list);
        }

        public void Load(string tablePath)
        {
            var list = File.ReadAllLines(tablePath).ToList();
            for (int i = 0; i < list.Count; i += 2) 
            {
                var path = list[i];
                var hash = list[i + 1];

                Add(path, hash);               
            }
        }

        public List<string> Compare(HashTable patch)
        {
            List<string> sameList = new List<string>();
            Dictionary<string, string> renameList = new Dictionary<string, string>();
            List<string> changeList = new List<string>();

            foreach (var patchPair in patch.PathToHashMap)
            {
                var newFile = patchPair.Key;
                var newHash = patchPair.Value;
                if (newHash == this.PathToHashMap[newFile])
                {
                    sameList.Add(newFile);
                }
                else if (this.HashToPathMap.ContainsKey(newHash))
                {
                    renameList.Add(this.HashToPathMap[newHash], newFile);
                }
                else
                {
                    changeList.Add(newFile);
                }
            }

            Console.WriteLine("SAME:");
            foreach (var sameFile in sameList)
            {
                Console.WriteLine(sameFile);
            }

            Console.WriteLine("RENAME:");
            foreach (var pair in renameList)
            {
                Console.WriteLine(pair.Key + " -> " + pair.Value);
            }

            Console.WriteLine("CHANGED:");
            foreach (var changeFile in changeList)
            {
                Console.WriteLine(changeFile);
            }

            Console.WriteLine(sameList.Count + " " + renameList.Count + " " + changeList.Count);

            return changeList;
        }
    }
}
