using Parser.Grammars;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Parser
{
    public class Lexical
    {
        public string Name { get; set; }
        public List<Right> Right { get; } = new List<Right>();
        public bool IsRexEx { get; set; }
        public Regex Regex { get; set; }
        public bool IsCode { get; set; }
        public bool HasTerminals { get; set; }

        public void AddRgiht(List<String> arg)
        {
            this.Right.Add(new Parser.Right(arg));
        }

        public override string ToString()
        {
            StringBuilder resume = new StringBuilder();
            Right.ForEach(x => resume.Append(x.ToString()));
            return Name + " "
                + (IsRexEx == false ? " ::=" : " ::=-")
                + (HasTerminals == false ? " " : "> ")
                + resume.ToString();
        }
        public bool HasRight(Char rightTag) => HasRight(rightTag.ToString());
        public bool HasRight(String rightTag) => IsRexEx ? MatchRexEx(rightTag) : (IsCode ? MatchCode(rightTag) : Right.Any(x => x.Token.Any(y => y == rightTag)));

        public bool MatchRexEx(string rightTag)
        {
            if (IsRexEx)
            {
                return this.Right.Any(x =>
                {
                    return x.Token.Any(y =>
                    {
                        var regex = y.Remove(0, 1).Remove(y.Length - 2, 1);
                        regex = @"^\p{" + regex + "}$";
                        Regex rgx = new Regex(regex, RegexOptions.IgnoreCase);
                        MatchCollection matches = rgx.Matches(rightTag);
                        return matches.Count > 0;
                    });
                });
            }
            else
            {
                return false;
            }
        }
        public bool MatchCode(string rightTag)
        {
            if (IsCode && rightTag.Length == 1)
            {
                return Right.Any(x =>
                {
                    return x.Token.Any(y =>
                    {
                        var code = y.Remove(0, 1).Remove(y.Length - 2, 1);
                        return Codes.codes[code] == rightTag[0];
                    });
                });
            }
            else
            {
                return false;
            }
        }
    }
    public class Right
    {
        public Right(List<String> arg)
        {
            Token.AddRange(arg);
        }
        public List<String> Token { get; } = new List<string>();
        public override string ToString()
        {
            StringBuilder resume = new StringBuilder();
            this.Token.ForEach(x =>
            {
                resume.AppendLine(string.Join(" ", x) + " || ");
            });
            resume = resume.Remove(resume.Length - 3, 2);
            return resume.ToString();
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
