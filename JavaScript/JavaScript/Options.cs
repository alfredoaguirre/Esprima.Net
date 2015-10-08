using System.Collections.Generic;
using Esprima.NET.Nodes;

namespace Esprima.NET
{
    public class Options
    {
        public bool tokens;

        public List<Node> defaults { get; set; }

        public bool inFor { get; set; }
        public string message { get; set; }
        public Token stricted { get; set; }
        public int defaultCount { get; set; }
        public Token firstRestricted { get; set; }
        public bool comment { get; set; }
        public bool tolerant { get; set; }
        public Loc loc { get; set; }
        public string sourceType { get; set; }
        public bool head { get; set; }
        public List<Node> @params { get; set; }
        public bool range { get; set; }
        public bool attachComment { get; set; }
    }
}