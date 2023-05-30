
namespace Patcher
{
    public class Program
    {
        static void Main()
        {
            //CreateHashTables();
            //CompareHashTables();
            //CreateHeaderTable();
            CreateDiffs();
        }

        static void CreateHashTables()
        {
            var originalTable = new HashTable();
            originalTable.Collect(Constants.OriginalFolder);
            originalTable.Save(Constants.OriginalHashTableFilePath);

            var patchTable = new HashTable();
            patchTable.Collect(Constants.PatchFolder);
            patchTable.Save(Constants.PatchHashTableFilePath);
        }

        static void CompareHashTables()
        {
            var original = new HashTable(Constants.OriginalHashTableFilePath);
            var patch = new HashTable(Constants.PatchHashTableFilePath);
            original.Compare(patch);
        }

        static void CreateHeaderTable()
        {
            var headerTable = new HeaderTable();
            headerTable.Collect(Constants.OriginalFolder, Constants.HeaderCollectionStep);
            headerTable.Save(Constants.HeaderTableFilePath);
        }

        static void CreateDiffs()
        {
            var fullPath = Path.Combine(Constants.PatchFolder, Constants.SubPath);
            var hashTable = new HashTable(Constants.OriginalHashTableFilePath);
            var headerTable = new HeaderTable(Constants.HeaderTableFilePath);

            
            var differ = new Differ(fullPath, Constants.OriginalFolder, hashTable, headerTable);
            differ.FullProcess();

        }

    }
}
