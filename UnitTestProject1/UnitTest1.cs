using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using parser;
using System.Linq;
using System.Collections.Generic;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            //creat grammar
            LoadGrammarHelper.LoadGrammar();
            var lexical = Grammar.getFromRight("while");
            var termanal = Grammar.IsTermanl("while");
            var lexical2 = Grammar.getFromRight("Keyword");
            var termanal2 = Grammar.IsTermanl("Keyword");
            var lexical3 = Grammar.Get("while");
            var lexical4 = Grammar.Get("Keyword");

        }
        [TestMethod]
        public void TestMethod2()
        {
            LoadGrammarHelper.LoadGrammar();
            var rokens = Tokenazer.getTokens("5 + 5").ToList();
        }

        [TestMethod]
        public void TestMethod3()
        {
            LoadGrammarHelper.LoadGrammar();
            var rokens = Tokenazer.getTokens("if ( 5 == 3 ) ").ToList();
        }

        [TestMethod]
        public void TestMethod4()
        {
            LoadGrammarHelper.LoadGrammar();
            var rokens = Tokenazer.getTokens("function d ( 5 == 3 ) { var t = 5 ; } ").ToList();
        }

        [TestMethod]
        public void TestMethod5()
        {
            LoadGrammarHelper.LoadGrammar();
            var tokens = Tokenazer.getTokens("if ( 5 == 3 ) { var  5 = 5; } else { var  5 = 5; }").ToList();
            var lexical = Grammar.Get("IfStatement");
            checkLexical(lexical, tokens.GetEnumerator() as IEnumerator<Token>);


        }

        public void checkLexical(Lexical lexical, IEnumerator<Token> tokens)
        {
            var tokenList = tokens;
            Console.WriteLine("lex: " + lexical.Name + " ::=");
            foreach (var l in lexical.Lexs)
            {
                bool valid;
                tokenList = checkLex(tokenList, l, out valid);
            }
        }

        private IEnumerator<Token> checkLex(IEnumerator<Token> tokenList, List<Lex> l, out bool valid)
        {
            valid = false;
            var l2 = l.GetEnumerator() as IEnumerator<Lex>;
            while (l2.MoveNext() && tokenList.MoveNext())
            {
                if (l2.Current.IsTerminal() && tokenList.Current.Name == l2.Current.Name)
                {
                    Console.WriteLine("=> " + tokenList.Current.Name);
                    continue;
                }
                IEnumerator<Token> middle2;
                l2 = MoveToNextTerminal(l2, out IEnumerator<Lex> middle);
                middle.MoveNext();
                Console.WriteLine(middle.Current.Name);
                if (l2.Current != null)
                {
                    tokenList = MoveToNext(l2.Current.Name, tokenList, out middle2);
                }
                else
                {
                    tokenList.Reset();
                    middle2 = tokenList;
                }
                checkLexical(middle.Current.Lexical, middle2);
                if (tokenList.Current.Name == l2.Current.Name)
                {
                    Console.WriteLine("=> " + tokenList.Current.Name);
                    continue;
                }
            }
            valid = true;
            return tokenList;
        }

        public IEnumerator<Token> MoveToNextTerminal(IEnumerator<Token> tokens, out IEnumerator<Token> middle)
        {
            List<Token> middleList = new List<Token>();
            while (tokens.Current != null && !tokens.Current.IsTerminal())
            {
                middleList.Add(tokens.Current);
                tokens.MoveNext();
            }
            middle = middleList.GetEnumerator() as IEnumerator<Token>;
            return tokens;
        }
        public IEnumerator<Token> MoveToNext(string name, IEnumerator<Token> tokens, out IEnumerator<Token> middle)
        {
            List<Token> middleList = new List<Token>();
            while (tokens.Current != null && tokens.Current.Name != name)
            {
                middleList.Add(tokens.Current);
                tokens.MoveNext();
            }
            middle = middleList.GetEnumerator() as IEnumerator<Token>;
            return tokens;
        }

        public IEnumerator<Lex> MoveToNextTerminal(IEnumerator<Lex> Lexs, out IEnumerator<Lex> middle)
        {
            List<Lex> middleList = new List<Lex>();
            while (Lexs.Current != null && !Lexs.Current.IsTerminal())
            {
                middleList.Add(Lexs.Current);
                Lexs.MoveNext();
            }
            middle = middleList.GetEnumerator() as IEnumerator<Lex>;
            return Lexs;
        }
        [TestMethod]
        public void TestMethod6()
        {
            LoadGrammarHelper.LoadGrammar();
          
            var lexical = Grammar.getFromRight("K");
          

        }
    }
}
