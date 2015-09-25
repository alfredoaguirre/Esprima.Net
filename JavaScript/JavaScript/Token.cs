// ReSharper disable InconsistentNaming
// ReSharper disable ParameterHidesMember
// ReSharper disable ArrangeThisQualifier

using System;
using System.Collections.Generic;

namespace Esprima.NET
{
    public class Token
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
        public TokenType type { get; set; }
        public string value { get; set; }
        internal int lineNumber { get; set; }
        internal int lineStart { get; set; }
        public TokenRange range { get; set; }
        public Loc loc { get; set; }
        internal bool octal { get; set; }
        internal string literal { get; set; }
        public Regex regex;
        public int prec { get; set; }

        public override string ToString()
        {
            return String.Format("{0}[{1}:{2};{3}] {4}", type, lineNumber, range.start, range.end, value);
        }

        public bool head { get; set; }

        public List<Comment> trailingComments { get; set; }

        public bool tail { get; set; }

        public Token()
        {
            range = new TokenRange(); 
        }
           
    }
}