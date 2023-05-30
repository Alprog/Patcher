
namespace Patcher
{
    public class Program
    {
        static void Main()
        {
            //CreateHashTables();
            //CheckTables();
            CreateHeaderTable();
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

        static void CheckTables()
        {
            var original = new HashTable(Constants.OriginalHashTableFilePath);
            var patch = new HashTable(Constants.PatchHashTableFilePath);
            original.Check(patch);
        }

        static void CreateHeaderTable()
        {
            var headerTable = new HeaderTable();
            headerTable.Collect(Constants.OriginalFolder, Constants.HeaderCollectionStep);
            headerTable.Save(Constants.HeaderTableFilePath);
        }

        static void tt()
        {
            //var info = new BlockInfo();
            //info.Collect(SteamFolder, mb / 16);
            //info.Save();

            //var info = new HeaderTable();
            //info.Load();

            //ProcessFile(Path.Combine(PatchFolder, SubPath), info);
        }

    }
}
