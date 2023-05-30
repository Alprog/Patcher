
namespace Patcher
{
    public class HeaderSeacher
    {
        public FileStream Stream;
        public long FileSize;
        public int FastCheckSum;
        private Queue<byte> ByteQueue;

        public long Position => Stream.Position - Constants.HeaderSize;
        public bool IsEOF => Stream.Position >= FileSize;
        public byte[] GetBytes() { return ByteQueue.ToArray(); }

        public HeaderSeacher(string filePath)
        {
            Stream = File.OpenRead(filePath);
            FileSize = new FileInfo(filePath).Length;
            FastCheckSum = 0;
            ByteQueue = new Queue<byte>();

            ReadStart();
        }

        private void ReadStart()
        {
            for (int j = 0; j < Constants.HeaderSize; j++)
            {
                var b = (byte)Stream.ReadByte();
                ByteQueue.Enqueue(b);
                FastCheckSum += b;
            }
        }

        public bool MoveNext()
        {
            if (IsEOF)
            {
                return false;
            }
            else
            {
                FastCheckSum -= ByteQueue.Dequeue();
                var b = (byte)Stream.ReadByte();
                FastCheckSum += b;
                ByteQueue.Enqueue(b);
                return true;
            }
        }
    }
}
