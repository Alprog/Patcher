
namespace Patcher
{
    public class HeaderMatch
    {
        public HeaderMatch(long position, BlockHeader header)
        {
            this.Position = position;
            this.Header = header;
        }

        public long Position;
        public BlockHeader Header;
    }
}
