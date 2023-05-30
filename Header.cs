
namespace Patcher
{
    public class Header
    {
        public string FilePath;
        public long StartPosition;
        public byte[] Bytes;
        public int FastChecksum;

        public Header(string filePath, long startPosition, byte[] bytes, int fastChecksum )
        {
            this.FilePath = filePath;
            this.StartPosition = startPosition;
            this.Bytes = bytes;
            this.FastChecksum = fastChecksum;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(FilePath);
            writer.Write(StartPosition);
            writer.Write(Bytes.Length);
            writer.Write(Bytes);
            writer.Write(FastChecksum);
        }

        public static Header Load(BinaryReader reader)
        {
            var path = reader.ReadString();
            var position = reader.ReadInt64();
            var count = reader.ReadInt32();
            var bytes = reader.ReadBytes(count);
            var fastChecksum = reader.ReadInt32();
            return new Header(path, position, bytes, fastChecksum );
        }
    }
}
