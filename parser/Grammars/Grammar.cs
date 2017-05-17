using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Parser.Tokens;
using Parser.Nodes;

namespace Parser.Grammars
{
    public static class Grammar
    {
        static private Dictionary<string, Lexical> _Lexicals = new Dictionary<string, Lexical>();
        static public List<Lexical> Lexicals => _Lexicals.Select(x => x.Value).ToList();
        public static List<Lexical> regExLexicals = new List<Lexical>();
        public static List<Lexical> CodeLexicals = new List<Lexical>();
        public static List<Lexical> TerminalsLexicals = new List<Lexical>();

        static public void Add(Lexical lexical)
        {
            Grammar._Lexicals.Add(lexical.Name, lexical);
        }

        static public Lexical Get(string name)
        {
            if (_Lexicals.Keys.Any(x => x == name))
                return _Lexicals[name];
            return null;
        }
        static public Lexical FindRight(Char rightTag) => FindRight(rightTag.ToString());

        static public Lexical FindRight(String rightTag)
        {
            return Lexicals.First(x => x.HasRight(rightTag));
        }
        static public bool IsTermanl(string name)
        {
            return !_Lexicals.Keys.Any(x => x == name);
        }
        static private List<Lexical> GetAllRexEx()
        {
            return Grammar._Lexicals.Values.Where(x => x.IsRexEx).ToList();
        }

        static IEnumerable<string> GetTerminalTokens()
        {
            var tokensGroups = new List<string>() { "Punctuator", "NullLiteral", "BooleanLiteral", "Keyword", "FutureReservedWord" };
            foreach (var groups in tokensGroups)
                foreach (var lex in Grammar._Lexicals["Punctuator"].Right)
                    foreach (var l in lex.Token)
                        yield return l;
        }

        public static Node GetTree(IEnumerable<Token> tokens, Lexical lexical)
        {
            
            var baseNode = new Node(lexical);
            foreach (var t in lexical.Right.First().Token)
            {
                if (Grammar.IsTermanl(t))
                {
                    //var t2 = tokens.Take(t.Length);
                    //var tokenString = t2.Aggregate<TokenBase, StringBuilder>(new StringBuilder(), (x, y) => x.Append(y.Ch)).ToString();
                    //if (tokenString == t)
                    //{
                    // //   baseNode.
                    //}
                }
            }
            return baseNode;
        }

        public static void GruopsLexical()
        {
            var terminal = Grammar.Lexicals.Where(x => x.HasTerminals || x.IsCode || x.IsRexEx).ToList();
            foreach (var t in terminal)
            {
                if (t.IsRexEx) regExLexicals.Add(t);
                if (t.IsCode) CodeLexicals.Add(t);
                if (t.HasTerminals) TerminalsLexicals.Add(t);
            }

        }
    }
    public static class Codes
    {
        public static Dictionary<string, char> codes = new Dictionary<string, char>
        {
            { "TAB", '\u0009'                          },
            { "VT",'\u000B'                            },
            { "FF",'\u000C'                            },
            { "SP",'\u0020'                            },
            { "BOM", '\uFEFF'                          },
            { "LF" ,'\u000A'                           },
            { "CR" ,'\u000D'                           },
            { "LS" ,'\u2028' },
            { "PS" ,'\u2029'            }
        };
    }
}

