
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
                var size = FileSystem.GetFileSize(fullPath);
                if (size < 15 * Constants.MiB)
                {
                    var diffFilePath = Path.Combine(Constants.DiffFolder, filePath);
                    if (!File.Exists(diffFilePath))
                    {
                        Console.WriteLine(filePath);
                        var differ = new Differ(fullPath, Constants.OriginalFolder, hashTable, headerTable);
                        differ.FullProcess();
                        differ.Save(filePath, diffFilePath);
                    }
                }
            }
        }

        static void MakePatch()
        {
            var hashTable = new HashTable(Constants.HashTableFilePath);
            var extractor = new Extractor(hashTable, Constants.OriginalFolder, Constants.OutputFolder);

            var fileNames = FileSystem.ListFiles(Constants.DiffFolder);
            foreach (var fileName in fileNames)
            {
                var fullName = Path.Combine(Constants.DiffFolder, fileName);
                //var outputFile = Path.Combine(Constants.OutputFolder, fileName);
                //if (!File.Exists(outputFile))
                {
                    extractor.ProcessFile(fullName);
                }
            }
        }

    }
}
