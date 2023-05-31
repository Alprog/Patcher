
namespace Patcher
{
    public class Extractor
    {
        private string OriginalFolder;

        public Extractor(string originalFolder)
        {
            this.OriginalFolder = originalFolder;
        }

        public void Extract(string outputPath, BinaryReader reader)
        {
            FileSystem.CreateFolderForPath(outputPath);

            var outputStream = File.OpenWrite(outputPath);
            var writer = new BinaryWriter(outputStream);
            ProcessBlock(reader, writer);
        }

        public void ProcessBlock(BinaryReader reader, BinaryWriter writer)
        {

        }
    }
}
