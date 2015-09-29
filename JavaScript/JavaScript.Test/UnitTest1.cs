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
        public void TestMethod1AMD()
        {
            var esprima = new Esprima.NET.Esprima();
            var code = file.ReadToEnd();
            var tokenize = esprima.tokenize(code, new Options());


            var node = esprima.parse(code, new Options());
        }
    }
}
