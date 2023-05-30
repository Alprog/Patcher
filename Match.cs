
namespace Patcher
{
    public class Match
    {
        public Match(long position, BlockStart blockStart)
        {
            this.Position = position;
            this.BlockStart = blockStart;
        }

        public long Position;
        public BlockStart BlockStart;
    }

    public class MatchBlock
    {
        public MatchBlock(long dstPosition, long srcPosition, long length, string srcFile)
        {
            this.DstPosition = dstPosition;
            this.SrcPosition = srcPosition;
            this.Length = length;
            this.SrcFile = srcFile;
        }

        public long DstEndPosition { get => DstPosition + Length; }

        public long DstPosition;
        public long SrcPosition;
        public long Length;
        public string SrcFile;
    }
}
