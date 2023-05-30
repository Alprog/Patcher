
namespace Patcher
{
    public class BlockMatch
    {
        public BlockMatch(long dstPosition, long srcPosition, long length, string srcFile)
        {
            this.DstPosition = dstPosition;
            this.SrcPosition = srcPosition;
            this.Length = length;
            this.SrcFile = srcFile;
        }

        public long DstEndPosition => DstPosition + Length;

        public long DstPosition;
        public long SrcPosition;
        public long Length;
        public string SrcFile;
    }
}
