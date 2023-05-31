
namespace Patcher
{
    public class FileDesc
    {
        public FileDesc(string path, string oldHash, string newHash)
        {
            this.Path = path;
            this.OldHash = oldHash;
            this.NewHash = newHash;
        }

        public string Path;
        public string OldHash;
        public string NewHash;
    }
}
