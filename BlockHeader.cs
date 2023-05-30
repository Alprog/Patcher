
namespace Patcher
{
    public class BlockHeader
    {
        public byte[] Bytes;
        public int FastChecksum;
        public long StartPosition;
        public string FilePath;

        public BlockHeader(byte[] bytes, int fastChecksum, long startPosition, string filePath)
        {
            this.Bytes = bytes;
            this.FastChecksum = fastChecksum;
            this.StartPosition = startPosition;
            this.FilePath = filePath;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Bytes.Length);
            writer.Write(Bytes);
            writer.Write(FastChecksum);
            writer.Write(StartPosition);
            writer.Write(FilePath);
        }

        public static BlockHeader Load(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            var bytes = reader.ReadBytes(count);
            var fastChecksum = reader.ReadInt32();
            var position = reader.ReadInt64();
            var path = reader.ReadString();
            return new BlockHeader(bytes, fastChecksum, position, path);
        }
    }
}
