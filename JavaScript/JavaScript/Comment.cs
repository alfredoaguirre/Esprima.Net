using Esprima.Net;

namespace Esprima.NET
{
    public class Comment
    {
        public string type;
        public string value;
        public Range range;
        public Loc loc { get; set; }
    };
}