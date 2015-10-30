namespace Esprima.NET
{
    public class Loc
    {
        public class Position
        {
            public int line { get; set; }
            public int column { get; set; }
        }

        public Position start { get; set; }
        public Position end { get; set; }
        public string source { get; set; }
    }
}