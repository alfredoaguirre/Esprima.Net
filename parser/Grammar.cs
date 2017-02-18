using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace parser
{
    public static class Grammar
    {
        static public List<Lexical> Lexicals { get; } = new List<Lexical>();
        static public void Add(Lexical lexical)
        {
            Grammar.Lexicals.Add(lexical);
        }

        static public Lexical Get(string name)
        {
            return Lexicals.SingleOrDefault(x => x.Name == name);
        }
        static public Lexical getFromRight(string rightTag)
        {
            return Lexicals.FirstOrDefault(x => x.HasRight(rightTag)) ?
                 GetAllRexEx()
        }
        static public bool IsTermanl(string name)
        {
            return !Lexicals.Any(x => x.Name == name);
        }
        static private List<Lexical> GetAllRexEx()
        {
            return Grammar.Lexicals.Where(x => x.IsRexEx).ToList();
        }
    }

    public class Lexical
    {
        public string Name { get; set; }
        public List<List<Lex>> Lexs { get; set; }
        public bool IsRexEx { get; set; }
        private List<List<string>> LexsString { get; set; }

        public Lexical()
        {
            Lexs = new List<List<Lex>>();
            LexsString = new List<List<string>>();
        }
        public void AddArg(List<String> arg)
        {
            LexsString.Add(arg);
            Lexs.Add(arg.Select(x => new Lex(x)).ToList());
        }

        public override string ToString()
        {
            StringBuilder resume = new StringBuilder();
            this.Lexs.ForEach(x =>
            {
                var line = string.Join(" ", x);
                resume.AppendLine(line + " || ");
            });
            resume = resume.Remove(resume.Length - 3, 2);
            return Name + (IsRexEx == false ? " ::= " : " ::=- ") + resume.ToString();
        }
        public bool HasRight(string rightTag)
        {
            return LexsString.Any(x => x.Any(y => y == rightTag));
        }
    }
    public class Lex
    {
        public string Name { get; set; }

        public Lexical Lexical
        {
            get
            {
                return Grammar.Get(Name);
            }
        }
        private bool? termanal;
        public Lex(string name)
        {
            Name = name;
        }
        public override string ToString()
        {
            return Name;
        }
        public bool IsTerminal()
        {
            var lexical = Grammar.Get(Name);
            if (lexical != null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

}
