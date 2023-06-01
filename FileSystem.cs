
using System.Security.Cryptography;

namespace Patcher
{
    public static class FileSystem
    {
        public static List<string> ListFiles(string folder)
        {
            int startLength = folder.Length;
            var list = new List<string>();
            foreach (var filePath in Directory.GetFiles(folder, "*", SearchOption.AllDirectories))
            {
                list.Add(filePath.Substring(startLength + 1));
            }
            return list;
        }

        public static string CalcMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        public static void CreateFolderForPath(string current)
        {
            var parent = Path.GetDirectoryName(current);
            if (!Directory.Exists(parent))
            {
                CreateFolderForPath(parent);
                Directory.CreateDirectory(parent);
            }
        }

        public static long GetFileSize(string filePath)
        {
            return new FileInfo(filePath).Length;
        }
    }
}
