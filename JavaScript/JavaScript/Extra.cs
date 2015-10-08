using System.Collections.Generic;
using Esprima.NET.Nodes;

namespace Esprima.NET
{
    public class Extra : Node
    {
        public Extra()
        {
            this.tokens = new List<Token>();
            this.errors = new List<Error>();
            this.comments = new List<Comment>();
            this.leadingComments = new List<Comment>();
            this.trailingComments = new List<Comment>();

        }

        public List<Token> tokens { get; set; }
        public List<Error> errors { get; set; }
        public List<Comment> comments { get; set; }
        public List<Comment> leadingComments { get; set; }
        public List<Token> bottomRightStack { get; set; }
        public List<Comment> trailingComments { get; set; }
        public bool tokenize { get; set; }
        public bool attachComment { get; set; }
        public int openParenToken { get; set; }
        public bool range { get; set; }
        public int openCurlyToken { get; set; }
        public Loc loc { get; set; }
    }
}