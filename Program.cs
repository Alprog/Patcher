
using Microsoft.VisualBasic;
using System;

namespace Patcher
{
    public class Program
    {
        static void Main()
        {
            //CreateHashTables();
            //CreateHeaderTable();
            //CreateDiffs();
            //CreatePack();
            //MakePatch();
            MakePatchFromPack();
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

            long total = 0;
            long processed = 0;
            var LastFiles = new List<string>();
            
            foreach (var filePath in FileSystem.ListFiles(Constants.PatchFolder))
            {
                var desc = hashTable.FromFilePath(filePath);
                string oldFilePath = string.Empty;
                if (desc.NewHash == Constants.NoneHash || hashTable.OldHashToFilePathMap.TryGetValue(desc.NewHash, out oldFilePath))
                {
                    if (oldFilePath == string.Empty)
                    {
                        oldFilePath = filePath;
                    }

                    var fullPath = Path.Combine(Constants.DiffFolder, filePath);
                    FileSystem.CreateFolderForPath(fullPath);
                    var stream = File.OpenWrite(fullPath);
                    var writer = new BinaryWriter(stream);
                    writer.Write((byte)BlockType.NewCopyFile);
                    writer.Write((Int16)hashTable.ToIndex(filePath));
                    writer.Write((Int16)hashTable.ToIndex(oldFilePath));
                    writer.Close();
                }
                else
                {
                    var fullPath = Path.Combine(Constants.PatchFolder, filePath);
                    var size = FileSystem.GetFileSize(fullPath);

                    total += size;
                    var diffFilePath = Path.Combine(Constants.DiffFolder, filePath);
                    if (!File.Exists(diffFilePath))
                    {
                        LastFiles.Add(filePath);
                        Console.WriteLine(filePath + " " + size);
                    }
                    else
                    {
                        processed += size;
                    }
                }
            }

            Console.WriteLine("Last files:" + LastFiles.Count);
            Console.WriteLine("Last bytes:" + (total - processed) + "(" + total + ")");
            
            foreach (var filePath in LastFiles)
            {
                var fullPath = Path.Combine(Constants.PatchFolder, filePath);
                var diffFilePath = Path.Combine(Constants.DiffFolder, filePath);
                Console.WriteLine(filePath);
                var differ = new Differ(fullPath, Constants.OriginalFolder, hashTable, headerTable);
                differ.FullProcess();
                differ.Save(filePath, diffFilePath);
            }
        }

        static void CreatePack()
        {
            var stream = File.OpenWrite(Constants.PackFilePath);
            var writer = new BinaryWriter(stream);

            var fileNames = FileSystem.ListFiles(Constants.DiffFolder);
            foreach (var fileName in fileNames)
            {
                var fullPath = Path.Combine(Constants.DiffFolder, fileName);
                var bytes = File.ReadAllBytes(fullPath);
                writer.Write(bytes);
            }

            writer.Close();
        }

        static void MakePatch()
        {
            var hashTable = new HashTable(Constants.HashTableFilePath);
            var extractor = new Extractor(hashTable, Constants.OriginalFolder, Constants.OutputFolder);

            var fileNames = FileSystem.ListFiles(Constants.DiffFolder);
            foreach (var fileName in fileNames)
            {
                var fullName = Path.Combine(Constants.DiffFolder, fileName);
                extractor.ProcessFile(fullName);
            }
        }

        static void MakePatchFromPack()
        {
            var defaultPath = Constants.OriginalFolder;
            Console.WriteLine("Enter encased folder (by default " + defaultPath + "):");

            var path = Console.ReadLine();
            if (path == string.Empty)
            {
                path = defaultPath;
            }
            Console.WriteLine("Check folder: " + path);

            //var hashTable = new HashTable("hashTable.txt");
            //var extractor = new Extractor(hashTable, Constants.OriginalFolder, Constants.OutputFolder);
            //extractor.ProcessFile(Constants.PackFilePath);
        }
    }
}
