
namespace Patcher
{
    public class BlockStart
    {
        public byte[] Bytes;
        public int FastChecksum;
        public long Position;
        public string Path;

        public BlockStart(byte[] bytes, int fastChecksum, long position, string path)
        {
            this.Bytes = bytes;
            this.FastChecksum = fastChecksum;
            this.Position = position;
            this.Path = path;
        }

        public void Save(BinaryWriter writer)
        {
            writer.Write(Bytes.Length);
            writer.Write(Bytes);
            writer.Write(FastChecksum);
            writer.Write(Position);
            writer.Write(Path);
        }

        public static BlockStart Load(BinaryReader reader)
        {
            var count = reader.ReadInt32();
            var bytes = reader.ReadBytes(count);
            var fastChecksum = reader.ReadInt32();
            var position = reader.ReadInt64();
            var path = reader.ReadString();
            return new BlockStart(bytes, fastChecksum, position, path);
        }
    }
}
