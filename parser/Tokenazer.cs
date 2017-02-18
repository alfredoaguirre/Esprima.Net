using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parser
{
    public static class Tokenazer
    {
        public static IEnumerable<Token> getTokens(string line)
        {
            return getTokens(line.Split(new Char[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries));
        }
        public static IEnumerable<Token> getTokens(string[] line)
        {
            return line.Select(x => new Token(x));
        }
    }
}
