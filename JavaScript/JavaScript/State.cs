using System;
using System.Collections.Generic;

namespace Esprima.NET
{
    public class State
    {
        public State()
        {
            labelSet = new List<string>();
            curlyStack = new Stack<string>();
        }

        public Boolean allowIn;
        public Boolean allowYield;
        public List<string> labelSet;
        public Boolean inFunctionBody;
        public Boolean inIteration;
        public Boolean inSwitch;
        public int lastCommentStart;
        public Stack<string> curlyStack;
        public string sourceType { get; set; }
        public int parenthesizedCount { get; set; }
    }
}