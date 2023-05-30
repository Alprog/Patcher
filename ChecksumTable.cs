
namespace Patcher
{
    public class ChecksumTable
    {
        public Dictionary<string, string> Hashes = new Dictionary<string, string>();
        public Dictionary<string, string> FileNames = new Dictionary<string, string>();

        public ChecksumTable(string filePath = null)
        {
            if (filePath != null)
            {
                Load(filePath);
            }
        }

        public static void Create(string folder, string tablePath)
        {
            var table = new ChecksumTable();
            var steamList = FileSystem.ListFiles(folder);
            int i = 0;
            foreach (var fileName in steamList)
            {
                var hash = FileSystem.CalcMD5(Path.Combine(folder, fileName));
                table.Add(fileName, hash);
                Console.WriteLine(i++);
            }
            table.Save(tablePath);
        }        

        public void Add(string path, string hash) 
        {
            Hashes.Add(path, hash);
        }

        public void Save(string table)
        {
            var list = new List<string>(); 
            foreach (var pair in Hashes)
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

                Hashes.Add(path, hash);
                FileNames.TryAdd(hash, path);
            }
        }

        public List<string> Check(ChecksumTable patch)
        {
            List<string> sameList = new List<string>();
            Dictionary<string, string> renameList = new Dictionary<string, string>();
            List<string> changeList = new List<string>();

            foreach (var patchPair in patch.Hashes)
            {
                var newFile = patchPair.Key;
                var newHash = patchPair.Value;
                if (newHash == this.Hashes[newFile])
                {
                    sameList.Add(newFile);
                }
                else if (this.FileNames.ContainsKey(newHash))
                {
                    renameList.Add(this.FileNames[newHash], newFile);
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
