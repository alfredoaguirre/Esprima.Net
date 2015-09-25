using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Esprima.NET;

namespace JavaScript.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var esprima = new Esprima.NET.Esprima();
            var code = @"

function g() {
      alert(g.caller.name); // f
}

   function f() {
      alert(f.caller.name); // undefined
      g();
}

f();";
            var tokenize = esprima.tokenize(code
                , new Options());
            var node = esprima.parse(code, new Options());
        }
        [TestMethod]
        public void TestMethod1AMD()
        {
            var esprima = new Esprima.NET.Esprima();
            var code = @"
define(['dep1', 'dep2'], function (dep1, dep2) {

    //Define the module value by returning a value.
    return function () {};
});";
            var tokenize = esprima.tokenize(code , new Options());


            var node = esprima.parse(code,new Options());
        }
    }
}
