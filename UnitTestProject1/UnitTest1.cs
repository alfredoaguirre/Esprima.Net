using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Parser;
using System.Linq;
using System.Collections.Generic;
using Parser.Tokens;
using Parser.Grammars;
using Parser.Rules;

namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestInitialize]
        public void Setup()
        {
            RulesParser.LoadRoules();
        }


        [TestMethod]
        public void TestMethod1()
        {
            //creat grammar
            var lexical1 = Grammar.FindRight("5");
            var lexical2 = Grammar.FindRight(" ");
            var lexical3 = Grammar.FindRight("+");
            var lexical4 = Grammar.FindRight("-");
            var lexical5 = Grammar.FindRight("g");
            var lexical6 = Grammar.FindRight("|");
            var termanal = Grammar.IsTermanl("while");
            var lexical7 = Grammar.FindRight("Keyword");

            var lexical8 = Grammar.FindRight("if");
            var lexical10 = Grammar.FindRight("else");
            var termanal2 = Grammar.IsTermanl("Keyword");

            var lexical = Grammar.Get("Keyword");

        }
        [TestMethod]
        public void TestMethod2()
        {
            var rokens = Tokenazer.getTokensFromString("5 + 5").ToList();
        }

        [TestMethod]
        public void TestMethod3()
        {
            var rokens = Tokenazer.getTokensFromString("if ( 5 == 3 ) ").ToList();
        }

        [TestMethod]
        public void TestMethod4()
        {
            var tokensbase = Tokenazer.getTokensFromString("function d ( 5 == 3 ) { var t = 5 ; } ").ToList();
            var tokens = Tokenazer.GetTokents(tokensbase).ToList();
        }

        /// <summary>
        /// get token base
        /// </summary>
        [TestMethod]
        public void TestMethod5()
        {
            var tokens = Tokenazer.getTokensFromString("if ( 5 == 3 ) { var  5 = 5; } else { var  5 = 5; }").ToList();
            var lexical = Grammar.Get("IfStatement");

         //   Grammar.GetTree(tokens, lexical);
            
        }
        /// <summary>
        /// get tokens 
        /// </summary>       
        [TestMethod]
        public void TestMethod6()
        {
            var tokensbase = Tokenazer.getTokensFromString("if ( test == 35 ) { var number = 5; } else {  return true; }").ToList();
            var tokens = Tokenazer.GetTokents(tokensbase).ToList();
        }
        [TestMethod]
        public void TestMethod8()
        {
            var tokensbase = Tokenazer.getTokensFromString("if(test==35){var number=5;}else{return true;}").ToList();
            var tokens = Tokenazer.GetTokents(tokensbase).ToList();
        }
        [TestMethod]
        public void TestMethod7()
        {
            var tokensbase = Tokenazer.getTokensFromString("i==-y").ToList();
            var tokens = Tokenazer.GetTokents(tokensbase).ToList();
        }

        //public void checkLexical(Lexical lexical, IEnumerator<Token> tokens)
        //{
        //    var tokenList = tokens;
        //    Console.WriteLine("lex: " + lexical.Name + " ::=");
        //    foreach (var l in lexical.Lexs)
        //    {
        //        bool valid;
        //        tokenList = checkLex(tokenList, l, out valid);
        //    }
        //}

        //private IEnumerator<Token> checkLex(IEnumerator<Token> tokenList, List<Lex> l, out bool valid)
        //{
        //    valid = false;
        //    var l2 = l.GetEnumerator() as IEnumerator<Lex>;
        //    while (l2.MoveNext() && tokenList.MoveNext())
        //    {
        //        if (l2.Current.IsTerminal() && tokenList.Current.Name == l2.Current.Name)
        //        {
        //            Console.WriteLine("=> " + tokenList.Current.Name);
        //            continue;
        //        }
        //        IEnumerator<Token> middle2;
        //        l2 = MoveToNextTerminal(l2, out IEnumerator<Lex> middle);
        //        middle.MoveNext();
        //        Console.WriteLine(middle.Current.Name);
        //        if (l2.Current != null)
        //        {
        //            tokenList = MoveToNext(l2.Current.Name, tokenList, out middle2);
        //        }
        //        else
        //        {
        //            tokenList.Reset();
        //            middle2 = tokenList;
        //        }
        //        checkLexical(middle.Current.Lexical, middle2);
        //        if (tokenList.Current.Name == l2.Current.Name)
        //        {
        //            Console.WriteLine("=> " + tokenList.Current.Name);
        //            continue;
        //        }
        //    }
        //    valid = true;
        //    return tokenList;
        //}

        public IEnumerator<Token> MoveToNextTerminal(IEnumerator<Token> tokens, out IEnumerator<Token> middle)
        {
            List<Token> middleList = new List<Token>();
            while (tokens.Current != null && !tokens.Current.IsTerminal)
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
        //[TestMethod]
        //public void TestMethod6()
        //{
        //    var lexical = Grammar.FindRight("K");
        //}
    }
}
