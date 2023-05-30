
namespace Patcher
{
    public class HeaderMatch
    {
        public HeaderMatch(long position, Header header)
        {
            this.Position = position;
            this.Header = header;
        }

        public long Position;
        public Header Header;
    }
}
