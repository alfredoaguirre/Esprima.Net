using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Parser.Grammars;

namespace Parser.Tokens
{
    public static class Tokenazer
    {

        public static IEnumerable<TokenBase> getTokensFromString(string line)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(line);
            writer.Flush();
            stream.Position = 0;
            return GetTokents(new StreamReader(stream));
        }

        public static IEnumerable<TokenBase> GetTokents(string filePath)
        {
            return GetTokents(new StreamReader(filePath));
        }
        public static IEnumerable<TokenBase> GetTokents(StreamReader stream)
        {
            long n = 0;
            while (stream.Peek() >= 0)
            {
                yield return new TokenBase((char)stream.Read(), 0, n);
                n++;
            }
        }
        public static IEnumerable<Token> GetTokents(IEnumerable<TokenBase> stream)
        {
            Token currenToken = new Token();
            foreach (var tokenbase in stream)
            {

                if (IsBreaker(tokenbase))
                {
                    if (currenToken != null)
                    {
                        currenToken.UpdateLexical();
                        yield return currenToken;
                    }
                    if (tokenbase.Lexical.Name == "WhiteSpace")
                    {
                        currenToken = null;
                        continue;
                    }
                    yield return new Token(tokenbase.Ch.ToString(), tokenbase.Line, tokenbase.Coll);

                }
                else
                {
                    if (currenToken == null)
                    {
                        currenToken = new Token(tokenbase.Ch.ToString(), tokenbase.Line, tokenbase.Coll);
                    }
                    else
                    {
                        currenToken.Srt += tokenbase.Ch.ToString();
                        currenToken.Coll++;
                    }
                }
            }
        }

        private static bool IsBreaker(TokenBase tokenbase)
        {
            if (tokenbase.Lexical.Name == "Punctuator")
                return true;
            if (tokenbase.Lexical.Name == "WhiteSpace")
                return true;
            return false;
        }


        //public static IEnumerable<Token> GetTokents2(IEnumerable<TokenBase> tokesns)
        //{
        //    tokesns()
        //}
        //public static IEnumerable<Token> getTokensFromFile(string[] line)
        //{
        //    var terminal = Grammar.Lexicals.Where(x => x.HasTerminals || x.IsCode || x.IsRexEx).ToList();
        //    using (StreamReader sr = new StreamReader("TestFile.txt"))
        //    {
        //        int i = 0;

        //        var c = new char[1];
        //        // Read the stream to a string, and write the string to the console.
        //        while (sr.EndOfStream)
        //        {
        //            sr.Read(c, 0, 1);
        //            i++;
        //            var t = new Token(new string(c)) { start = i };
        //            //Console.WriteLine(line);
        //            yield return t;
        //        }

        //    }
        //}

        public static List<Lexical> RegExLexicals { get; } = new List<Lexical>();
        public static List<Lexical> CodeLexicals { get; } = new List<Lexical>();
        public static List<Lexical> TerminalsLexicals { get; } = new List<Lexical>();

        public static void SetUpLexical()
        {
            var terminal = Grammar.Lexicals.Where(x => x.HasTerminals || x.IsCode || x.IsRexEx).ToList();
            foreach (var t in terminal)
            {
                if (t.IsRexEx) RegExLexicals.Add(t);
                if (t.IsCode) CodeLexicals.Add(t);
                if (t.HasTerminals) TerminalsLexicals.Add(t);
            }
        }
        //public static Lexical GetLexical(char c)
        //{
        //    foreach (var tl in TerminalsLexicals)
        //    {
        //        tl.Lexs.Any(x => x[0].Name[0] == c);
        //    }
        //    return RegExLexicals.FirstOrDefault(tl => tl.MatchRexEx(c.ToString()));
        //}
    }
}
