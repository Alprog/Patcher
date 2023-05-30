
namespace Patcher
{
    public class HeaderMatch
    {
        public HeaderMatch(long position, BlockHeader blockStart)
        {
            this.Position = position;
            this.BlockStart = blockStart;
        }

        public long Position;
        public BlockHeader BlockStart;
    }
}
