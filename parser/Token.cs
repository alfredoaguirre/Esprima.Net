using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parser
{
    public class Token
    {
        public string Name { get; }
        public int start { get; set; }
        public int end { get; set; }
        public Token(string str)
        {
            Name = str;
        }
        public Lexical GetLexical
        {
            get
            {
                return Grammar.getFromRight(Name);
            }
        }
        public bool IsTerminal()
        {
            return Grammar.Get(Name) == null ? false : true;
        }
        public override string ToString()
        {
            return Name + " => " + GetLexical;
        }
    }

    public class Token2
    {
        public class TokenLoc
        {
            public class TokenPosition
            {
                public int Line { get; set; }
                public int Column { get; set; }
            }

            public TokenPosition start { get; set; }
            public TokenPosition end { get; set; }
        }

        public class TokenRange
        {
            public int start { get; set; }
            public int end { get; set; }
        }


        public int start { get; set; }
        public int end { get; set; }
        //public TokenType type { get; set; }
        public string value { get; set; }
        internal int lineNumber { get; set; }
        internal int lineStart { get; set; }
        public TokenRange range { get; set; }
        //public Loc loc { get; set; }
        internal bool octal { get; set; }
        internal string literal { get; set; }
        //public Regex regex;
        public int prec { get; set; }

        //public override string ToString()
        //{
        //    return String.Format("{0}[{1}:{2};{3}] {4}", type, lineNumber, start, end, value);
        //}

        public bool head { get; set; }

        //public List<Comment> trailingComments { get; set; }

        //public bool tail { get; set; }

        public Token2()
        {
            range = new TokenRange();
        }

    }
}
