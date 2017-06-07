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
                yield return new TokenBase((char)stream.Read(), n, 1);
                n++;
            }
        }
        public static IEnumerable<Token> GetTokents(IEnumerable<TokenBase> stream)
        {
            Token currenToken = new Token();
            foreach (var tokenbase in stream)
            {
                if (IsBreaker(tokenbase, currenToken))
                {
                    yield return currenToken;
                    currenToken = new Token(tokenbase.Ch.ToString(), tokenbase.Line, tokenbase.Coll);
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
            yield return currenToken;
        }

        static string lastType = null;
        private static bool IsBreaker(TokenBase tokenbase, Token currentToken = null)
        {
            if (lastType == null)
            {
                lastType = tokenbase.Lexical.Name;
                return false;
            }
            if (tokenbase.Lexical.Name != lastType)
            {
                lastType = tokenbase.Lexical.Name;
                return true;
            }
            else
            {
                if (!(tokenbase.Lexical.HasTerminals))
                    return false;
                if (currentToken.Lexical.Right.Any(x => x.Token.Any(y => y == (currentToken.Srt + tokenbase.Ch))))
                    return false;
                else return true;
            }
        }

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
    }
}
