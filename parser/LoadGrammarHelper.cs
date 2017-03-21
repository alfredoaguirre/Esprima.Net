using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parser
{
    public class LoadGrammarHelper
    {
        public static void LoadGrammar()
        {
            var fileStream = new FileStream(@"C:\Users\Alfredo Aguirre\Documents\Visual Studio 2017\Projects\Esprima.Net\parser\TextFile1.txt", FileMode.Open, FileAccess.Read);
            using (var streamReader = new StreamReader(fileStream, Encoding.UTF8))
            {
                string line;
                Lexical lex = null;
                while ((line = streamReader.ReadLine()) != null)
                {
                    var split = line.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                    if (split.Count() < 2 || split[0].StartsWith("///"))
                    {
                        continue;
                    }
                    if (split[1] == "::=")
                    {
                        if (lex != null) Grammar.Add(lex);
                        lex = new Lexical() { Name = split[0] };
                        lex.AddArg(split.Skip(2).ToList());
                        continue;
                    }

                    if (split[0] == "::=")
                    {
                        lex.AddArg(split.Skip(1).ToList());
                        continue;
                    }

                    if (split[1] == "::=>")
                    {
                        if (lex != null) Grammar.Add(lex);
                        lex = new Lexical() { Name = split[0], HasTerminals = true };
                        split.Skip(2).ToList().ForEach(x => lex.AddArg(new List<string> { x }));
                        continue;
                    }
                    if (split[1] == "::=-") //regEx  or code
                    {
                        if (lex != null) Grammar.Add(lex);
                        lex = new Lexical() { Name = split[0], IsRexEx = true };
                        split.Skip(2).ToList().ForEach(x => lex.AddArg(new List<string> { x }));
                        continue;
                    }
                    if (split[1] == "::==") //code
                    {
                        if (lex != null) Grammar.Add(lex);
                        lex = new Lexical() { Name = split[0], IsCode = true };
                        split.Skip(2).ToList().ForEach(x => lex.AddArg(new List<string> { x }));
                        continue;
                    }
                }
            }
            Grammar.Get("Token");
        }
    }
}


