using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Esprima.NET;

namespace JavaScriptToNet.Test.utils
{
   public class EsprimaHelper
    {
        public static void parseHelper()
        {
            var jq = @"C:\Users\Alfredo Aguirre\Esprima.Net\JavaScriptToNet.Test\node_modules\jquery\src\core.js";
            StreamReader file = new StreamReader(jq);
           var esprima = new Esprima.NET.Esprima();
            var code = file.ReadToEnd();
            var tokenize = esprima.tokenize(code, new Options());
            var node = esprima.parse(code, new Options());
        }
    }
    
}
