
namespace Patcher
{
    public static class Constants
    {
        public static string OriginalFolder = "C:/Steam/steamapps/common/Encased";
        public static string PatchFolder = "C:/Users/alpro/Downloads/v1.4.713.1655";
        public static string DiffFolder = "C:/patcher/data/diff";
        public static string OutputFolder = "C:/patcher/data/output";

        public static string OriginalHashTableFilePath = "C:/patcher/data/originalHashTable.txt";
        public static string PatchHashTableFilePath = "C:/patcher/data/patchHashTable.txt";
        public static string HeaderTableFilePath = "C:/patcher/data/headerTable.txt";

        //public static string SubPath = "Encased_Data/StreamingAssets/Master.bank";
        //public static string SubPath = "Encased_Data/StreamingAssets/AssetBundles/misc";
        public static string SubPath = "Encased_Data/sharedassets76.assets.resS";
        //public static string SubPath = "Encased_Data/level178";

        public static int KiB = 1024;
        public static int MiB = KiB * KiB;

        public static int HeaderSize = KiB;
        public static int HeaderCollectionStep = 64 * KiB;

        public static int SkipAfterMatchSize = 16 * KiB;
        public static int DeepSkipSize = 16 * MiB;
    }
}
