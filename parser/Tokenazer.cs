using System;
using System.Collections.Generic;
using System.IO;
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
        public static IEnumerable<Token> getTokensFromFile(string[] line)
        {
            LoadGrammarHelper.LoadGrammar();
            var terminal = Grammar.Lexicals.Where(x => x.HasTerminals || x.IsCode || x.IsRexEx).ToList();
            using (StreamReader sr = new StreamReader("TestFile.txt"))
            {
                int i = 0;

                var c = new char[1];
                // Read the stream to a string, and write the string to the console.
                while (sr.EndOfStream)
                {
                    sr.Read(c, 0, 1);
                    i++;
                    var t = new Token(new string(c)) { start = i };
                    //Console.WriteLine(line);
                    yield return t;
                }

            }
        }

        public static List<Lexical> regExLexicals = new List<Lexical>();
        public static List<Lexical> CodeLexicals = new List<Lexical>();
        public static List<Lexical> TerminalsLexicals = new List<Lexical>();

        public static void SetUpLexical()
        {
            var terminal = Grammar.Lexicals.Where(x => x.HasTerminals || x.IsCode || x.IsRexEx).ToList();
            foreach (var t in terminal)
            {
                if (t.IsRexEx) regExLexicals.Add(t);
                if (t.IsCode) CodeLexicals.Add(t);
                if (t.HasTerminals) TerminalsLexicals.Add(t);
            }

        }
        public static Lexical GetLexical(char c)
        {
            foreach(var tl in TerminalsLexicals)
            {
                tl.Lexs.Any(x => x[0].Name[0] == c);
            }
           
                return regExLexicals.FirstOrDefault(tl=> tl.MatchRexEx(c));
           
        }
    }
}
