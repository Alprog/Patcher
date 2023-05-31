
using System;

namespace Patcher
{
    public class Program
    {
        static void Main()
        {
            //CreateHashTables();
            //CreateHeaderTable();
            CreateDiffs();
            //MakePatch();
        }

        static void CreateHashTables()
        {
            var hashTable = new HashTable();
            hashTable.Collect(Constants.OriginalFolder, true);
            hashTable.Collect(Constants.PatchFolder, false);
            hashTable.Save(Constants.HashTableFilePath);
        }

        static void CreateHeaderTable()
        {
            var headerTable = new HeaderTable();
            headerTable.Collect(Constants.OriginalFolder, Constants.HeaderCollectionStep);
            headerTable.Save(Constants.HeaderTableFilePath);
        }

        static void CreateDiffs()
        {
            var hashTable = new HashTable(Constants.HashTableFilePath);
            var headerTable = new HeaderTable(Constants.HeaderTableFilePath);

            foreach (var filePath in FileSystem.ListFiles(Constants.PatchFolder))
            {
                var desc = hashTable.FromFilePath(filePath);
                if (desc.NewHash == Constants.NoneHash)
                {
                    Console.WriteLine("skip");
                    continue;
                }

                if (hashTable.OldHashToFilePathMap.TryGetValue(desc.NewHash, out string oldFilePath))
                {
                    //Console.WriteLine(filePath + " -> " + oldFilePath);
                    continue;
                }

                var fullPath = Path.Combine(Constants.PatchFolder, filePath);
                var size = new FileInfo(fullPath).Length;
                if (size > Constants.MiB && size < 2 * Constants.MiB)
                {
                    var diffFilePath = Path.Combine(Constants.DiffFolder, filePath);
                    if (!File.Exists(diffFilePath))
                    {
                        Console.WriteLine(filePath);
                        var differ = new Differ(fullPath, Constants.OriginalFolder, hashTable, headerTable);
                        if (differ.FullProcess() > 50)
                        {
                            differ.Save(filePath, diffFilePath);
                        }
                    }
                }
            }
        }

        static void MakePatch()
        {
            var diffFile = Path.Combine(Constants.PatchFolder, Constants.SubPath);
            var outputFile = Path.Combine(Constants.OutputFolder, Constants.SubPath);
            
        }

    }
}
