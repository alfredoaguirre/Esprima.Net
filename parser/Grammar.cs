using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace parser
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
        static public Lexical getFromRight(string rightTag)
        {
            var lexical = _Lexicals.FirstOrDefault(x => x.Value.HasRight(rightTag)).Value;
            if (lexical != null)
                return lexical;
            var regExLexicals = GetAllRexEx();
            return regExLexicals.FirstOrDefault(x => x.MatchRexEx(rightTag));
        }
        static public bool IsTermanl(string name)
        {
            return _Lexicals.Keys.Any(x => x == name);
        }
        static private List<Lexical> GetAllRexEx()
        {
            return Grammar._Lexicals.Values.Where(x => x.IsRexEx).ToList();
        }

        static IEnumerable<string> getTerminalTokens()
        {
            var tokensGroups = new List<string>() { "Punctuator", "NullLiteral", "BooleanLiteral", "Keyword", "FutureReservedWord" };
            foreach (var groups in tokensGroups)
                foreach (var lex in Grammar._Lexicals["Punctuator"].Lexs)
                    foreach (var l in lex)
                        yield return l.Name;
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
        public static Char TAB = '\u0009';
        public static Char VT = '\u000B';
        public static Char FF = '\u000C';
        public static Char SP = '\u0020';
        //public static Char #x0a = '\u00A0'    ;
        public static Char BOM = '\uFEFF';
        public static Char USP = '?';
        public static Char LF = '\u000A';
        public static Char CR = '\u000D';
        public static Char LS = '\u2028';
        public static Char PS = '\u2029';
    }
}

