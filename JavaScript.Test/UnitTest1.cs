using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Esprima.NET;
using System.IO;

namespace JavaScript.Test
{
    [TestClass]
    public class UnitTest1
    {
        static StreamReader file = new StreamReader(@"js\example.js");
        [TestMethod]
        public void TestMethod1()
        {
            var esprima = new Esprima.NET.Esprima();
            var code = file.ReadToEnd();
            var tokenize = esprima.tokenize(code, new Options());
            var node = esprima.parse(code, new Options());
        }
        [TestMethod]
        public void TestMethod1ARgo()
        {
            var esprima = new Esprima.NET.Esprima();
            var code = file.ReadToEnd();
            var tokenize = esprima.tokenize(code, new Options());


            var node = esprima.parse(code, new Options());
        }

        [TestMethod]
        public void TestMethod1AMD()
        {
            StreamReader file = new StreamReader(@"js\AMDSimple.js");
            var esprima = new Esprima.NET.Esprima();
            var code = file.ReadToEnd();
            var tokenize = esprima.tokenize(code, new Options());


            var node = esprima.parse(code, new Options());
        }
        [TestMethod]
        public void TestRequireJS()
        {
            StreamReader file = new StreamReader(@"js\RequireJS.js");
            var esprima = new Esprima.NET.Esprima();
            var code = file.ReadToEnd();
            var tokenize = esprima.tokenize(code, new Options());


            var node = esprima.parse(code, new Options());
        }
    }
}
