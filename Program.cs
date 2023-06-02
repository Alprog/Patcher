
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

            Console.ReadLine();
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

        static bool CheckIfExist(string fileName)
        {
            if (!File.Exists(fileName))
            {
                Console.WriteLine(string.Format("Can't find {0}. It should be in the same folder as *.exe file", fileName));
                return false;
            }
            return true;
        }

        static void MakePatchFromPack()
        {
            //Directory.SetCurrentDirectory("C:\\patcher\\data");

            var defaultPath = Constants.OriginalFolder;
            Console.WriteLine("Encased RPG Patch 1.4");

            var hasTableName = "hashTable.txt";
            var packFileName = "pack.bin";
            if (!CheckIfExist(hasTableName) || !CheckIfExist(packFileName))
            {
                return;
            }

            var hashTable = new HashTable("hashTable.txt");

            Console.WriteLine("Enter path to Encased 1.3 directory (by default " + defaultPath + "):");
            Console.Write("> ");

            var steamFolderPath = Console.ReadLine();
            if (steamFolderPath == string.Empty)
            {
                steamFolderPath = defaultPath;
            }

            var i = 0;
            foreach (var desc in hashTable.Files)
            {
                var percent = Math.Round((float)i++ / hashTable.Files.Count * 100);

                if (desc.OldHash != Constants.NoneHash)
                {
                    var fileName = Path.GetFileName(desc.Path);
                    Console.Write(string.Format("{0}% Calculate checksum of {1}... ", percent, fileName));

                    var fullPath = Path.Combine(steamFolderPath, desc.Path);

                    if (!File.Exists(fullPath))
                    {
                        Console.WriteLine("ERROR! No such file");
                        Console.WriteLine("Failed. Make sure that you set correct 1.3 Encased directory");
                        return;
                    }

                    var curHash = FileSystem.CalcMD5(fullPath);

                    if (curHash != desc.OldHash)
                    {
                        Console.WriteLine("ERROR! Wrong checksum");
                        if (curHash == desc.NewHash)
                        {
                            Console.WriteLine("Failed. File looks like already patched to 1.4. Make sure that you specify 1.3 Encased directory");
                        }
                        else
                        {
                            Console.WriteLine("Failed. File looks corrupted. Make sure that you specify 1.3 Encased directory without changes");
                        }
                        return;
                    }

                    Console.WriteLine("done");
                }
            }

            var backupFolderPath = steamFolderPath + "_Backup";

            Console.Write("Creating backup... ");
            try
            {
                new DirectoryInfo(steamFolderPath).MoveTo(backupFolderPath);
                Console.WriteLine("done");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("Make sure that you close SteamApp and any files in game directory. Try rerun with admin rights");
                return;
            }

            var extractor = new Extractor(hashTable, backupFolderPath, steamFolderPath);
            extractor.ProcessFile(packFileName);

            if (extractor.Errors > 0)
            {
                Console.WriteLine(string.Format("Failed. Completed with {0} errors. See log for more info", extractor.Errors));
                Console.WriteLine(string.Format("You can find backup at {0}", backupFolderPath));
                return;
            }

            Console.Write("Deleting backup... ");
            try
            {
                new DirectoryInfo(backupFolderPath).Delete(true);
                Console.WriteLine("done");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

            Console.WriteLine("Successfully patched to 1.4! Enjoy!");
        }
    }
}
