using Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parser.Tokens;

namespace Parser.Nodes
{
    public class Node
    {
        public Lexical lexical;

        public List<Node> Nodes { get; private set; }
        public Node(Lexical lexical)
        {
            this.lexical = lexical;
            this.Nodes = new List<Node>();
        }

    }
    public class Leaf : Node
    {
        string Text { get; set; }
        public List<TokenBase> LeftTokens => new List<TokenBase>();
        public Leaf(Lexical lexical, string text) : base(lexical)
        {

        }
    }
}
