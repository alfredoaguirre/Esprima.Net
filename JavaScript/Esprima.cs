using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Esprima.Net;
using Esprima.NET.Nodes;


namespace Esprima.NET
{

    public class Esprima
    {

        //private int    TokenName;
        private string source;
        private bool strict;
        private int index;
        private int lineNumber;
        private int lineStart;
        private bool hasLineTerminator;
        public static int lastIndex = 0;
        public static int lastLineNumber;
        public static int lastLineStart;
        public static int startIndex;
        private int startLineNumber;
        private int startLineStart;
        private bool scanning;
        private int length;
        private Token lookahead;
        private State state;
        public static Extra extra = new Extra();
        private bool isBindingElement;
        private bool isAssignmentTarget;
        private Token firstCoverInitializedNameError;


        private Dictionary<TokenType, string> TokenName = new Dictionary<TokenType, string>()
        {
            {TokenType.BooleanLiteral, "Boolean"},
            {TokenType.EOF, "<end>"},
            {TokenType.Identifier, "Identifier"},
            {TokenType.Keyword, "Keyword"},
            {TokenType.NullLiteral, "Null"},
            {TokenType.NumericLiteral, "Numeric"},
            {TokenType.Punctuator, "Punctuator"},
            {TokenType.StringLiteral, "String"},
            {TokenType.RegularExpression, "RegularExpression"},
            {TokenType.Template, "Template"},
        };

        // A function following one of those tokens is an expression.
        private readonly List<string> FnExprTokens = new List<string>
        {
            "(",
            "{",
            "[",
            "in",
            "typeof",
            "instanceof",
            "new",
            "return",
            "case",
            "delete",
            "throw",
            "void",
            // assignment operators
            "=",
            "+=",
            "-=",
            "*=",
            "/=",
            "%=",
            "<<=",
            ">>=",
            ">>>=",
            "&=",
            "|=",
            "^=",
            ",",
            // binary/unary operators
            "+",
            "-",
            "*",
            "/",
            "%",
            "++",
            "--",
            "<<",
            ">>",
            ">>>",
            "&",
            "|",
            "^",
            "!",
            "~",
            "&&",
            "||",
            "?",
            ":",
            "==",
            "==",
            ">=",
            "<=",
            "<",
            ">",
            "!=",
            "!=="
        };

        private int _lineStart;

        public static class PlaceHolders
        {
            public const string ArrowParameterPlaceHolder = "ArrowParameterPlaceHolder";
        };

        // Error messages should be identical to V8.
        public static class Messages
        {
            public const string UnexpectedToken = "Unexpected token {0}";
            public const string UnexpectedNumber = "Unexpected number";
            public const string UnexpectedString = "Unexpected string";
            public const string UnexpectedIdentifier = "Unexpected identifier";
            public const string UnexpectedReserved = "Unexpected reserved word";
            public const string UnexpectedTemplate = "Unexpected quasi {0}";
            public const string UnexpectedEOS = "Unexpected end of input";
            public const string NewlineAfterThrow = "Illegal newline after throw";
            public const string InvalidRegExp = "Invalid regular expression";
            public const string UnterminatedRegExp = "Invalid regular expression= missing /";
            public const string InvalidLHSInAssignment = "Invalid left-hand side in assignment";
            public const string InvalidLHSInForIn = "Invalid left-hand side in for-in";
            public const string InvalidLHSInForLoop = "Invalid left-hand side in for-loop";
            public const string MultipleDefaultsInSwitch = "More than one default clause in switch statement";
            public const string NoCatchOrFinally = "Missing catch or finally after try";
            public const string UnknownLabel = "Undefined label \"{0}\"";
            public const string Redeclaration = "{0} \"{1}\" has already been declared";
            public const string IllegalContinue = "Illegal continue statement";
            public const string IllegalBreak = "Illegal break statement";
            public const string IllegalReturn = "Illegal return statement";
            public const string StrictModeWith = "Strict mode code may not include a with statement";
            public const string StrictCatchVariable = "Catch variable may not be eval or arguments in strict mode";
            public const string StrictVarName = "Variable name may not be eval or arguments in strict mode";
            public const string StrictParamName = "Parameter name eval or arguments is not allowed in strict mode";
            public const string StrictParamDupe = "Strict mode function may not have duplicate parameter names";
            public const string StrictFunctionName = "Function name may not be eval or arguments in strict mode";
            public const string StrictOctalLiteral = "Octal literals are not allowed in strict mode.";
            public const string StrictDelete = "Delete of an unqualified identifier in strict mode.";
            public const string StrictLHSAssignment = "Assignment to eval or arguments is not allowed in strict mode";

            public const string StrictLHSPostfix =
                "Postfix increment/decrement may not have eval or arguments operand in strict mode";

            public const string StrictLHSPrefix =
                "Prefix increment/decrement may not have eval or arguments operand in strict mode";

            public const string StrictReservedWord = "Use of future reserved word in strict mode";
            public const string TemplateOctalLiteral = "Octal literals are not allowed in template strings.";
            public const string ParameterAfterRestParameter = "Rest parameter must be last formal parameter";
            public const string DefaultRestParameter = "Unexpected token =";
            public const string ObjectPatternAsRestParameter = "Unexpected token {";
            public const string DuplicateProtoProperty = "Duplicate __proto__ fields are not allowed in object literals";
            public const string ConstructorSpecialMethod = "Class constructor may not be an accessor";
            public const string DuplicateConstructor = "A class may only have one constructor";
            public const string StaticPrototype = "Classes may not have static property named prototype";
            public const string MissingFromClause = "Unexpected token";
            public const string NoAsAfterImportNamespace = "Unexpected token";
            public const string InvalidModuleSpecifier = "Unexpected token";
            public const string IllegalImportDeclaration = "Unexpected token";
            public const string IllegalExportDeclaration = "Unexpected token";
            public const string DuplicateBinding = "Duplicate binding {0}";
        };

        // See also tools/generate-unicode-regex.js.

        // Ensure the condition is true, otherwise throw an error.
        // This is only to have a better contract semantic, i.e. another safety net
        // to catch a logic error. The condition shall be fulfilled in normal case.
        // Do NOT use this to enforce a certain condition on any user input.

        public void assert(bool condition, string message)
        {
            /* istanbul ignore if */
            if (!condition)
            {
                throw new Error("ASSERT: " + message);
            }
        }

        public bool isDecimalDigit(char ch)
        {
            return (ch >= 0x30 && ch <= 0x39); // 0..9
        }

        public bool isHexDigit(char ch)
        {
            return "0123456789abcdefABCDEF".IndexOf(ch) >= 0;
        }

        public bool isOctalDigit(char ch)
        {
            return "01234567".IndexOf(ch) >= 0;
        }

        public class Octal
        {
            public char code;
        }

        // todo: need to fix it
        public Octal octalToDecimal(char ch)
        {
            return new Octal();
        }

        // ECMA-262 11.2 White Space

        public bool isWhiteSpace(char ch)
        {
            
            return (ch == 0x20) || (ch == 0x09) || (ch == 0x0B) || (ch == 0x0C) || (ch == 0xA0);//||
            //      (ch >= 0x1680 && [0x1680, 0x180E, 0x2000, 0x2001, 0x2002, 0x2003, 0x2004, 0x2005, 0x2006, 0x2007, 0x2008, 0x2009, 0x200A, 0x202F, 0x205F, 0x3000, 0xFEFF].indexOf(ch) >= 0);
        }

        // ECMA-262 11.3 Line Terminators

        public bool isLineTerminator(char ch)
        {
            return (ch == 0x0A) || (ch == 0x0D) || (ch == 0x2028) || (ch == 0x2029);
        }

        // ECMA-262 11.6 Identifier Names and Identifiers

        public char fromCodePoint(char cp)
        {
            return (cp < 0x10000)
                ? cp
                : (char)((0xD800 + ((cp - 0x10000) >> 10)) + (0xDC00 + ((cp - 0x10000) & 1023)));
        }

        public bool isIdentifierStart(char ch)
        {
            return (ch == 0x24) || (ch == 0x5F) || // $ (dollar) and _ (underscore)
                   (ch >= 0x41 && ch <= 0x5A) || // A..Z
                   (ch >= 0x61 && ch <= 0x7A) || // a..z
                   (ch == 0x5C) || // \ (backslash)
                   ((ch >= 0x80) && Regex.NonAsciiIdentifierStart.IsMatch(ch.ToString()));
        }

        public bool isIdentifierPart(char ch)
        {
            return (ch == 0x24) || (ch == 0x5F) || // $ (dollar) and _ (underscore)
                   (ch >= 0x41 && ch <= 0x5A) || // A..Z
                   (ch >= 0x61 && ch <= 0x7A) || // a..z
                   (ch >= 0x30 && ch <= 0x39) || // 0..9
                   (ch == 0x5C) || // \ (backslash)
                   ((ch >= 0x80) && Regex.NonAsciiIdentifierPart.IsMatch(ch.ToString()));
        }

        // ECMA-262 11.6.2.2 Future Reserved Words

        public bool isFutureReservedWord(string id)
        {
            switch (id)
            {
                case "enum":
                case "export":
                case "import":
                case "super":
                    return true;
                default:
                    return false;
            }
        }

        public bool isStrictModeReservedWord(string id)
        {
            switch (id)
            {
                case "implements":
                case "interface":
                case "package":
                case "private":
                case "protected":
                case "public":
                case "static":
                case "yield":
                case "let":
                    return true;
                default:
                    return false;
            }
        }

        public bool isRestrictedWord(string id)
        {
            return id == "eval" || id == "arguments";
        }

        // ECMA-262 11.6.2.1 Keywords
        public bool isKeyword(string id)
        {

            // "const" is specialized as Keyword in V8.
            // "yield" and "let" are for compatibility with SpiderMonkey and ES.next.
            // Some others are from future reserved words.

            switch (id.Length)
            {
                case 2:
                    return (id == "if") || (id == "in") || (id == "do");
                case 3:
                    return (id == "var") || (id == "for") || (id == "new") ||
                           (id == "try") || (id == "let");
                case 4:
                    return (id == "this") || (id == "else") || (id == "case") ||
                           (id == "void") || (id == "with") || (id == "enum");
                case 5:
                    return (id == "while") || (id == "break") || (id == "catch") ||
                           (id == "throw") || (id == "const") || (id == "yield") ||
                           (id == "class") || (id == "super");
                case 6:
                    return (id == "return") || (id == "typeof") || (id == "delete") ||
                           (id == "switch") || (id == "export") || (id == "import");
                case 7:
                    return (id == "default") || (id == "finally") || (id == "extends");
                case 8:
                    return (id == "function") || (id == "continue") || (id == "debugger");
                case 10:
                    return (id == "instanceof");
                default:
                    return false;
            }
        }

        // ECMA-262 11.4 Comments

        public void addComment(string type, string value, int start, int end, Loc loc)
        {
            Comment comment;

            // assert(typeof start == "number", "Comment must have valid position");

            state.lastCommentStart = start;

            comment = new Comment()
            {
                type = type,
                value = value
            };
            if (extra.range != null)
            {
                comment.range = new Range() { Start = start, End = end };
            }
            if (extra.loc != null)
            {
                comment.loc = loc;
            }
            extra.comments.Add(comment);
            if (extra.attachComment)
            {
                extra.leadingComments.Add(comment);
                extra.trailingComments.Add(comment);
            }
        }


        public void skipSingleLineComment(int offset)
        {
            int start;
            Loc loc;
            char ch;
            string comment;

            start = index - offset;
            loc = new Loc()
            {
                start = new Loc.Position()
                {
                    line = lineNumber,
                    column = index - lineStart - offset
                }
            };

            while (index < length)
            {
                ch = source.ToCharArray()[index];
                ++index;
                if (isLineTerminator(ch))
                {
                    hasLineTerminator = true;
                    if (extra.comments.Any())
                    {
                        comment = source.Substring(start + offset, index - 1);
                        loc.end = new Loc.Position()
                        {
                            line = lineNumber,
                            column = index - lineStart - 1
                        };
                        addComment("Line", comment, start, index - 1, loc);
                    }
                    if (ch == 13 && source.ToCharArray()[index] == 10)
                    {
                        ++index;
                    }
                    ++lineNumber;
                    lineStart = index;
                    return;
                }
            }

            if (extra.comments.Any())
            {
                comment = source.Substring(start + offset, index);
                loc.end = new Loc.Position()
                {
                    line = lineNumber,
                    column = index - lineStart
                };
                addComment("Line", comment, start, index, loc);
            }
        }

        public void skipMultiLineComment()
        {
            int start = 0;
            Loc loc = new Loc();
            char ch;
            string comment;

            if (extra.comments.Any())
            {
                start = index - 2;
                loc = new Loc()
                {
                    start = new Loc.Position()
                    {
                        line = lineNumber,
                        column = index - lineStart - 2
                    }
                };
            }

            while (index < length)
            {
                ch = source.ToCharArray()[index];
                if (isLineTerminator(ch))
                {
                    if (ch == 0x0D && source.ToCharArray()[index + 1] == 0x0A)
                    {
                        ++index;
                    }
                    hasLineTerminator = true;
                    ++lineNumber;
                    ++index;
                    lineStart = index;
                }
                else if (ch == 0x2A)
                {
                    // Block comment ends with "*/".
                    if (source.ToCharArray()[index + 1] == 0x2F)
                    {
                        ++index;
                        ++index;
                        if (extra.comments.Any())
                        {
                            comment = source.Substring(start + 2, index - 2);
                            loc.end = new Loc.Position()
                            {
                                line = lineNumber,
                                column = index - lineStart
                            };
                            addComment("Block", comment, start, index, loc);
                        }
                        return;
                    }
                    ++index;
                }
                else
                {
                    ++index;
                }
            }

            // Ran off the end of the file - the whole thing is a comment
            if (extra.comments.Any())
            {
                loc.end = new Loc.Position()
                {
                    line = lineNumber,
                    column = index - lineStart
                };
                comment = source.Substring(start + 2, index);
                addComment("Block", comment, start, index, loc);
            }
            tolerateUnexpectedToken();
        }

        public void skipComment()
        {
            char ch;
            bool start;

            hasLineTerminator = false;

            start = (index == 0);
            while (index < length)
            {
                ch = source[index];

                if (isWhiteSpace(ch))
                {
                    ++index;
                }
                else if (isLineTerminator(ch))
                {
                    hasLineTerminator = true;
                    ++index;
                    if (ch == 0x0D && source.ToCharArray()[index] == 0x0A)
                    {
                        ++index;
                    }
                    ++lineNumber;
                    lineStart = index;
                    start = true;
                }
                else if (ch == 0x2F)
                {
                    // U+002F is "/"
                    ch = source.ToCharArray()[index + 1];
                    if (ch == 0x2F)
                    {
                        ++index;
                        ++index;
                        skipSingleLineComment(2);
                        start = true;
                    }
                    else if (ch == 0x2A)
                    {
                        // U+002A is "*"
                        ++index;
                        ++index;
                        skipMultiLineComment();
                    }
                    else
                    {
                        break;
                    }
                }
                else if (start && ch == 0x2D)
                {
                    // U+002D is "-"
                    // U+003E is ">"
                    if ((source.ToCharArray()[index + 1] == 0x2D) && (source.ToCharArray()[index + 2] == 0x3E))
                    {
                        // "-->" is a single-line comment
                        index += 3;
                        skipSingleLineComment(3);
                    }
                    else
                    {
                        break;
                    }
                }
                else if (ch == 0x3C)
                {
                    // U+003C is "<"
                    if (source.Substring(index + 1,  3) == "!--")
                    {
                        ++index; // `<`
                        ++index; // `!`
                        ++index; // `-`
                        ++index; // `-`
                        skipSingleLineComment(4);
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
        }

        public char scanHexEscape(char prefix)
        {
            int i;
            int len;
            char ch;
            int code = 0;

            len = (prefix == 'u') ? 4 : 2;
            for (i = 0; i < len; ++i)
            {
                if (index < length && isHexDigit(source[index]))
                {
                    ch = source[index++];
                    code = code * 16 + "0123456789abcdef".IndexOf(char.ToLower(ch));
                }
                else
                {
                    return '\0';
                }
            }
            return Convert.ToChar(code);
        }

        public char scanUnicodeCodePointEscape()
        {
            char ch;
            int code;

            ch = source[index];
            code = 0;

            // At least, one hex digit is required.
            if (ch == '}')
            {
                throwUnexpectedToken();
            }

            while (index < length)
            {
                ch = source[index++];
                if (!isHexDigit(ch))
                {
                    break;
                }
                code = code * 16 + "0123456789abcdef".IndexOf(Char.ToLower(ch));
            }

            if (code > 0x10FFFF || ch != '}')
            {
                throwUnexpectedToken();
            }

            return fromCodePoint((char)code);
        }

        public char codePointAt(int i)
        {
            char cp;
            char first;
            char second;

            cp = source.ToCharArray()[i];
            if (cp >= 0xD800 && cp <= 0xDBFF)
            {
                second = source.ToCharArray()[i + 1];
                if (second >= 0xDC00 && second <= 0xDFFF)
                {
                    first = cp;
                    cp = (char)((first - 0xD800) * 0x400 + second - 0xDC00 + 0x10000);
                }
            }

            return cp;
        }

        public string getComplexIdentifier()
        {
            char cp;
            char ch;
            string id;

            cp = codePointAt(index);
            id = fromCodePoint(cp).ToString();
            index += id.Length;

            // "\u" (U+005C, U+0075) denotes an escaped character.
            if (cp == 0x5C)
            {
                if (source.ToCharArray()[index] != 0x75)
                {
                    throwUnexpectedToken();
                }
                ++index;
                if (source[index] == '{')
                {
                    ++index;
                    ch = scanUnicodeCodePointEscape();
                }
                else
                {
                    ch = scanHexEscape('u');
                    cp = ch;
                    if (ch != '\0' || ch == '\\' || !isIdentifierStart(cp))
                    {
                        throwUnexpectedToken();
                    }
                }
                id = ch.ToString();
            }

            while (index < length)
            {
                cp = codePointAt(index);
                if (!isIdentifierPart(cp))
                {
                    break;
                }
                ch = fromCodePoint(cp);
                id += ch;
                index++; //+= ch.length;

                // "\u" (U+005C, U+0075) denotes an escaped character.
                if (cp == 0x5C)
                {
                    id = id.Substring(0, id.Length - 1);
                    if (source[index] != 0x75)
                    {
                        throwUnexpectedToken();
                    }
                    ++index;
                    if (source[index] == '{')
                    {
                        ++index;
                        ch = scanUnicodeCodePointEscape();
                    }
                    else
                    {
                        ch = scanHexEscape('u');
                        cp = ch;
                        if (ch != '\0' || ch == '\\' || !isIdentifierPart(cp))
                        {
                            throwUnexpectedToken();
                        }
                    }
                    id += ch;
                }
            }

            return id;
        }

        public string getIdentifier()
        {
            int start;
            char ch;

            start = index++;
            while (index < length)
            {
                ch = source[index];
                if (ch == 0x5C)
                {
                    // Blackslash (U+005C) marks Unicode escape sequence.
                    index = start;
                    return getComplexIdentifier();
                }
                else if (ch >= 0xD800 && ch < 0xDFFF)
                {
                    // Need to handle surrogate pairs.
                    index = start;
                    return getComplexIdentifier();
                }
                if (isIdentifierPart(ch))
                {
                    ++index;
                }
                else
                {
                    break;
                }
            }

            return source.Substring(start, index - start);
        }

        public Token scanIdentifier()
        {
            int start;
            string id;
            TokenType type;

            start = index;

            // Backslash (U+005C) starts an escaped character.
            id = (source[index] == 0x5C) ? getComplexIdentifier() : getIdentifier();

            // There is no keyword or literal with only one character.
            // Thus, it must be an identifier.
            if (id.Count() == 1)
            {
                type = TokenType.Identifier;
            }
            else if (isKeyword(id))
            {
                type = TokenType.Keyword;
            }
            else if (id == "null")
            {
                type = TokenType.NullLiteral;
            }
            else if (id == "true" || id == "false")
            {
                type = TokenType.BooleanLiteral;
            }
            else
            {
                type = TokenType.Identifier;
            }

            return new Token()
            {
                type = type,
                value = id,
                lineNumber = lineNumber,
                lineStart = lineStart,
                start = start,
                end = index
            };
        }


        // ECMA-262 11.7 Punctuators

        public Token scanPunctuator()
        {
            Token token;
            string str;

            token = new Token()
            {
                type = TokenType.Punctuator,
                value = "",
                lineNumber = lineNumber,
                lineStart = lineStart,
                start = index,
                end = index
            };

            // Check for most common single-character punctuators.
            str = source.Substring(index, 1);
            switch (str)
            {

                case "(":
                    if (extra.tokenize)
                    {
                        extra.openParenToken = extra.tokens.Count;
                    }
                    ++index;
                    break;

                case "{":
                    if (extra.tokenize)
                    {
                        extra.openCurlyToken = extra.tokens.Count;
                    }
                    state.curlyStack.Push("{");
                    ++index;
                    break;

                case ".":
                    ++index;
                    if (source[index] == '.' && source[index + 1] == '.')
                    {
                        // Spread operator: ...
                        index += 2;
                        str = "...";
                    }
                    break;

                case "}":
                    ++index;
                    state.curlyStack.Pop();
                    break;
                case ")":
                case ";":
                case ",":
                case "[":
                case "]":
                case ":":
                case "?":
                case "~":
                    ++index;
                    break;

                default:
                    // 4-character punctuator.
                    str = source.Substring(index, 4);
                    if (str == ">>>=")
                    {
                        index += 4;
                    }
                    else
                    {

                        // 3-character punctuators.
                        str = str.Substring(0, 3);
                        if (str == "==" || str == "!==" || str == ">>>" ||
                            str == "<<=" || str == ">>=")
                        {
                            index += 3;
                        }
                        else
                        {

                            // 2-character punctuators.
                            str = str.Substring(0, 2);
                            if (str == "&&" || str == "||" || str == "==" || str == "!=" ||
                                str == "+=" || str == "-=" || str == "*=" || str == "/=" ||
                                str == "++" || str == "--" || str == "<<" || str == ">>" ||
                                str == "&=" || str == "|=" || str == "^=" || str == "%=" ||
                                str == "<=" || str == ">=" || str == "=>")
                            {
                                index += 2;
                            }
                            else
                            {

                                // 1-character punctuators.
                                str = source[index].ToString();
                                if ("<>=!+-*%&|^/".IndexOf(str) >= 0)
                                {
                                    ++index;
                                }
                            }
                        }
                    }
                    break;
            }

            if (index == token.start)
            {
                throwUnexpectedToken();
            }

            token.end = index;
            token.value = str;
            return token;
        }

        // ECMA-262 11.8.3 Numeric Literals

        public Token scanHexLiteral(int start)
        {
            var number = "";

            while (index < length)
            {
                if (!isHexDigit(source[index]))
                {
                    break;
                }
                number += source[index++];
            }

            if (number.Length == 0)
            {
                throwUnexpectedToken();
            }

            if (isIdentifierStart(source[index]))
            {
                throwUnexpectedToken();
            }

            return new Token()
            {
                type = TokenType.NumericLiteral,
                value = Convert.ToInt32(number, 16).ToString(),
                lineNumber = lineNumber,
                lineStart = lineStart,
                start = start,
                end = index
            };
        }

        public Token scanBinaryLiteral(int start)
        {
            char ch;
            string number;

            number = "";

            while (index < length)
            {
                ch = source[index];
                if (ch != '0' && ch != '1')
                {
                    break;
                }
                number += source[index++];
            }

            if (number.Length == 0)
            {
                // only 0b or 0B
                throwUnexpectedToken();
            }

            if (index < length)
            {
                ch = source.ToCharArray()[index];
                /* istanbul ignore else */
                if (isIdentifierStart(ch) || isDecimalDigit(ch))
                {
                    throwUnexpectedToken();
                }
            }

            return new Token()
            {
                type = TokenType.NumericLiteral,
                value = Convert.ToInt32(number, 2).ToString(),
                lineNumber = lineNumber,
                lineStart = lineStart,
                start = start,
                end = index
            };
        }

        public Token scanOctalLiteral(char prefix, int start)
        {
            string number;
            bool octal;

            if (isOctalDigit(prefix))
            {
                octal = true;
                number = "0" + source[index++];
            }
            else
            {
                octal = false;
                ++index;
                number = "";
            }

            while (index < length)
            {
                if (!isOctalDigit(source[index]))
                {
                    break;
                }
                number += source[index++];
            }

            if (!octal && number.Length == 0)
            {
                // only 0o or 0O
                throwUnexpectedToken();
            }

            if (isIdentifierStart(source.ToCharArray()[index]) || isDecimalDigit(source.ToCharArray()[index]))
            {
                throwUnexpectedToken();
            }

            return new Token()
            {
                type = TokenType.NumericLiteral,
                value = Convert.ToInt32(number, 8).ToString(),
                octal = octal,
                lineNumber = lineNumber,
                lineStart = lineStart,
                start = start,
                end = index
            };
        }

        public bool isImplicitOctalLiteral()
        {
            int i;
            char ch;

            // Implicit octal, unless there is a non-octal digit.
            // (Annex B.1.1 on Numeric Literals)
            for (i = index + 1; i < length; ++i)
            {
                ch = source[i];
                if (ch == '8' || ch == '9')
                {
                    return false;
                }
                if (!isOctalDigit(ch))
                {
                    return true;
                }
            }

            return true;
        }

        public Token scanNumericLiteral()
        {
            string number;
            int start;
            char ch;

            ch = source[index];
            assert(char.IsDigit(ch) || (ch == '.'),
                "Numeric literal must start with a decimal digit or a decimal point");

            start = index;
            number = "";
            if (ch != '.')
            {
                number = source[index++].ToString();
                ch = source[index];

                // Hex number starts with "0x".
                // Octal number starts with "0".
                // Octal number in ES6 starts with "0o".
                // Binary number in ES6 starts with "0b".
                if (number == "0")
                {
                    if (ch == 'x' || ch == 'X')
                    {
                        ++index;
                        return scanHexLiteral(start);
                    }
                    if (ch == 'b' || ch == 'B')
                    {
                        ++index;
                        return scanBinaryLiteral(start);
                    }
                    if (ch == 'o' || ch == 'O')
                    {
                        return scanOctalLiteral(ch, start);
                    }

                    if (isOctalDigit(ch))
                    {
                        if (isImplicitOctalLiteral())
                        {
                            return scanOctalLiteral(ch, start);
                        }
                    }
                }

                while (isDecimalDigit(source.ToCharArray()[index]))
                {
                    number += source[index++];
                }
                ch = source[index];
            }

            if (ch == '.')
            {
                number += source[index++];
                while (isDecimalDigit(source.ToCharArray()[index]))
                {
                    number += source[index++];
                }
                ch = source[index];
            }

            if (ch == 'e' || ch == 'E')
            {
                number += source[index++];

                ch = source[index];
                if (ch == '+' || ch == '-')
                {
                    number += source[index++];
                }
                if (isDecimalDigit(source[index]))
                {
                    while (isDecimalDigit(source[index]))
                    {
                        number += source[index++];
                    }
                }
                else
                {
                    throwUnexpectedToken();
                }
            }

            if (isIdentifierStart(source[index]))
            {
                throwUnexpectedToken();
            }

            return new Token
            {
                type = TokenType.NumericLiteral,
                //value= parseFloat(number),
                value = number,
                lineNumber = lineNumber,
                lineStart = lineStart,
                start = start,
                end = index
            };
        }

        // ECMA-262 11.8.4 String Literals
        public Token scanStringLiteral()
        {
            string str = "";
            char quote;
            int start;
            char ch;
            char unescaped;
            Octal octToDec;
            bool octal = false;

            quote = source[index];
            assert((quote == '\"' || quote == '\''),
                "String literal must starts with a quote");

            start = index;
            ++index;

            while (index < length)
            {
                ch = source[index++];

                if (ch == quote)
                {
                    quote = '\0';
                    break;
                }
                else if (ch == '\\')
                {
                    ch = source[index++];
                    if (ch != 0 || !isLineTerminator(ch))
                    {
                        switch (ch)
                        {
                            case 'u':
                            case 'x':
                                if (source[index] == '{')
                                {
                                    ++index;
                                    str += scanUnicodeCodePointEscape();
                                }
                                else
                                {
                                    unescaped = scanHexEscape(ch);
                                    if (unescaped != '\0')
                                    {
                                        throwUnexpectedToken();
                                    }
                                    str += unescaped;
                                }
                                break;
                            case 'n':
                                str += "\n";
                                break;
                            case 'r':
                                str += "\r";
                                break;
                            case 't':
                                str += "\t";
                                break;
                            case 'b':
                                str += "\b";
                                break;
                            case 'f':
                                str += "\f";
                                break;
                            case 'v':
                                str += "\x0B";
                                break;
                            case '8':
                            case '9':
                                str += ch;
                                tolerateUnexpectedToken();
                                break;

                            default:
                                if (isOctalDigit(ch))
                                {
                                    octToDec = octalToDecimal(ch);

                                    // octal = octToDec.octal || octal;
                                    str += new string(new char[] { octToDec.code });
                                }
                                else
                                {
                                    str += ch;
                                }
                                break;
                        }
                    }
                    else
                    {
                        ++lineNumber;
                        if (ch == '\r' && source[index] == '\n')
                        {
                            ++index;
                        }
                        lineStart = index;
                    }
                }
                else if (isLineTerminator(ch))
                {
                    break;
                }
                else
                {
                    str += ch;
                }
            }

            if (quote != '\0')
            {
                throwUnexpectedToken();
            }

            return new Token()
            {
                type = TokenType.StringLiteral,
                value = str,
                octal = octal,
                lineNumber = startLineNumber,
                lineStart = startLineStart,
                start = start,
                end = index
            };
        }

        // ECMA-262 11.8.6 Template Literal Lexical Components
        public Token scanTemplate()
        {
            var cooked = "";
            char ch;
            int start;
            int rawOffset;
            bool terminated;
            bool head;
            bool tail;
            int restore, unescaped;

            terminated = false;
            tail = false;
            start = index;
            head = (source[index] == '`');
            rawOffset = 2;

            ++index;

            while (index < length)
            {
                ch = source[index++];
                if (ch == '`')
                {
                    rawOffset = 1;
                    tail = true;
                    terminated = true;
                    break;
                }
                else if (ch == '$')
                {
                    if (source[index] == '{')
                    {
                        state.curlyStack.Push("{");
                        ++index;
                        terminated = true;
                        break;
                    }
                    cooked += ch;
                }
                else if (ch == '\\')
                {
                    ch = source[index++];
                    if (!isLineTerminator(ch))
                    {
                        switch (ch)
                        {
                            case 'n':
                                cooked += "\n";
                                break;
                            case 'r':
                                cooked += "\r";
                                break;
                            case 't':
                                cooked += "\t";
                                break;
                            case 'u':
                            case 'x':
                                if (source[index] == '{')
                                {
                                    ++index;
                                    cooked += scanUnicodeCodePointEscape();
                                }
                                else
                                {
                                    restore = index;
                                    unescaped = scanHexEscape(ch);
                                    if (unescaped != 0)
                                    {
                                        cooked += unescaped;
                                    }
                                    else
                                    {
                                        index = restore;
                                        cooked += ch;
                                    }
                                }
                                break;
                            case 'b':
                                cooked += "\b";
                                break;
                            case 'f':
                                cooked += "\f";
                                break;
                            case 'v':
                                cooked += "\v";
                                break;

                            default:
                                if (ch == '0')
                                {
                                    if (isDecimalDigit(source.ToCharArray()[index]))
                                    {
                                        // Illegal: \01 \02 and so on
                                        throwError(Messages.TemplateOctalLiteral);
                                    }
                                    cooked += "\0";
                                }
                                else if (isOctalDigit(ch))
                                {
                                    // Illegal: \1 \2
                                    throwError(Messages.TemplateOctalLiteral);
                                }
                                else
                                {
                                    cooked += ch;
                                }
                                break;
                        }
                    }
                    else
                    {
                        ++lineNumber;
                        if (ch == '\r' && source[index] == '\n')
                        {
                            ++index;
                        }
                        lineStart = index;
                    }
                }
                else if (isLineTerminator(ch))
                {
                    ++lineNumber;
                    if (ch == '\r' && source[index] == '\n')
                    {
                        ++index;
                    }
                    lineStart = index;
                    cooked += "\n";
                }
                else
                {
                    cooked += ch;
                }
            }

            if (!terminated)
            {
                throwUnexpectedToken();
            }

            if (!head)
            {
                state.curlyStack.Pop();
            }

            return new Token()
            {
                type = TokenType.Template,
                //value= new  {
                //    cooked= cooked,
                //    raw= source.Substring(start + 1, index - rawOffset)
                //},
                head = head,
                tail = tail,
                lineNumber = lineNumber,
                lineStart = lineStart,
                start = start,
                end = index
            };
        }

        // ECMA-262 11.8.5 Regular Expression Literals
        public Regex testRegExp(string pattern, string flags)
        {
            // The BMP character to use as a replacement for astral symbols when
            // translating an ES6 "u"-flagged pattern to an ES5-compatible
            // approximation.
            // Note: replacing with "\uFFFF" enables false positives in unlikely
            // scenarios. For example, `[\u{1044f}-\u{10440}]` is an invalid
            // pattern that would not be detected by this substitution.
            var astralSubstitute = "\uFFFF";
            string tmp = pattern;

            if (flags.IndexOf("u") >= 0)
            {
                tmp = tmp;
                // Replace every Unicode escape sequence with the equivalent
                // BMP character or a constant ASCII code point in the case of
                // astral symbols. (See the above note on `astralSubstitute`
                // for more information.)
                //.replace(/\\u\{([0-9a-fA-F]+)\}|\\u([a-fA-F0-9]{4})/g, function ($0, $1, $2) {
                //    var codePoint = parseInt($1 || $2, 16);
                //    if (codePoint > 0x10FFFF) {
                //        throwUnexpectedToken(null, Messages.InvalidRegExp);
                //    }
                //    if (codePoint <= 0xFFFF) {
                //        return String.fromCharCode(codePoint);
                //    }
                //    return astralSubstitute;
                //})
                //// Replace each paired surrogate with a single ASCII symbol to
                //// avoid throwing on regular expressions that are only valid in
                //// combination with the "u" flag.
                //.replace(
                //    "/[\uD800-\uDBFF][\uDC00-\uDFFF]/g",
                //    astralSubstitute
                //);
            }

            // First, detect invalid regular expressions.
            try
            {
                new Regex() { flags = flags, pattern = pattern };
            }
            catch (Exception)
            {
                throwUnexpectedToken(null, Messages.InvalidRegExp);
            }

            // Return a regular expression object for this pattern-flag pair, or
            // `null` in case the current environment doesn"t support the flags it
            // uses.
            try
            {
                return new Regex() { pattern = pattern, flags = flags };
            }
            catch (Exception exception)
            {
                return null;
            }
        }

        public Token scanRegExpBody()
        {
            char ch;
            String str;
            bool classMarker;
            bool terminated;
            string body;

            ch = source[index];
            assert(ch == '/', "Regular expression literal must start with a slash");
            str = source[index++].ToString();

            classMarker = false;
            terminated = false;
            while (index < length)
            {
                ch = source[index++];
                str += ch;
                if (ch == '\0')
                {
                    ch = source[index++];
                    // ECMA-262 7.8.5
                    if (isLineTerminator(ch))
                    {
                        throwUnexpectedToken(null, Messages.UnterminatedRegExp);
                    }
                    str += ch;
                }
                else if (isLineTerminator(ch))
                {
                    throwUnexpectedToken(null, Messages.UnterminatedRegExp);
                }
                else if (classMarker)
                {
                    if (ch == ']')
                    {
                        classMarker = false;
                    }
                }
                else
                {
                    if (ch == '/')
                    {
                        terminated = true;
                        break;
                    }
                    else if (ch == '[')
                    {
                        classMarker = true;
                    }
                }
            }

            if (!terminated)
            {
                throwUnexpectedToken(null, Messages.UnterminatedRegExp);
            }

            // Exclude leading and trailing slash.
            body = str.Substring(1, str.Length - 2);
            return new Token()
            {
                value = body,
                literal = str
            };
        }

        public Token scanRegExpFlags()
        {
            char ch;
            string str;
            char flags;
            int restore;

            str = "";
            flags = '\0';
            while (index < length)
            {
                ch = source[index];
                if (!isIdentifierPart(ch))
                {
                    break;
                }

                ++index;
                if (ch == '\\' && index < length)
                {
                    ch = source[index];
                    if (ch == 'u')
                    {
                        ++index;
                        restore = index;
                        ch = scanHexEscape('u');
                        if (ch != '\0')
                        {
                            flags += ch;
                            for (str += "\\u"; restore < index; ++restore)
                            {
                                str += source[restore];
                            }
                        }
                        else
                        {
                            index = restore;
                            flags += 'u';
                            str += "\\u";
                        }
                        tolerateUnexpectedToken();
                    }
                    else
                    {
                        str += "\\";
                        tolerateUnexpectedToken();
                    }
                }
                else
                {
                    flags += ch;
                    str += ch;
                }
            }

            return new Token()
            {
                value = flags.ToString(),
                literal = str
            };
        }

        public Token scanRegExp()
        {
            int start;
            Token body, flags;
            Regex value;
            scanning = true;

            lookahead = null;
            skipComment();
            start = index;

            body = scanRegExpBody();
            flags = scanRegExpFlags();
            value = testRegExp(body.value, flags.value);
            scanning = false;
            if (extra.tokenize)
            {
                return new Token()
                {
                    type = TokenType.RegularExpression,
                    //value= value,
                    regex = new Regex()
                    {
                        pattern = body.value,
                        flags = flags.value
                    },
                    lineNumber = lineNumber,
                    lineStart = lineStart,
                    start = start,
                    end = index
                };
            }

            return new Token()
            {
                literal = body.literal + flags.literal,
                //value  = value,
                regex = new Regex()
                {
                    pattern = body.value,
                    flags = flags.value
                },
                start = start,
                end = index
            };
        }

        public Token collectRegex()
        {
            int pos;
            Loc loc;
            Token regex;
            Token token;

            skipComment();

            pos = index;
            loc = new Loc()
            {
                start = new Loc.Position()
                {
                    line = lineNumber,
                    column = index - lineStart
                }
            };

            regex = scanRegExp();

            loc.end = new Loc.Position()
            {
                line = lineNumber,
                column = index - lineStart
            };

            /* istanbul ignore next */
            if (!extra.tokenize)
            {
                // Pop the previous token, which is likely "/" or "/="
                if (extra.tokens.Count > 0)
                {
                    token = extra.tokens[extra.tokens.Count - 1];
                    // if (token.range] == pos && token.type == TokenType.Punctuator)
                    {
                        if (token.value == "/" || token.value == "/=")
                        {
                            extra.tokens.Remove(extra.tokens.Last());
                        }
                    }
                }

                extra.tokens.Add(new Token()
                {
                    type = TokenType.RegularExpression,
                    value = regex.literal,
                    regex = regex.regex,
                    range = new Token.TokenRange() { start = pos, end = index },
                    loc = loc
                });
            }

            return regex;
        }

        public bool isIdentifierName(Token token)
        {
            return token.type == TokenType.Identifier ||
                   token.type == TokenType.Keyword ||
                   token.type == TokenType.BooleanLiteral ||
                   token.type == TokenType.NullLiteral;
        }

        public Token advanceSlash()
        {
            Token prevToken,
                checkToken;
            // Using the following algorithm:
            // https://github.com/mozilla/sweet.js/wiki/design
            prevToken = extra.tokens[extra.tokens.Count - 1];
            if (prevToken == null)
            {
                // Nothing before that: it cannot be a division.
                return collectRegex();
            }
            if (prevToken.type == TokenType.Punctuator)
            {
                if (prevToken.value == "]")
                {
                    return scanPunctuator();
                }
                if (prevToken.value == ")")
                {
                    checkToken = extra.tokens[extra.openParenToken - 1];
                    if (checkToken != null &&
                        checkToken.type == TokenType.Keyword &&
                        (checkToken.value == "if" ||
                         checkToken.value == "while" ||
                         checkToken.value == "for" ||
                         checkToken.value == "with"))
                    {
                        return collectRegex();
                    }
                    return scanPunctuator();
                }
                if (prevToken.value == "}")
                {
                    // Dividing a function by anything makes little sense,
                    // but we have to check for that.
                    if (extra.tokens.Count > (extra.openCurlyToken - 3) &&
                        extra.tokens[extra.openCurlyToken - 3].type == TokenType.Keyword)
                    {
                        // Anonymous function.
                        checkToken = extra.tokens[extra.openCurlyToken - 4];
                        if (checkToken != null)
                        {
                            return scanPunctuator();
                        }
                    }
                    else if (extra.tokens.Count > (extra.openCurlyToken - 4) &&
                             extra.tokens[extra.openCurlyToken - 4].type == TokenType.Keyword)
                    {
                        // Named function.
                        checkToken = extra.tokens[extra.openCurlyToken - 5];
                        if (checkToken != null)
                        {
                            return collectRegex();
                        }
                    }
                    else
                    {
                        return scanPunctuator();
                    }
                    // checkToken determines whether the function is
                    // a declaration or an expression.
                    if (FnExprTokens.IndexOf(checkToken.value) >= 0)
                    {
                        // It is an expression.
                        return scanPunctuator();
                    }
                    // It is a declaration.
                    return collectRegex();
                }
                return collectRegex();
            }
            if (prevToken.type == TokenType.Keyword && prevToken.value != "this")
            {
                return collectRegex();
            }
            return scanPunctuator();
        }

        public Token advance()
        {
            char cp;
            Token token;

            if (index >= length)
            {
                return new Token()
                {
                    type = TokenType.EOF,
                    lineNumber = lineNumber,
                    lineStart = lineStart,
                    start = index,
                    end = index
                };
            }

            cp = source[index];

            if (isIdentifierStart(cp))
            {
                token = scanIdentifier();
                if (strict && isStrictModeReservedWord(token.value.ToString()))
                {
                    token.type = TokenType.Keyword;
                }
                return token;
            }

            // Very common: ( and ) and ;
            if (cp == 0x28 || cp == 0x29 || cp == 0x3B)
            {
                return scanPunctuator();
            }

            // String literal starts with single quote (U+0027) or double quote (U+0022).
            if (cp == 0x27 || cp == 0x22)
            {
                return scanStringLiteral();
            }

            // Dot (.) U+002E can also start a floating-point number, hence the need
            // to check the next character.
            if (cp == 0x2E)
            {
                if (isDecimalDigit(source.ToCharArray()[index + 1]))
                {
                    return scanNumericLiteral();
                }
                return scanPunctuator();
            }

            if (isDecimalDigit(cp))
            {
                return scanNumericLiteral();
            }

            // Slash (/) U+002F can also start a regex.
            if (extra.tokenize && cp == 0x2F)
            {
                return advanceSlash();
            }

            // Template literals start with ` (U+0060) for template head
            // or } (U+007D) for template middle or template tail.
            if (cp == 0x60 || (cp == 0x7D && state.curlyStack.Last() == "${"))
            {
                // [state.curlyStack.Count - 1] )) {
                return scanTemplate();
            }

            // Possible identifier start in a surrogate pair.
            if (cp >= 0xD800 && cp < 0xDFFF)
            {
                cp = codePointAt(index);
                if (isIdentifierStart(cp))
                {
                    return scanIdentifier();
                }
            }
            return scanPunctuator();
        }

        public Token collectToken()
        {
            Loc loc;
            Token token;
            string value;
            Token entry;

            loc = new Loc()
            {
                start = new Loc.Position()
                {
                    line = lineNumber,
                    column = index - lineStart
                }
            };

            token = advance();
            loc.end = new Loc.Position()
            {
                line = lineNumber,
                column = index - lineStart
            };

            if (token.type != TokenType.EOF)
            {

                value = source.Substring(token.start, token.end - token.start);
                entry = new Token
                {
                    type = token.type,
                    value = value,
                    range = new Token.TokenRange() { start = token.start, end = token.end },

                    loc = loc,
                    lineNumber = lineNumber,
                    start = loc.start.column,
                    end = loc.end.column,

                };
                if (token.regex != null)
                {
                    entry.regex = new Regex()
                    {
                        pattern = token.regex.pattern,
                        flags = token.regex.flags
                    };
                }
                extra.tokens.Add(entry);
            }

            return token;
        }

        public Token lex()
        {
            Token token;
            scanning = true;

            lastIndex = index;
            lastLineNumber = lineNumber;
            lastLineStart = lineStart;

            skipComment();

            token = lookahead;

            startIndex = index;
            startLineNumber = lineNumber;
            startLineStart = lineStart;

            lookahead = extra.tokens.Count >= 0 ? collectToken() : advance();
            scanning = false;
            return token;
        }

        public void peek()
        {
            scanning = true;

            skipComment();

            lastIndex = index;
            lastLineNumber = lineNumber;
            lastLineStart = lineStart;

            startIndex = index;
            startLineNumber = lineNumber;
            startLineStart = lineStart;

            lookahead = extra.tokens.Count >= 0 ? collectToken() : advance();
            scanning = false;
        }

        public Loc.Position Position()
        {
            return new Loc.Position() { line = startLineNumber, column = startIndex - startLineStart };
        }

        public Loc SourceLocation()
        {
            return new Loc() { start = new Loc.Position(), end = null };
        }

        public static Loc WrappingSourceLocation(Token startToken)
        {
            return new Loc()
            {
                start = new Loc.Position()
                {
                    line = startToken.lineNumber,
                    column = startToken.start - startToken.lineStart
                },
                end = null
            };
        }

        //public Token  Node() {
        //    if (extra.range) {
        //        this.range = [startIndex, 0];
        //    }
        //    if (extra.loc) {
        //        this.loc = new SourceLocation();
        //    }
        //}

        //public void WrappingNode( Token startToken) {
        //    if (extra.range) {
        //        this.range = [startToken.start, 0];
        //    }
        //    if (extra.loc) {
        //        this.loc = new WrappingSourceLocation(startToken);
        //    }
        //}


        public void recordError(Error error)
        {
            int e;
            Error existing;

            for (e = 0; e < extra.errors.Count; e++)
            {
                existing = extra.errors[e];
                // Prevent duplicated error.
                /* istanbul ignore next */
                if (existing.index == error.index && existing.message == error.message)
                {
                    return;
                }
            }

            extra.errors.Add(error);
        }

        public Error constructError(string msg, int column)
        {
            var error = new Error(msg) { column = column };
            //try {
            //    throw error;
            //} catch (Exception @base) {
            //    /* istanbul ignore else */
            //    if (Object.create && Object.defineProperty) {
            //        error = Object.create(@base);
            //        //Object.defineProperty(error, "column", { value: column });
            //        error.column = column;
            //    }
            //} finally {

            //}
            return error;
        }

        public Error createError(int line, int pos, string description)
        {
            string msg;
            int column;
            Error error;

            msg = "Line " + line + ": " + description;
            column = pos - (scanning ? lineStart : lastLineStart) + 1;
            error = constructError(msg, column);
            error.lineNumber = line;
            error.description = description;
            error.index = pos;
            return error;
        }

        // Throw an exception
        public void throwError(string messageFormat, params string[] arg)
        {
            throw createError(lastLineNumber, lastIndex, string.Format(messageFormat, arg));
        }

        public void throwError(string messageFormat)
        {
            throw createError(lastLineNumber, lastIndex, messageFormat);
        }

        public void tolerateError(string messageFormat)
        {

            System.Console.WriteLine(messageFormat);
            //var args, msg;
            //Error error;

            //args = Array.prototype.slice.call(arguments, 1);
            ///* istanbul ignore next */
            //msg = messageFormat.replace(/%(\d)/g,
            //    function (whole, idx) {
            //        assert(idx < args.length, "Message reference must be in range");
            //        return args[idx];
            //    }
            //);

            //error = createError(lineNumber, lastIndex, msg);
            //if (extra.errors) {
            //    recordError(error);
            //} else {
            //    throw error;
            //}
        }

        // Throw an exception because of the token.

        public Error unexpectedTokenError(Token token, string message = null)
        {
            string value;
            string msg = message ?? Messages.UnexpectedToken;

            if (token != null)
            {
                if (message != null)
                {
                    msg = (token.type == TokenType.EOF)
                        ? Messages.UnexpectedEOS
                        : (token.type == TokenType.Identifier)
                            ? Messages.UnexpectedIdentifier
                            : (token.type == TokenType.NumericLiteral)
                                ? Messages.UnexpectedNumber
                                : (token.type == TokenType.StringLiteral)
                                    ? Messages.UnexpectedString
                                    : (token.type == TokenType.Template)
                                        ? Messages.UnexpectedTemplate
                                        : Messages.UnexpectedToken;

                    if (token.type == TokenType.Keyword)
                    {
                        if (isFutureReservedWord(token.value))
                        {
                            msg = Messages.UnexpectedReserved;
                        }
                        else if (strict && isStrictModeReservedWord(token.value))
                        {
                            msg = Messages.StrictReservedWord;
                        }
                    }
                }

                // value = (token.type == TokenType.Template) ? token.value.raw : token.value;
                value = token.value;
            }
            else
            {
                value = "ILLEGAL";
            }

            //msg = msg.replace("%0", value);

            return createError(token.lineNumber, token.start, msg); //:
            //  createError(scanning ? lineNumber : lastLineNumber, scanning ? index : lastIndex, msg);
        }

        public void throwUnexpectedToken(Token token = null, string message = "")
        {
            throw unexpectedTokenError(token, message);
        }

        public void tolerateUnexpectedToken(Token token = null, string message = "")
        {
            var error = unexpectedTokenError(token, message);
            if (extra.errors.Any())
            {
                recordError(error);
            }
            else
            {
                throw error;
            }
        }

        // Expect the next token to match the specified punctuator.
        // If not, an exception will be thrown.

        public void expect(object value)
        {
            Token token = lex();
            if (token.type != TokenType.Punctuator || token.value != value.ToString())
            {
                throwUnexpectedToken(token);
            }
        }

        /**
    * @name expectCommaSeparator
    * @description Quietly expect a comma when in tolerant mode, otherwise delegates
    * to <code>expect(value)</code>
    * @since 2.0
    */

        public void expectCommaSeparator()
        {
            Token token;

            if (extra.errors != null)
            {
                token = lookahead;
                if (token.type == TokenType.Punctuator && token.value == ",")
                {
                    lex();
                }
                else if (token.type == TokenType.Punctuator && token.value == ";")
                {
                    lex();
                    tolerateUnexpectedToken(token);
                }
                else
                {
                    tolerateUnexpectedToken(token, Messages.UnexpectedToken);
                }
            }
            else
            {
                expect(",");
            }
        }

        // Expect the next token to match the specified keyword.
        // If not, an exception will be thrown.

        public void expectKeyword(string keyword)
        {
            Token token = lex();
            if (token.type != TokenType.Keyword || token.value != keyword)
            {
                throwUnexpectedToken(token);
            }
        }

        // Return true if the next token matches the specified punctuator.

        public bool match(string value)
        {
            return lookahead.type == TokenType.Punctuator && lookahead.value == value;
        }

        // Return true if the next token matches the specified keyword

        public bool matchKeyword(string keyword)
        {
            return lookahead.type == TokenType.Keyword && lookahead.value == keyword;
        }

        // Return true if the next token matches the specified contextual keyword
        // (where an identifier is sometimes a keyword depending on the context)

        public bool matchContextualKeyword(string keyword)
        {
            return lookahead.type == TokenType.Identifier && lookahead.value == keyword;
        }

        // Return true if the next token is an assignment operator

        public bool matchAssign()
        {
            string op;

            if (lookahead.type != TokenType.Punctuator)
            {
                return false;
            }
            op = lookahead.value;
            return op == "=" ||
                   op == "*=" ||
                   op == "/=" ||
                   op == "%=" ||
                   op == "+=" ||
                   op == "-=" ||
                   op == "<<=" ||
                   op == ">>=" ||
                   op == ">>>=" ||
                   op == "&=" ||
                   op == "^=" ||
                   op == "|=";
        }

        public void consumeSemicolon()
        {
            // Catch the very common case first: immediately a semicolon (U+003B).
            if (source[startIndex] == 0x3B || match(";"))
            {
                lex();
                return;
            }

            if (hasLineTerminator)
            {
                return;
            }

            // FIXME(ikarienator): this is seemingly an issue in the previous location info convention.
            lastIndex = startIndex;
            lastLineNumber = startLineNumber;
            lastLineStart = startLineStart;

            if (lookahead.type != TokenType.EOF && !match("}"))
            {
                throwUnexpectedToken(lookahead);
            }
        }

        // Cover grammar support.
        //
        // When an assignment expression position starts with an left parenthesis, the determination of the type
        // of the syntax is to be deferred arbitrarily long until the end of the parentheses pair (plus a lookahead)
        // or the first comma. This situation also defers the determination of all the expressions nested in the pair.
        //
        // There are three productions that can be parsed in a parentheses pair that needs to be determined
        // after the outermost pair is closed. They are:
        //
        //  1. AssignmentExpression
        //  2. BindingElements
        //  3. AssignmentTargets
        //
        // In order to avoid exponential backtracking, we use two flags to denote if the production can be
        // binding element or assignment target.
        //
        // The three productions have the relationship:
        //
        //  BindingElements ? AssignmentTargets ? AssignmentExpression
        //
        // with a single exception that CoverInitializedName when used directly in an Expression, generates
        // an early error. Therefore, we need the third state, firstCoverInitializedNameError, to track the
        // first usage of CoverInitializedName and report it when we reached the end of the parentheses pair.
        //
        // isolateCoverGrammar function runs the given parser function with a new cover grammar context, and it does not
        // effect the current flags. This means the production the parser parses is only used as an expression. Therefore
        // the CoverInitializedName check is conducted.
        //
        // inheritCoverGrammar function runs the given parse function with a new cover grammar context, and it propagates
        // the flags outside of the parser. This means the production the parser parses is used as a part of a potential
        // pattern. The CoverInitializedName check is deferred.
        public Node isolateCoverGrammar(Func<Node> parser)
        {
            bool oldIsBindingElement = isBindingElement;
            bool oldIsAssignmentTarget = isAssignmentTarget;
            Token oldFirstCoverInitializedNameError = firstCoverInitializedNameError;
            Node result;
            isBindingElement = true;
            isAssignmentTarget = true;
            firstCoverInitializedNameError = null;
            result = parser();
            if (firstCoverInitializedNameError != null)
            {
                throwUnexpectedToken(firstCoverInitializedNameError);
            }
            isBindingElement = oldIsBindingElement;
            isAssignmentTarget = oldIsAssignmentTarget;
            firstCoverInitializedNameError = oldFirstCoverInitializedNameError;
            return result;
        }

        public Node inheritCoverGrammar(Func<Node> parser)
        {
            bool oldIsBindingElement = isBindingElement;
            bool oldIsAssignmentTarget = isAssignmentTarget;
            Token oldFirstCoverInitializedNameError = firstCoverInitializedNameError;
            Node result;
            isBindingElement = true;
            isAssignmentTarget = true;
            firstCoverInitializedNameError = null;
            result = parser();
            isBindingElement = isBindingElement && oldIsBindingElement;
            isAssignmentTarget = isAssignmentTarget && oldIsAssignmentTarget;
            firstCoverInitializedNameError = oldFirstCoverInitializedNameError ?? firstCoverInitializedNameError;
            return result;
        }

        // ECMA-262 13.3.3 Destructuring Binding Patterns

        public Node parseArrayPattern(List<Token> @params, string kind)
        {
            var node = new Node();
            List<Node> elements = new List<Node>();
            Node rest;
            Node restNode;
            expect("[");

            while (!match("]"))
            {
                if (match(","))
                {
                    lex();
                    elements.Add(null);
                }
                else
                {
                    if (match("..."))
                    {
                        restNode = new Node();
                        lex();
                        @params.Add(lookahead);
                        rest = parseVariableIdentifier(kind);
                        //rest = parseVariableIdentifier(@params, kind);
                        elements.Add(restNode.finishRestElement(rest));
                        break;
                    }
                    else
                    {
                        elements.Add(parsePatternWithDefault(@params, kind));
                    }
                    if (!match("]"))
                    {
                        expect(",");
                    }
                }

            }

            expect("]");

            return node.finishArrayPattern(elements);
        }

        public Node parsePropertyPattern(List<Token> @params, string kind)
        {
            var node = new Node();
            Node key;
            Token keyToken;
            bool computed = match("[");
            Node init;
            if (lookahead.type == TokenType.Identifier)
            {
                keyToken = lookahead;
                key = parseVariableIdentifier();
                if (match("="))
                {
                    @params.Add(keyToken);
                    lex();
                    init = parseAssignmentExpression();

                    return node.finishProperty("init", key, false,
                        new Node(keyToken).finishAssignmentPattern(key, init), false, false);
                }
                else if (!match(":"))
                {
                    @params.Add(keyToken);
                    return node.finishProperty("init", key, false, key, false, true);
                }
            }
            else
            {
                key = new Node();// parseObjectPropertyKey(@params, kind);
            }
            expect(":");
            init = parsePatternWithDefault(@params, kind);
            return node.finishProperty("init", key, computed, init, false, false);
        }

        public Node parseObjectPattern(List<Token> @params, string kind)
        {
            Node node = new Node();
            List<Node> properties = new List<Node>();

            expect("{");

            while (!match("}"))
            {
                properties.Add(parsePropertyPattern(@params, kind));
                if (!match("}"))
                {
                    expect(",");
                }
            }

            lex();

            return node.finishObjectPattern(properties);
        }

        public Node parsePattern(List<Token> @params, string kind)
        {
            if (match("["))
            {
                return parseArrayPattern(@params, kind);
            }
            else if (match("{"))
            {
                return parseObjectPattern(@params, kind);
            }
            @params.Add(lookahead);
            return parseVariableIdentifier(kind);
        }

        public Node parsePatternWithDefault(List<Token> @params, string kind)
        {
            var startToken = lookahead;
            Node pattern;
            bool previousAllowYield;
            Node right;
            pattern = parsePattern(@params, kind);
            if (match("="))
            {
                lex();
                previousAllowYield = state.allowYield;
                state.allowYield = true;
                right = isolateCoverGrammar(parseAssignmentExpression);
                state.allowYield = previousAllowYield;
                pattern = new Node(startToken).finishAssignmentPattern(pattern, right);
            }
            return pattern;
        }

        // ECMA-262 12.2.5 Array Initializer

        public Node parseArrayInitializer()
        {
            List<Node> elements = new List<Node>();
            Node node = new Node();
            Node restSpread;

            expect("[");

            while (!match("]"))
            {
                if (match(","))
                {
                    lex();
                    elements.Add(null);
                }
                else if (match("..."))
                {
                    restSpread = new Node();
                    lex();
                    restSpread.finishSpreadElement(inheritCoverGrammar(parseAssignmentExpression));

                    if (!match("]"))
                    {
                        isAssignmentTarget = isBindingElement = false;
                        expect(",");
                    }
                    elements.Add(restSpread);
                }
                else
                {
                    elements.Add(inheritCoverGrammar(parseAssignmentExpression));

                    if (!match("]"))
                    {
                        expect(",");
                    }
                }
            }

            lex();

            return node.finishArrayExpression(elements);
        }

        // ECMA-262 12.2.6 Object Initializer

        public Node parsePropertyFunction(Node node, Options paramInfo, bool isGenerator)
        {
            bool previousStrict;
            List<Node> body;

            isAssignmentTarget = isBindingElement = false;

            previousStrict = strict;
            body = new List<Node>() { isolateCoverGrammar(parseFunctionSourceElements) };

            if (strict && paramInfo.firstRestricted != null)
            {
                tolerateUnexpectedToken(paramInfo.firstRestricted, paramInfo.message);
            }
            if (strict && paramInfo.stricted != null)
            {
                tolerateUnexpectedToken(paramInfo.stricted, paramInfo.message);
            }

            strict = previousStrict;
            return node.finishFunctionExpression(null, paramInfo.@params, paramInfo.defaults, body, isGenerator);
        }

        public Node parsePropertyMethodFunction()
        {
            Options @params;
            Node method;
            Node node = new Node();
            bool previousAllowYield = state.allowYield;

            state.allowYield = false;
            @params = parseParams();
            state.allowYield = previousAllowYield;

            state.allowYield = false;
            method = parsePropertyFunction(node, @params, false);
            state.allowYield = previousAllowYield;

            return method;
        }

        public Node parseObjectPropertyKey()
        {
            Token token;
            Node node = new Node();
            Node expr;

            token = lex();

            // Note: This function is called only from parseObjectProperty(), where
            // EOF and Punctuator tokens are already filtered out.

            switch (token.type)
            {
                case TokenType.StringLiteral:
                case TokenType.NumericLiteral:
                    if (strict && token.octal)
                    {
                        tolerateUnexpectedToken(token, Messages.StrictOctalLiteral);
                    }
                    return node.finishLiteral(token);
                case TokenType.Identifier:
                case TokenType.BooleanLiteral:
                case TokenType.NullLiteral:
                case TokenType.Keyword:
                    return node.finishIdentifier(token.value);
                case TokenType.Punctuator:
                    if (token.value == "[")
                    {
                        expr = isolateCoverGrammar(parseAssignmentExpression);
                        expect("]");
                        return expr;
                    }
                    break;
            }
            throwUnexpectedToken(token);
            return null;
        }

        public bool lookaheadPropertyName()
        {
            switch (lookahead.type)
            {
                case TokenType.Identifier:
                case TokenType.StringLiteral:
                case TokenType.BooleanLiteral:
                case TokenType.NullLiteral:
                case TokenType.NumericLiteral:
                case TokenType.Keyword:
                    return true;
                case TokenType.Punctuator:
                    return lookahead.value == "[";
            }
            return false;
        }

        // This function is to try to parse a MethodDefinition as defined in 14.3. But in the case of object literals,
        // it might be called at a position where there is in fact a short hand identifier pattern or a data property.
        // This can only be determined after we consumed up to the left parentheses.
        //
        // In order to avoid back tracking, it returns `null` if the position is not a MethodDefinition and the caller
        // is responsible to visit other options.
        public Node tryParseMethodDefinition(Token token, Node key, bool computed, Node node)
        {
            Node value;
            Options options;
            Node methodNode;
            Options @params;
            bool previousAllowYield = state.allowYield;

            if (token.type == TokenType.Identifier)
            {
                // check for `get` and `set`;

                if (token.value == "get" && lookaheadPropertyName())
                {
                    computed = match("[");
                    key = parseObjectPropertyKey();
                    methodNode = new Node();
                    expect("(");
                    expect(")");

                    state.allowYield = false;
                    value = parsePropertyFunction(methodNode, new Options()
                    {
                        @params = new List<Node>(),
                        defaults = new List<Node>(),
                        stricted = null,
                        firstRestricted = null,
                        message = null
                    }, false);
                    state.allowYield = previousAllowYield;

                    return node.finishProperty("get", key, computed, value, false, false);
                }
                else if (token.value == "set" && lookaheadPropertyName())
                {
                    computed = match("[");
                    key = parseObjectPropertyKey();
                    methodNode = new Node();
                    expect("(");

                    options = new Options()
                    {
                        @params = new List<Node>(),
                        defaultCount = 0,
                        defaults = new List<Node>(),
                        firstRestricted = null,
                        // paramSet = {}
                    };
                    if (match(")"))
                    {
                        tolerateUnexpectedToken(lookahead);
                    }
                    else
                    {
                        state.allowYield = false;
                        parseParam(options);
                        state.allowYield = previousAllowYield;
                        if (options.defaultCount == 0)
                        {
                            options.defaults = new List<Node>();
                        }
                    }
                    expect(")");

                    state.allowYield = false;
                    value = parsePropertyFunction(methodNode, options, false);
                    state.allowYield = previousAllowYield;

                    return node.finishProperty("set", key, computed, value, false, false);
                }
            }
            else if (token.type == TokenType.Punctuator && token.value == "*" && lookaheadPropertyName())
            {
                computed = match("[");
                key = parseObjectPropertyKey();
                methodNode = new Node();

                state.allowYield = true;
                @params = parseParams();
                state.allowYield = previousAllowYield;

                state.allowYield = false;
                value = parsePropertyFunction(methodNode, @params, true);
                state.allowYield = previousAllowYield;

                return node.finishProperty("init", key, computed, value, true, false);
            }

            if (key != null && match("("))
            {
                value = parsePropertyMethodFunction();
                return node.finishProperty("init", key, computed, value, true, false);
            }

            // Not a MethodDefinition.
            return null;
        }

        public Node parseObjectProperty( bool hasProto)
        {
            var token = lookahead;
            Node node = new Node();
            bool computed;
            Node key = null;
            Node maybeMethod;
            bool proto;
            Node value;

            computed = match("[");
            if (match("*")) {
                lex();
            } else {
                key = parseObjectPropertyKey();
            }
            maybeMethod = tryParseMethodDefinition(token, key, computed, node);
            if (maybeMethod!= null) {
                return maybeMethod;
            }

            if (key== null) {
                throwUnexpectedToken(lookahead);
            }

            // Check for duplicated __proto__
            if (!computed) {
                proto = (key.type == Syntax.Identifier && key.name == "__proto__") ||
                    (key.type == Syntax.Literal && key.name== "__proto__");
                if (hasProto && proto) {
                    tolerateError(Messages.DuplicateProtoProperty);
                }
                hasProto |= proto;
            }

            if (match(":")) {
                lex();
                value = inheritCoverGrammar(parseAssignmentExpression);
                return node.finishProperty("init", key, computed, value, false, false);
            }

            if (token.type == TokenType.Identifier) {
                if (match("=")) {
                    firstCoverInitializedNameError = lookahead;
                    lex();
                    value = isolateCoverGrammar(parseAssignmentExpression);
                    return node.finishProperty("init", key, computed,
                        new Node(token).finishAssignmentPattern(key, value), false, true);
                }
                return node.finishProperty("init", key, computed, key, false, true);
            }

            throwUnexpectedToken(lookahead);
            return null;
        }

        public Node parseObjectInitializer()
        {
            List<Node> properties = new List<Node>();
            // hasProto = { value: false };
            Node node = new Node();

            expect("{");

            while (!match("}"))
            {
                properties.Add(parseObjectProperty(false));

                if (!match("}"))
                {
                    expectCommaSeparator();
                }
            }

            expect("}");

            return node.finishObjectExpression(properties);
        }

        public void reinterpretExpressionAsPattern(Node expr)
        {
            int i;
            switch (expr.type)
            {
                case Syntax.Identifier:
                case Syntax.MemberExpression:
                case Syntax.RestElement:
                case Syntax.AssignmentPattern:
                    break;
                case Syntax.SpreadElement:
                    expr.type = Syntax.RestElement;
                    reinterpretExpressionAsPattern(expr.argument);
                    break;
                case Syntax.ArrayExpression:
                    expr.type = Syntax.ArrayPattern;
                    for (i = 0; i < expr.elements.Count; i++)
                    {
                        if (expr.elements[i] != null)
                        {
                            reinterpretExpressionAsPattern(expr.elements[i]);
                        }
                    }
                    break;
                case Syntax.ObjectExpression:
                    expr.type = Syntax.ObjectPattern;
                    for (i = 0; i < expr.properties.Count; i++)
                    {
                        reinterpretExpressionAsPattern(expr.properties[i].value);
                    }
                    break;
                case Syntax.AssignmentExpression:
                    expr.type = Syntax.AssignmentPattern;
                    reinterpretExpressionAsPattern(expr.left);
                    break;
                default:
                    // Allow other node type for tolerant parsing.
                    break;
            }
        }

        // ECMA-262 12.2.9 Template Literals

        public Node parseTemplateElement(Options option)
        {
            Node node;
            Token token;

            if (lookahead.type != TokenType.Template || (option.head && !lookahead.head))
            {
                throwUnexpectedToken();
            }

            node = new Node();
            token = lex();

            return node.finishTemplateElement(new Node() { raw = token.value }, token.tail);
            //.raw, cooked= token.value.cooked }
        }

        public Node parseTemplateLiteral()
        {
            Node quasi;
            List<Node> quasis;
            List<Node> expressions;
            Node node = new Node();

            quasi = parseTemplateElement(new Options() { head = true });
            quasis = new List<Node>() { quasi };
            expressions = new List<Node>();
            ;

            while (quasi.tail != null)
            {
                expressions.Add(parseExpression());
                quasi = parseTemplateElement(new Options() { head = false });
                quasis.Add(quasi);
            }

            return node.finishTemplateLiteral(quasis, expressions);
        }

        // ECMA-262 12.2.10 The Grouping Operator

        public Node parseGroupExpression()
        {
            Node expr;
            List<Node> expressions;
            Token startToken;
            int i;
            List<Token> @params = new List<Token>();

            expect("(");

            if (match(")"))
            {
                lex();
                if (!match("=>"))
                {
                    expect("=>");
                }
                return new Node()
                {
                    type = PlaceHolders.ArrowParameterPlaceHolder,
                    @params = new List<Node>(),
                    //rawParams= []
                };
            }

            startToken = lookahead;
            if (match("..."))
            {
                expr = parseRestElement(@params);
                expect(")");
                if (!match("=>"))
                {
                    expect("=>");
                }
                return new Node()
                {
                    type = PlaceHolders.ArrowParameterPlaceHolder,
                    @params = new List<Node>() { expr }
                };
            }

            isBindingElement = true;
            expr = inheritCoverGrammar(parseAssignmentExpression);

            if (match(","))
            {
                isAssignmentTarget = false;
                expressions = new List<Node>() { expr };

                while (startIndex < length)
                {
                    if (!match(","))
                    {
                        break;
                    }
                    lex();

                    if (match("..."))
                    {
                        if (!isBindingElement)
                        {
                            throwUnexpectedToken(lookahead);
                        }
                        expressions.Add(parseRestElement(@params));
                        expect(")");
                        if (!match("=>"))
                        {
                            expect("=>");
                        }
                        isBindingElement = false;
                        for (i = 0; i < expressions.Count; i++)
                        {
                            reinterpretExpressionAsPattern(expressions[i]);
                        }
                        return new Node()
                        {
                            type = PlaceHolders.ArrowParameterPlaceHolder,
                            @params = expressions
                        };
                    }

                    expressions.Add(inheritCoverGrammar(parseAssignmentExpression));
                }

                expr = new Node(startToken).finishSequenceExpression(expressions);
            }


            expect(")");

            if (match("=>"))
            {
                if (expr.type == Syntax.Identifier && expr.name == "yield")
                {
                    return new Node()
                    {
                        type = PlaceHolders.ArrowParameterPlaceHolder,
                        @params = new List<Node>() { expr }
                    };
                }

                if (!isBindingElement)
                {
                    throwUnexpectedToken(lookahead);
                }

                if (expr.type == Syntax.SequenceExpression)
                {
                    for (i = 0; i < expr.expressions.Count; i++)
                    {
                        reinterpretExpressionAsPattern(expr.expressions[i]);
                    }
                }
                else
                {
                    reinterpretExpressionAsPattern(expr);
                }

                expr = new Node()
                {
                    type = PlaceHolders.ArrowParameterPlaceHolder,
                    @params = expr.type == Syntax.SequenceExpression ? expr.expressions : new List<Node>() { expr }
                };
            }
            isBindingElement = false;
            return expr;
        }


        // ECMA-262 12.2 Primary Expressions

        public Node parsePrimaryExpression()
        {
            TokenType type;
            Token token;
            Node expr = null;
            Node node;

            if (match("("))
            {
                isBindingElement = false;
                return inheritCoverGrammar(parseGroupExpression);
            }

            if (match("["))
            {
                return inheritCoverGrammar(parseArrayInitializer);
            }

            if (match("{"))
            {
                return inheritCoverGrammar(parseObjectInitializer);
            }

            type = lookahead.type;
            node = new Node();

            if (type == TokenType.Identifier)
            {
                if (state.sourceType == "module" && lookahead.value == "await")
                {
                    tolerateUnexpectedToken(lookahead);
                }
                expr = node.finishIdentifier(lex().value);
            }
            else if (type == TokenType.StringLiteral || type == TokenType.NumericLiteral)
            {
                isAssignmentTarget = isBindingElement = false;
                if (strict && lookahead.octal)
                {
                    tolerateUnexpectedToken(lookahead, Messages.StrictOctalLiteral);
                }
                expr = node.finishLiteral(lex());
            }
            else if (type == TokenType.Keyword)
            {
                if (!strict && state.allowYield && matchKeyword("yield"))
                {
                    return parseNonComputedProperty();
                }
                isAssignmentTarget = isBindingElement = false;
                if (matchKeyword("function"))
                {
                    return parseFunctionExpression();
                }
                if (matchKeyword("this"))
                {
                    lex();
                    return node.finishThisExpression();
                }
                if (matchKeyword("class"))
                {
                    return parseClassExpression();
                }
                throwUnexpectedToken(lex());
            }
            else if (type == TokenType.BooleanLiteral)
            {
                isAssignmentTarget = isBindingElement = false;
                token = lex();
                token.value = (token.value == "true").ToString();
                expr = node.finishLiteral(token);
            }
            else if (type == TokenType.NullLiteral)
            {
                isAssignmentTarget = isBindingElement = false;
                token = lex();
                token.value = null;
                expr = node.finishLiteral(token);
            }
            else if (match("/") || match("/="))
            {
                isAssignmentTarget = isBindingElement = false;
                index = startIndex;

                if (extra.tokens != null)
                {
                    token = collectRegex();
                }
                else
                {
                    token = scanRegExp();
                }
                lex();
                expr = node.finishLiteral(token);
            }
            else if (type == TokenType.Template)
            {
                expr = parseTemplateLiteral();
            }
            else
            {
                throwUnexpectedToken(lex());
            }

            return expr;
        }

        // ECMA-262 12.3 Left-Hand-Side Expressions

        public List<Node> parseArguments()
        {
            List<Node> args = new List<Node>();
            Node expr;

            expect("(");

            if (!match(")"))
            {
                while (startIndex < length)
                {
                    if (match("..."))
                    {
                        expr = new Node();
                        lex();
                        expr.finishSpreadElement(isolateCoverGrammar(parseAssignmentExpression));
                    }
                    else
                    {
                        expr = isolateCoverGrammar(parseAssignmentExpression);
                    }
                    args.Add(expr);
                    if (match(")"))
                    {
                        break;
                    }
                    expectCommaSeparator();
                }
            }

            expect(")");

            return args;
        }

        public Node parseNonComputedProperty()
        {
            Token token;
            Node node = new Node();

            token = lex();

            if (!isIdentifierName(token))
            {
                throwUnexpectedToken(token);
            }

            return node.finishIdentifier(token.value);
        }

        public Node parseNonComputedMember()
        {
            expect(".");
            return parseNonComputedProperty();
        }

        public Node parseComputedMember()
        {

            Node expr;

            expect("[");

            expr = isolateCoverGrammar(parseExpression);

            expect("]");

            return expr;
        }

        // ECMA-262 12.3.3 The new Operator

        public Node parseNewExpression()
        {
            Node callee;
            List<Node> args;
            Node node = new Node();

            expectKeyword("new");

            if (match("."))
            {
                lex();
                if (lookahead.type == TokenType.Identifier && lookahead.value == "target")
                {
                    if (state.inFunctionBody)
                    {
                        lex();
                        return node.finishMetaProperty("new", "target");
                    }
                }
                throwUnexpectedToken(lookahead);
            }

            callee = isolateCoverGrammar(parseLeftHandSideExpression);
            args = match("(") ? parseArguments() : new List<Node>();

            isAssignmentTarget = isBindingElement = false;

            return node.finishNewExpression(callee, args);
        }

        // ECMA-262 12.3.4 Function Calls

        public Node parseLeftHandSideExpressionAllowCall()
        {
            Node quasi;
            Node expr;
            List<Node> args;
            Node property;
            Token startToken;
            bool previousAllowIn = state.allowIn;

            startToken = lookahead;
            state.allowIn = true;

            if (matchKeyword("super") && state.inFunctionBody)
            {
                expr = new Node();
                lex();
                expr = expr.finishSuper();
                if (!match("(") && !match(".") && !match("["))
                {
                    throwUnexpectedToken(lookahead);
                }
            }
            else
            {
                expr = inheritCoverGrammar(() =>
                {
                    if (matchKeyword("new"))
                        return parseNewExpression();
                    return parsePrimaryExpression();
                });
            }

            for (; ; )
            {
                if (match("."))
                {
                    isBindingElement = false;
                    isAssignmentTarget = true;
                    property = parseNonComputedMember();
                    expr = new Node(startToken).finishMemberExpression(".", expr, property);
                }
                else if (match("("))
                {
                    isBindingElement = false;
                    isAssignmentTarget = false;
                    args = parseArguments();
                    expr = new Node(startToken).finishCallExpression(expr, args);
                }
                else if (match("["))
                {
                    isBindingElement = false;
                    isAssignmentTarget = true;
                    property = parseComputedMember();
                    expr = new Node(startToken).finishMemberExpression("[", expr, property);
                }
                else if (lookahead.type == TokenType.Template && lookahead.head)
                {
                    quasi = parseTemplateLiteral();
                    expr = new Node(startToken).finishTaggedTemplateExpression(expr, quasi);
                }
                else
                {
                    break;
                }
            }
            state.allowIn = previousAllowIn;

            return expr;
        }

        // ECMA-262 12.3 Left-Hand-Side Expressions

        public Node parseLeftHandSideExpression()
        {
            Node quasi;
            Node expr;
            Node property;
            Token startToken;
            assert(state.allowIn, "callee of new expression always allow in keyword.");

            startToken = lookahead;

            if (matchKeyword("super") && state.inFunctionBody)
            {
                expr = new Node();
                lex();
                expr = expr.finishSuper();
                if (!match("[") && !match("."))
                {
                    throwUnexpectedToken(lookahead);
                }
            }
            else
            {
                expr = inheritCoverGrammar(() =>
                {
                    if (matchKeyword("new")) return parseNewExpression();
                    return parsePrimaryExpression();
                });
            }

            for (; ; )
            {
                if (match("["))
                {
                    isBindingElement = false;
                    isAssignmentTarget = true;
                    property = parseComputedMember();
                    expr = new Node(startToken).finishMemberExpression("[", expr, property);
                }
                else if (match("."))
                {
                    isBindingElement = false;
                    isAssignmentTarget = true;
                    property = parseNonComputedMember();
                    expr = new Node(startToken).finishMemberExpression(".", expr, property);
                }
                else if (lookahead.type == TokenType.Template && lookahead.head)
                {
                    quasi = parseTemplateLiteral();
                    expr = new Node(startToken).finishTaggedTemplateExpression(expr, quasi);
                }
                else
                {
                    break;
                }
            }
            return expr;
        }

        // ECMA-262 12.4 Postfix Expressions

        public Node parsePostfixExpression()
        {
            Node expr;
            Token token;
            Token startToken = lookahead;

            expr = inheritCoverGrammar(parseLeftHandSideExpressionAllowCall);

            if (!hasLineTerminator && lookahead.type == TokenType.Punctuator)
            {
                if (match("++") || match("--"))
                {
                    // ECMA-262 11.3.1, 11.3.2
                    if (strict && expr.type == Syntax.Identifier && isRestrictedWord(expr.name))
                    {
                        tolerateError(Messages.StrictLHSPostfix);
                    }

                    if (!isAssignmentTarget)
                    {
                        tolerateError(Messages.InvalidLHSInAssignment);
                    }

                    isAssignmentTarget = isBindingElement = false;

                    token = lex();
                    expr = new Node(startToken).finishPostfixExpression(token.value, expr);
                }
            }

            return expr;
        }

        // ECMA-262 12.5 Unary Operators

        public Node parseUnaryExpression()
        {
            Token token;
            Node expr;
            Token startToken;

            if (lookahead.type != TokenType.Punctuator && lookahead.type != TokenType.Keyword)
            {
                expr = parsePostfixExpression();
            }
            else if (match("++") || match("--"))
            {
                startToken = lookahead;
                token = lex();
                expr = inheritCoverGrammar(parseUnaryExpression);
                // ECMA-262 11.4.4, 11.4.5
                if (strict && expr.type == Syntax.Identifier && isRestrictedWord(expr.name))
                {
                    tolerateError(Messages.StrictLHSPrefix);
                }

                if (!isAssignmentTarget)
                {
                    tolerateError(Messages.InvalidLHSInAssignment);
                }
                expr = new Node(startToken).finishUnaryExpression(token.value, expr);
                isAssignmentTarget = isBindingElement = false;
            }
            else if (match("+") || match("-") || match("~") || match("!"))
            {
                startToken = lookahead;
                token = lex();
                expr = inheritCoverGrammar(parseUnaryExpression);
                expr = new Node(startToken).finishUnaryExpression(token.value, expr);
                isAssignmentTarget = isBindingElement = false;
            }
            else if (matchKeyword("delete") || matchKeyword("void") || matchKeyword("typeof"))
            {
                startToken = lookahead;
                token = lex();
                expr = inheritCoverGrammar(parseUnaryExpression);
                expr = new Node(startToken).finishUnaryExpression(token.value, expr);
                if (strict && expr.@operator == "delete" && expr.argument.type == Syntax.Identifier)
                {
                    tolerateError(Messages.StrictDelete);
                }
                isAssignmentTarget = isBindingElement = false;
            }
            else
            {
                expr = parsePostfixExpression();
            }

            return expr;
        }

        public int binaryPrecedence(Token token, bool allowIn)
        {
            var prec = 0;

            if (token.type != TokenType.Punctuator && token.type != TokenType.Keyword)
            {
                return 0;
            }

            switch (token.value.ToString())
            {
                case "||":
                    prec = 1;
                    break;

                case "&&":
                    prec = 2;
                    break;

                case "|":
                    prec = 3;
                    break;

                case "^":
                    prec = 4;
                    break;

                case "&":
                    prec = 5;
                    break;

                case "==":
                case "!=":
                //case "==":
                case "!==":
                    prec = 6;
                    break;

                case "<":
                case ">":
                case "<=":
                case ">=":
                case "instanceof":
                    prec = 7;
                    break;

                case "in":
                    prec = allowIn ? 7 : 0;
                    break;

                case "<<":
                case ">>":
                case ">>>":
                    prec = 8;
                    break;

                case "+":
                case "-":
                    prec = 9;
                    break;

                case "*":
                case "/":
                case "%":
                    prec = 11;
                    break;

                default:
                    break;
            }

            return prec;
        }

        // ECMA-262 12.6 Multiplicative Operators
        // ECMA-262 12.7 Additive Operators
        // ECMA-262 12.8 Bitwise Shift Operators
        // ECMA-262 12.9 Relational Operators
        // ECMA-262 12.10 Equality Operators
        // ECMA-262 12.11 Binary Bitwise Operators
        // ECMA-262 12.12 Binary Logical Operators

        public Node parseBinaryExpression()
        {
            Token marker;
            List<Token> markers;
            Node expr = null;
            Token token;
            int prec;
            List<Token> stack;
            Node right;
            string @operator;
            Node left;
            int i;

            marker = lookahead;
            left = inheritCoverGrammar(parseUnaryExpression);

            token = lookahead;
            prec = binaryPrecedence(token, state.allowIn);
            if (prec == 0)
            {
                return left;
            }
            isAssignmentTarget = isBindingElement = false;
            token.prec = prec;
            lex();

            markers = new List<Token>() { marker, lookahead };

            right = isolateCoverGrammar(parseUnaryExpression);

            //stack = new List<Token>() {left, token, right};

            //while ((prec = binaryPrecedence(lookahead, state.allowIn)) > 0)
            //{

            //    // Reduce: make a binary expression from the three topmost entries.
            //    while ((stack.Count > 2) && (prec <= stack[stack.Count - 2].prec))
            //    {
            //        right = stack.pop();
            //        @operator = stack.pop().value;
            //        left = stack.pop();
            //        markers.pop();
            //        expr = new Node(markers[markers.Count - 1]).finishBinaryExpression(@operator, left, right);
            //        stack.Add(expr);
            //    }

            //    // Shift.
            //    token = lex();
            //    token.prec = prec;
            //    stack.Add(token);
            //    markers.Add(lookahead);
            //    expr = isolateCoverGrammar(parseUnaryExpression);
            //    stack.Add(expr);
            //}

            //// Final reduce to clean-up the stack.
            //i = stack.Count - 1;
            //expr = stack[i];
            //markers.pop();
            //while (i > 1)
            //{
            //    expr = new Node(markers.pop()).finishBinaryExpression(stack[i - 1].value, stack[i - 2], expr);
            //    i -= 2;
            //}

            return expr;
        }


        // ECMA-262 12.13 Conditional Operator

        public Node parseConditionalExpression()
        {
            Node expr;
            bool previousAllowIn;
            Node consequent;
            Node alternate;
            Token startToken;

            startToken = lookahead;

            expr = inheritCoverGrammar(parseBinaryExpression);
            if (match("?"))
            {
                lex();
                previousAllowIn = state.allowIn;
                state.allowIn = true;
                consequent = isolateCoverGrammar(parseAssignmentExpression);
                state.allowIn = previousAllowIn;
                expect(":");
                alternate = isolateCoverGrammar(parseAssignmentExpression);

                expr = new Node(startToken).finishConditionalExpression(expr, consequent, alternate);
                isAssignmentTarget = isBindingElement = false;
            }

            return expr;
        }

        // ECMA-262 14.2 Arrow Function Definitions

        public Node parseConciseBody()
        {
            if (match("{"))
            {
                return parseFunctionSourceElements();
            }
            return isolateCoverGrammar(parseAssignmentExpression);
        }

        public void checkPatternParam(Options options, Node @param)
        {
            int i;
            switch (@param.type)
            {
                case Syntax.Identifier:
                    validateParam(options, @param, @param.name);
                    break;
                case Syntax.RestElement:
                    checkPatternParam(options, @param.argument);
                    break;
                case Syntax.AssignmentPattern:
                    checkPatternParam(options, @param.left);
                    break;
                case Syntax.ArrayPattern:
                    for (i = 0; i < @param.elements.Count; i++)
                    {
                        if (@param.elements[i] != null)
                        {
                            checkPatternParam(options, param.elements[i]);
                        }
                    }
                    break;
                case Syntax.YieldExpression:
                    break;
                default:
                    assert(param.type == Syntax.ObjectPattern, "Invalid type");
                    for (i = 0; i < param.properties.Count; i++)
                    {
                        checkPatternParam(options, param.properties[i].value);
                    }
                    break;
            }
        }

        public Options reinterpretAsCoverFormalsList(Node expr)
        {
            int i, len;
            Node param;
            List<Node> @params;
            List<Node> defaults;
            int defaultCount;
            Options options;

            Token token;

            defaults = new List<Node>();
            defaultCount = 0;
            @params = new List<Node>() { expr };

            switch (expr.type)
            {
                case Syntax.Identifier:
                    break;
                case PlaceHolders.ArrowParameterPlaceHolder:
                    @params = expr.@params;
                    break;
                default:
                    return null;
            }

            options = new Options()
            {
                // paramSet= {}
            };

            for (i = 0, len = @params.Count; i < len; i += 1)
            {
                param = @params[i];
                switch (param.type)
                {
                    case Syntax.AssignmentPattern:
                        @params[i] = param.left;
                        if (param.right.type == Syntax.YieldExpression)
                        {
                            if (param.right.argument != null)
                            {
                                throwUnexpectedToken(lookahead);
                            }
                            param.right.type = Syntax.Identifier;
                            param.right.name = "yield";
                            param.right.argument = null;
                            param.right.@delegate = null;
                        }
                        defaults.Add(param.right);
                        ++defaultCount;
                        checkPatternParam(options, param.left);
                        break;
                    default:
                        checkPatternParam(options, param);
                        @params[i] = param;
                        defaults.Add(null);
                        break;
                }
            }

            if (strict || !state.allowYield)
            {
                for (i = 0, len = @params.Count; i < len; i += 1)
                {
                    param = @params[i];
                    if (param.type == Syntax.YieldExpression)
                    {
                        throwUnexpectedToken(lookahead);
                    }
                }
            }

            if (options.message == Messages.StrictParamDupe)
            {
                token = strict ? options.stricted : options.firstRestricted;
                throwUnexpectedToken(token, options.message);
            }

            if (defaultCount == 0)
            {
                defaults = new List<Node>();
            }

            return new Options()
            {
                @params = @params,
                defaults = defaults,
                stricted = options.stricted,
                firstRestricted = options.firstRestricted,
                message = options.message
            };
        }

        public Node parseArrowFunctionExpression(Options options, Node node)
        {
            bool previousStrict;
            bool previousAllowYield;
            List<Node> body;

            if (hasLineTerminator)
            {
                tolerateUnexpectedToken(lookahead);
            }
            expect("=>");

            previousStrict = strict;
            previousAllowYield = state.allowYield;
            state.allowYield = true;

            body = new List<Node>() { parseConciseBody() };

            if (strict && options.firstRestricted != null)
            {
                throwUnexpectedToken(options.firstRestricted, options.message);
            }
            if (strict && options.stricted != null)
            {
                tolerateUnexpectedToken(options.stricted, options.message);
            }

            strict = previousStrict;
            state.allowYield = previousAllowYield;

            return node.finishArrowFunctionExpression(options.@params, options.defaults, body, null);
            //body.type != Syntax.BlockStatement);
        }

        // ECMA-262 14.4 Yield expression

        public Node parseYieldExpression()
        {
            Node argument;
            Node expr;
            bool @delegate = false;
            bool previousAllowYield;

            argument = null;
            expr = new Node();

            expectKeyword("yield");

            if (!hasLineTerminator)
            {
                previousAllowYield = state.allowYield;
                state.allowYield = false;
                @delegate = match("*");
                if (@delegate)
                {
                    lex();
                    argument = parseAssignmentExpression();
                }
                else
                {
                    if (!match(";") && !match("}") && !match(")") && lookahead.type != TokenType.EOF)
                    {
                        argument = parseAssignmentExpression();
                    }
                }
                state.allowYield = previousAllowYield;
            }

            return expr.finishYieldExpression(argument, @delegate);
        }

        // ECMA-262 12.14 Assignment Operators

        public Node parseAssignmentExpression()
        {
            Token token;
            Node expr;
            Node right;
            Options list;

            Token startToken;

            startToken = lookahead;
            token = lookahead;

            if (!state.allowYield && matchKeyword("yield"))
            {
                return parseYieldExpression();
            }

            expr = parseConditionalExpression();

            if (expr != null && expr.type == PlaceHolders.ArrowParameterPlaceHolder || match("=>"))
            {
                isAssignmentTarget = isBindingElement = false;
                list = reinterpretAsCoverFormalsList(expr);

                if (list != null)
                {
                    firstCoverInitializedNameError = null;
                    return parseArrowFunctionExpression(list, new Node(startToken));
                }

                return expr;
            }

            if (matchAssign())
            {
                if (!isAssignmentTarget)
                {
                    tolerateError(Messages.InvalidLHSInAssignment);
                }

                // ECMA-262 12.1.1
                if (strict && expr.type == Syntax.Identifier)
                {
                    if (isRestrictedWord(expr.name))
                    {
                        tolerateUnexpectedToken(token, Messages.StrictLHSAssignment);
                    }
                    if (isStrictModeReservedWord(expr.name))
                    {
                        tolerateUnexpectedToken(token, Messages.StrictReservedWord);
                    }
                }

                if (!match("="))
                {
                    isAssignmentTarget = isBindingElement = false;
                }
                else
                {
                    reinterpretExpressionAsPattern(expr);
                }

                token = lex();
                right = isolateCoverGrammar(parseAssignmentExpression);
                expr = new Node(startToken).finishAssignmentExpression(token.value, expr, right);
                firstCoverInitializedNameError = null;
            }

            return expr;
        }

        // ECMA-262 12.15 Comma Operator

        public Node parseExpression()
        {
            Node expr;
            Token startToken = lookahead;
            List<Node> expressions;

            expr = isolateCoverGrammar(parseAssignmentExpression);

            if (match(","))
            {
                expressions = new List<Node>() { expr };

                while (startIndex < length)
                {
                    if (!match(","))
                    {
                        break;
                    }
                    lex();
                    expressions.Add(isolateCoverGrammar(parseAssignmentExpression));
                }

                expr = new Node(startToken).finishSequenceExpression(expressions);
            }

            return expr;
        }

        // ECMA-262 13.2 Block

        public Node parseStatementListItem()
        {
            if (lookahead.type == TokenType.Keyword)
            {
                switch (lookahead.value)
                {
                    case "export":
                        if (state.sourceType != "module")
                        {
                            tolerateUnexpectedToken(lookahead, Messages.IllegalExportDeclaration);
                        }
                        return parseExportDeclaration();
                    case "import":
                        if (state.sourceType != "module")
                        {
                            tolerateUnexpectedToken(lookahead, Messages.IllegalImportDeclaration);
                        }
                        return parseImportDeclaration();
                    case "const":
                    case "let":
                        return parseLexicalDeclaration(new Options() { inFor = false });
                    case "function":
                        return parseFunctionDeclaration(new Node());
                    case "class":
                        return parseClassDeclaration(false);
                }
            }

            return parseStatement();
        }

        public List<Node> parseStatementList()
        {
            List<Node> list = new List<Node>();
            while (startIndex < length)
            {
                if (match("}"))
                {
                    break;
                }
                list.Add(parseStatementListItem());
            }

            return list;
        }

        public Node parseBlock()
        {
            List<Node> block;
            Node node = new Node();

            expect("{");

            block = parseStatementList();

            expect("}");

            return node.finishBlockStatement(block);
        }

        // ECMA-262 13.3.2 Variable Statement

        public Node parseVariableIdentifier(string kind = null)
        {
            Token token;
            Node node = new Node();

            token = lex();

            if (token.type == TokenType.Keyword && token.value.ToString() == "yield")
            {
                if (strict)
                {
                    tolerateUnexpectedToken(token, Messages.StrictReservedWord);
                }
                if (!state.allowYield)
                {
                    throwUnexpectedToken(token);
                }
            }
            else if (token.type != TokenType.Identifier)
            {
                if (strict && token.type == TokenType.Keyword && isStrictModeReservedWord(token.value))
                {
                    tolerateUnexpectedToken(token, Messages.StrictReservedWord);
                }
                else
                {
                    if (strict || token.value != "let" || kind != "var")
                    {
                        throwUnexpectedToken(token);
                    }
                }
            }
            else if (state.sourceType == "module" && token.type == TokenType.Identifier && token.value == "await")
            {
                tolerateUnexpectedToken(token);
            }

            return node.finishIdentifier(token.value);
        }

        public Node parseVariableDeclaration(Options options)
        {
            Node init = null;
            Node id, node = new Node();
            List<Token> @params = new List<Token>();

            id = parsePattern(@params, "var");

            // ECMA-262 12.2.1
            if (strict && isRestrictedWord(id.name))
            {
                tolerateError(Messages.StrictVarName);
            }

            if (match("="))
            {
                lex();
                init = isolateCoverGrammar(parseAssignmentExpression);
            }
            else if (id.type != Syntax.Identifier && !options.inFor)
            {
                expect("=");
            }

            return node.finishVariableDeclarator(id, init);
        }

        public List<Node> parseVariableDeclarationList(Options options)
        {
            List<Node> list = new List<Node>();

            do
            {
                list.Add(parseVariableDeclaration(new Options() { inFor = options.inFor }));
                if (!match(","))
                {
                    break;
                }
                lex();
            } while (startIndex < length);

            return list;
        }

        public Node parseVariableStatement(Node node)
        {

            List<Node> declarations;
            expectKeyword("var");
            declarations = parseVariableDeclarationList(new Options() { inFor = false });
            consumeSemicolon();
            return node.finishVariableDeclaration(declarations);
        }

        // ECMA-262 13.3.1 Let and Const Declarations

        public Node parseLexicalBinding(string kind, Options options)
        {
            Node init = null;
            Node id;
            Node node = new Node();
            List<Token> @params = new List<Token>();

            id = parsePattern(@params, kind);

            // ECMA-262 12.2.1
            if (strict && id.type == Syntax.Identifier && isRestrictedWord(id.name))
            {
                tolerateError(Messages.StrictVarName);
            }

            if (kind == "const")
            {
                if (!matchKeyword("in") && !matchContextualKeyword("of"))
                {
                    expect("=");
                    init = isolateCoverGrammar(parseAssignmentExpression);
                }
            }
            else if ((!options.inFor && id.type != Syntax.Identifier) || match("="))
            {
                expect("=");
                init = isolateCoverGrammar(parseAssignmentExpression);
            }

            return node.finishVariableDeclarator(id, init);
        }

        public List<Node> parseBindingList(string kind, Options options)
        {
            List<Node> list = new List<Node>();

            do
            {
                list.Add(parseLexicalBinding(kind, options));
                if (!match(","))
                {
                    break;
                }
                lex();
            } while (startIndex < length);

            return list;
        }

        public Node parseLexicalDeclaration(Options options)
        {
            string kind;
            List<Node> declarations;
            Node node = new Node();

            kind = lex().value;
            assert(kind == "let" || kind == "const", "Lexical declaration must be either let or const");

            declarations = parseBindingList(kind, options);

            consumeSemicolon();

            return node.finishLexicalDeclaration(declarations, kind);
        }

        public Node parseRestElement(List<Token> @params)
        {
            Node param;
            Node node = new Node();

            lex();

            if (match("{"))
            {
                throwError(Messages.ObjectPatternAsRestParameter);
            }

            @params.Add(lookahead);

            param = parseVariableIdentifier();

            if (match("="))
            {
                throwError(Messages.DefaultRestParameter);
            }

            if (!match(")"))
            {
                throwError(Messages.ParameterAfterRestParameter);
            }

            return node.finishRestElement(param);
        }

        // ECMA-262 13.4 Empty Statement

        public Node parseEmptyStatement(Node node)
        {
            expect(";");
            return node.finishEmptyStatement();
        }

        // ECMA-262 12.4 Expression Statement

        public Node parseExpressionStatement(Node node)
        {
            var expr = parseExpression();
            consumeSemicolon();
            return node.finishExpressionStatement(expr);
        }

        // ECMA-262 13.6 If statement

        public Node parseIfStatement(Node node)
        {
            Node test;
            Node consequent, alternate;

            expectKeyword("if");

            expect("(");

            test = parseExpression();

            expect(")");

            consequent = parseStatement();

            if (matchKeyword("else"))
            {
                lex();
                alternate = parseStatement();
            }
            else
            {
                alternate = null;
            }

            return node.finishIfStatement(test, consequent, alternate);
        }

        // ECMA-262 13.7 Iteration Statements

        public Node parseDoWhileStatement(Node node)
        {
            List<Node> body;
            Node test;
            bool oldInIteration;

            expectKeyword("do");

            oldInIteration = state.inIteration;
            state.inIteration = true;

            body = new List<Node>() { parseStatement() };

            state.inIteration = oldInIteration;

            expectKeyword("while");

            expect("(");

            test = parseExpression();

            expect(")");

            if (match(";"))
            {
                lex();
            }

            return node.finishDoWhileStatement(body, test);
        }

        public Node parseWhileStatement(Node node)
        {
            Node test;
            List<Node> body;
            bool oldInIteration;

            expectKeyword("while");

            expect("(");

            test = parseExpression();

            expect(")");

            oldInIteration = state.inIteration;
            state.inIteration = true;

            body = new List<Node>() { parseStatement() };

            state.inIteration = oldInIteration;

            return node.finishWhileStatement(test, body);
        }

        public Node parseForStatement(Node node)
        {
            Node init;
            bool forIn;
            List<Node> initSeq;
            Token initStartToken;
            Node test;
            Node update;
            Node left = null, right = null;
            string kind;
            List<Node> declarations;
            List<Node> body;
            bool oldInIteration;
            bool previousAllowIn = state.allowIn;

            init = test = update = null;
            forIn = true;

            expectKeyword("for");

            expect("(");

            if (match(";"))
            {
                lex();
            }
            else
            {
                if (matchKeyword("var"))
                {
                    init = new Node();
                    lex();

                    state.allowIn = false;
                    declarations = parseVariableDeclarationList(new Options() { inFor = true });
                    state.allowIn = previousAllowIn;

                    if (declarations.Count == 1 && matchKeyword("in"))
                    {
                        init = init.finishVariableDeclaration(declarations);
                        lex();
                        left = init;
                        right = parseExpression();
                        init = null;
                    }
                    else if (declarations.Count == 1 && declarations[0].init == null && matchContextualKeyword("of"))
                    {
                        init = init.finishVariableDeclaration(declarations);
                        lex();
                        left = init;
                        right = parseAssignmentExpression();
                        init = null;
                        forIn = false;
                    }
                    else
                    {
                        init = init.finishVariableDeclaration(declarations);
                        expect(";");
                    }
                }
                else if (matchKeyword("const") || matchKeyword("let"))
                {
                    init = new Node();
                    kind = lex().value;

                    state.allowIn = false;
                    declarations = parseBindingList(kind, new Options() { inFor = true });
                    state.allowIn = previousAllowIn;

                    if (declarations.Count == 1 && declarations[0].init == null && matchKeyword("in"))
                    {
                        init = init.finishLexicalDeclaration(declarations, kind);
                        lex();
                        left = init;
                        right = parseExpression();
                        init = null;
                    }
                    else if (declarations.Count == 1 && declarations[0].init == null && matchContextualKeyword("of"))
                    {
                        init = init.finishLexicalDeclaration(declarations, kind);
                        lex();
                        left = init;
                        right = parseAssignmentExpression();
                        init = null;
                        forIn = false;
                    }
                    else
                    {
                        consumeSemicolon();
                        init = init.finishLexicalDeclaration(declarations, kind);
                    }
                }
                else
                {
                    initStartToken = lookahead;
                    state.allowIn = false;
                    init = inheritCoverGrammar(parseAssignmentExpression);
                    state.allowIn = previousAllowIn;

                    if (matchKeyword("in"))
                    {
                        if (!isAssignmentTarget)
                        {
                            tolerateError(Messages.InvalidLHSInForIn);
                        }

                        lex();
                        reinterpretExpressionAsPattern(init);
                        left = init;
                        right = parseExpression();
                        init = null;
                    }
                    else if (matchContextualKeyword("of"))
                    {
                        if (!isAssignmentTarget)
                        {
                            tolerateError(Messages.InvalidLHSInForLoop);
                        }

                        lex();
                        reinterpretExpressionAsPattern(init);
                        left = init;
                        right = parseAssignmentExpression();
                        init = null;
                        forIn = false;
                    }
                    else
                    {
                        if (match(","))
                        {
                            initSeq = new List<Node>() { init };
                            while (match(","))
                            {
                                lex();
                                initSeq.Add(isolateCoverGrammar(parseAssignmentExpression));
                            }
                            init = new Node(initStartToken).finishSequenceExpression(initSeq);
                        }
                        expect(";");
                    }
                }
            }

            if (left != null)
            {

                if (!match(";"))
                {
                    test = parseExpression();
                }
                expect(";");

                if (!match(")"))
                {
                    update = parseExpression();
                }
            }

            expect(")");

            oldInIteration = state.inIteration;
            state.inIteration = true;

            body = new List<Node>() { isolateCoverGrammar(parseStatement) };

            state.inIteration = oldInIteration;

            return (left == null)
                ? node.finishForStatement(init, test, update, body)
                : forIn
                    ? node.finishForInStatement(left, right, body)
                    : node.finishForOfStatement(left, right, body);
        }

        // ECMA-262 13.8 The continue statement

        public Node parseContinueStatement(Node node)
        {
            Node label = null;
            string key;

            expectKeyword("continue");

            // Optimize the most common form: "continue;".
            if (source.ToCharArray()[startIndex] == 0x3B)
            {
                lex();

                if (!state.inIteration)
                {
                    throwError(Messages.IllegalContinue);
                }

                return node.finishContinueStatement(null);
            }

            if (hasLineTerminator)
            {
                if (!state.inIteration)
                {
                    throwError(Messages.IllegalContinue);
                }

                return node.finishContinueStatement(null);
            }

            if (lookahead.type == TokenType.Identifier)
            {
                label = parseVariableIdentifier();

                key = "$" + label.name;
                //if (!Object.prototype.hasOwnProperty.p(state.labelSet, key)) {
                //    throwError(Messages.UnknownLabel, label.name);
                //}
            }

            consumeSemicolon();

            if (label == null && !state.inIteration)
            {
                throwError(Messages.IllegalContinue);
            }

            return node.finishContinueStatement(label);
        }

        // ECMA-262 13.9 The break statement

        public Node parseBreakStatement(Node node)
        {
            Node label = null;
            string key;

            expectKeyword("break");

            // Catch the very common case first: immediately a semicolon (U+003B).
            if (source.ToCharArray()[lastIndex] == 0x3B)
            {
                lex();

                if (!(state.inIteration || state.inSwitch))
                {
                    throwError(Messages.IllegalBreak);
                }

                return node.finishBreakStatement(null);
            }

            if (hasLineTerminator)
            {
                if (!(state.inIteration || state.inSwitch))
                {
                    throwError(Messages.IllegalBreak);
                }

                return node.finishBreakStatement(null);
            }

            if (lookahead.type == TokenType.Identifier)
            {
                label = parseVariableIdentifier();

                //key = "$" + label.name;
                //if (!Object.prototype.hasOwnProperty.call(state.labelSet, key)) {
                //    throwError(Messages.UnknownLabel, label.name);
                //}
            }

            consumeSemicolon();

            if (label == null && !(state.inIteration || state.inSwitch))
            {
                throwError(Messages.IllegalBreak);
            }

            return node.finishBreakStatement(label);
        }

        // ECMA-262 13.10 The return statement

        public Node parseReturnStatement(Node node)
        {
            Node argument = null;

            expectKeyword("return");

            if (!state.inFunctionBody)
            {
                tolerateError(Messages.IllegalReturn);
            }

            // "return" followed by a space and an identifier is very common.
            if (source.ToCharArray()[lastIndex] == 0x20)
            {
                if (isIdentifierStart(source.ToCharArray()[lastIndex + 1]))
                {
                    argument = parseExpression();
                    consumeSemicolon();
                    return node.finishReturnStatement(argument);
                }
            }

            if (hasLineTerminator)
            {
                // HACK
                return node.finishReturnStatement(null);
            }

            if (!match(";"))
            {
                if (!match("}") && lookahead.type != TokenType.EOF)
                {
                    argument = parseExpression();
                }
            }

            consumeSemicolon();

            return node.finishReturnStatement(argument);
        }

        // ECMA-262 13.11 The with statement

        public Node parseWithStatement(Node node)
        {
            Node @object;
            List<Node> body;

            if (strict)
            {
                tolerateError(Messages.StrictModeWith);
            }

            expectKeyword("with");

            expect("(");

            @object = parseExpression();

            expect(")");

            body = new List<Node>() { parseStatement() };

            return node.finishWithStatement(@object, body);
        }

        // ECMA-262 13.12 The switch statement

        public Node parseSwitchCase()
        {
            Node test;
            List<Node> consequent = new List<Node>();
            Node statement;
            Node node = new Node();

            if (matchKeyword("default"))
            {
                lex();
                test = null;
            }
            else
            {
                expectKeyword("case");
                test = parseExpression();
            }
            expect(":");

            while (startIndex < length)
            {
                if (match("}") || matchKeyword("default") || matchKeyword("case"))
                {
                    break;
                }
                statement = parseStatementListItem();
                consequent.Add(statement);
            }

            return node.finishSwitchCase(test, consequent);
        }

        public Node parseSwitchStatement(Node node)
        {
            Node discriminant;
            List<Node> cases;
            Node clause;
            bool oldInSwitch;
            bool defaultFound;

            expectKeyword("switch");

            expect("(");

            discriminant = parseExpression();

            expect(")");

            expect("{");

            cases = new List<Node>();

            if (match("}"))
            {
                lex();
                return node.finishSwitchStatement(discriminant, cases);
            }

            oldInSwitch = state.inSwitch;
            state.inSwitch = true;
            defaultFound = false;

            while (startIndex < length)
            {
                if (match("}"))
                {
                    break;
                }
                clause = parseSwitchCase();
                if (clause.test == null)
                {
                    if (defaultFound)
                    {
                        throwError(Messages.MultipleDefaultsInSwitch);
                    }
                    defaultFound = true;
                }
                cases.Add(clause);
            }

            state.inSwitch = oldInSwitch;

            expect("}");

            return node.finishSwitchStatement(discriminant, cases);
        }

        // ECMA-262 13.14 The throw statement

        public Node parseThrowStatement(Node node)
        {

            Node argument;

            expectKeyword("throw");

            if (hasLineTerminator)
            {
                throwError(Messages.NewlineAfterThrow);
            }

            argument = parseExpression();

            consumeSemicolon();

            return node.finishThrowStatement(argument);
        }

        // ECMA-262 13.15 The try statement

        public Node parseCatchClause()
        {
            Node param;
            List<Node> @params = new List<Node>();
            //  paramMap = { },
            string key;
            int i;
            List<Node> body;
            Node node = new Node();

            expectKeyword("catch");

            expect("(");
            if (match(")"))
            {
                throwUnexpectedToken(lookahead);
            }

            param = null;// parsePattern(@params, "");
            for (i = 0; i < @params.Count; i++)
            {
                key = "$" + @params[i].value;
                //if (Object.prototype.hasOwnProperty.call(paramMap, key)) {
                //    tolerateError(Messages.DuplicateBinding, params[i].value);
                //}
                // paramMap[key] = true;
            }

            // ECMA-262 12.14.1
            if (strict && isRestrictedWord(param.name))
            {
                tolerateError(Messages.StrictCatchVariable);
            }

            expect(")");
            body = new List<Node>() { parseBlock() };
            return node.finishCatchClause(param, body);
        }

        public Node parseTryStatement(Node node)
        {
            Node block;
            Node handler = null, finalizer = null;

            expectKeyword("try");

            block = parseBlock();

            if (matchKeyword("catch"))
            {
                handler = parseCatchClause();
            }

            if (matchKeyword("finally"))
            {
                lex();
                finalizer = parseBlock();
            }

            if (handler == null && finalizer == null)
            {
                throwError(Messages.NoCatchOrFinally);
            }

            return node.finishTryStatement(block, handler, finalizer);
        }

        // ECMA-262 13.16 The debugger statement

        public Node parseDebuggerStatement(Node node)
        {
            expectKeyword("debugger");

            consumeSemicolon();

            return node.finishDebuggerStatement();
        }

        // 13 Statements

        public Node parseStatement()
        {
            var type = lookahead.type;
            Node expr = null;
            List<Node> labeledBody;
            string key;
            Node node;

            if (type == TokenType.EOF)
            {
                throwUnexpectedToken(lookahead);
            }

            if (type == TokenType.Punctuator && lookahead.value == "{")
            {
                return parseBlock();
            }
            isAssignmentTarget = isBindingElement = true;
            node = new Node();

            if (type == TokenType.Punctuator)
            {
                switch (lookahead.value)
                {
                    case ";":
                        return parseEmptyStatement(node);
                    case "(":
                        return parseExpressionStatement(node);
                    default:
                        break;
                }
            }
            else if (type == TokenType.Keyword)
            {
                switch (lookahead.value)
                {
                    case "break":
                        return parseBreakStatement(node);
                    case "continue":
                        return parseContinueStatement(node);
                    case "debugger":
                        return parseDebuggerStatement(node);
                    case "do":
                        return parseDoWhileStatement(node);
                    case "for":
                        return parseForStatement(node);
                    case "function":
                        return parseFunctionDeclaration(node);
                    case "if":
                        return parseIfStatement(node);
                    case "return":
                        return parseReturnStatement(node);
                    case "switch":
                        return parseSwitchStatement(node);
                    case "throw":
                        return parseThrowStatement(node);
                    case "try":
                        return parseTryStatement(node);
                    case "var":
                        return parseVariableStatement(node);
                    case "while":
                        return parseWhileStatement(node);
                    case "with":
                        return parseWithStatement(node);
                    default:
                        break;
                }
            }

            expr = parseExpression();

            // ECMA-262 12.12 Labelled Statements
            if ((expr.type == Syntax.Identifier) && match(":"))
            {
                lex();

                key = "$" + expr.name;
                //if (Object.prototype.hasOwnProperty.call(state.labelSet, key)) {
                //    throwError(Messages.Redeclaration, "Label", expr.name);
                //}

                state.labelSet.Add(key);// = true;
                labeledBody = new List<Node>() { parseStatement() };
                state.labelSet.Remove(key);
                return node.finishLabeledStatement(expr, labeledBody);
            }

            consumeSemicolon();

            return node.finishExpressionStatement(expr);
        }

        // ECMA-262 14.1 Function Definition

        public Node parseFunctionSourceElements()
        {
            Node statement;
            List<Node> body = new List<Node>();
            Token token;
            string directive;
            Token firstRestricted = null;
            List<string> oldLabelSet;
            bool oldInIteration;
            bool oldInSwitch, oldInFunctionBody;
            int oldParenthesisCount;
            Node node = new Node();

            expect("{");

            while (startIndex < length)
            {
                if (lookahead.type != TokenType.StringLiteral)
                {
                    break;
                }
                token = lookahead;

                statement = parseStatementListItem();
                body.Add(statement);
                if (statement.expression.type != Syntax.Literal)
                {
                    // this is not directive
                    break;
                }
                directive = source.Substring(token.start + 1, token.end - 1);
                if (directive == "use strict")
                {
                    strict = true;
                    if (firstRestricted != null)
                    {
                        tolerateUnexpectedToken(firstRestricted, Messages.StrictOctalLiteral);
                    }
                }
                else
                {
                    if (firstRestricted == null && token.octal)
                    {
                        firstRestricted = token;
                    }
                }
            }

            oldLabelSet = state.labelSet;
            oldInIteration = state.inIteration;
            oldInSwitch = state.inSwitch;
            oldInFunctionBody = state.inFunctionBody;
            oldParenthesisCount = state.parenthesizedCount;

            state.labelSet = new List<string>();
            state.inIteration = false;
            state.inSwitch = false;
            state.inFunctionBody = true;
            state.parenthesizedCount = 0;

            while (startIndex < length)
            {
                if (match("}"))
                {
                    break;
                }
                body.Add(parseStatementListItem());
            }

            expect("}");

            state.labelSet = oldLabelSet;
            state.inIteration = oldInIteration;
            state.inSwitch = oldInSwitch;
            state.inFunctionBody = oldInFunctionBody;
            state.parenthesizedCount = oldParenthesisCount;

            return node.finishBlockStatement(body);
        }

        public void validateParam(Options options, Node param, string name)
        {
            //var key = "$" + name;
            //if (strict)
            //{
            //    if (isRestrictedWord(name))
            //    {
            //        options.stricted = param;
            //        options.message = Messages.StrictParamName;
            //    }
            //    //if (Object.prototype.hasOwnProperty.call(options.paramSet, key)) {
            //    //    options.stricted = param;
            //    //    options.message = Messages.StrictParamDupe;
            //    //}
            //}
            //else if (!options.firstRestricted)
            //{
            //    if (isRestrictedWord(name))
            //    {
            //        options.firstRestricted = param;
            //        options.message = Messages.StrictParamName;
            //    }
            //    else if (isStrictModeReservedWord(name))
            //    {
            //        options.firstRestricted = param;
            //        options.message = Messages.StrictReservedWord;
            //    }
            //    //} else if (Object.prototype.hasOwnProperty.call(options.paramSet, key)) {
            //    //    options.stricted = param;
            //    //    options.message = Messages.StrictParamDupe;
            //    //}
            //}
            //options.paramSet[key] = true;
        }

        public bool parseParam(Options options)
        {
            Token token;
            Node param;
            List<Token> @params = new List<Token>();
            int i;
            Node def = null;

            token = lookahead;
            if (token.value == "...")
            {
                param = parseRestElement(@params);
                validateParam(options, param.argument, param.argument.name);
                options.@params.Add(param);
                options.defaults.Add(null);
                return false;
            }

            param = parsePatternWithDefault(@params, "");
            for (i = 0; i < @params.Count; i++)
            {
                //validateParam(options, @params[i], "");//, @params[i].value);
            }

            if (param.type == Syntax.AssignmentPattern)
            {
                def = param.right;
                param = param.left;
                ++options.defaultCount;
            }

            options.@params.Add(param);
            options.defaults.Add(def);

            return !match(")");
        }

        public Options parseParams(Token firstRestricted = null)
        {
            Options options;

            options = new Options
            {
                @params = new List<Node>(),
                defaultCount = 0,
                defaults = new List<Node>(),
                firstRestricted = firstRestricted
            };

            expect("(");

            if (!match(")"))
            {
                //options.paramSet =
                //{
                //}
                //;
                while (startIndex < length)
                {
                    if (!parseParam(options))
                    {
                        break;
                    }
                    expect(",");
                }
            }

            expect(")");

            if (options.defaultCount == 0)
            {
                options.defaults = new List<Node>();
            }

            return new Options()
            {
                @params = options.@params,
                defaults = options.defaults,
                stricted = options.stricted,
                firstRestricted = options.firstRestricted,
                message = options.message
            };
        }

        public Node parseFunctionDeclaration(Node node, bool identifierIsOptional = true)
        {
            Node id = null;
            List<Node> @params = new List<Node>();
            List<Node> defaults = new List<Node>();
            List<Node> body;
            Token token;
            Token stricted;
            Options tmp;
            Token firstRestricted = null;
            string message = "";
            bool previousStrict;
            bool isGenerator;
            bool previousAllowYield;

            previousAllowYield = state.allowYield;

            expectKeyword("function");

            isGenerator = match("*");
            if (isGenerator)
            {
                lex();
            }

            if (!identifierIsOptional || !match("("))
            {
                token = lookahead;
                id = parseVariableIdentifier();
                if (strict)
                {
                    if (isRestrictedWord(token.value))
                    {
                        tolerateUnexpectedToken(token, Messages.StrictFunctionName);
                    }
                }
                else
                {
                    if (isRestrictedWord(token.value))
                    {
                        firstRestricted = token;
                        message = Messages.StrictFunctionName;
                    }
                    else if (isStrictModeReservedWord(token.value))
                    {
                        firstRestricted = token;
                        message = Messages.StrictReservedWord;
                    }
                }
            }

            state.allowYield = !isGenerator;
            tmp = parseParams(firstRestricted);
            @params = tmp.@params;
            defaults = tmp.defaults;
            stricted = tmp.stricted;
            firstRestricted = tmp.firstRestricted;
            if (tmp.message != null)
            {
                message = tmp.message;
            }


            previousStrict = strict;
            body = new List<Node>() { parseFunctionSourceElements() };
            if (strict && firstRestricted != null)
            {
                throwUnexpectedToken(firstRestricted, message);
            }
            if (strict && stricted != null)
            {
                tolerateUnexpectedToken(stricted, message);
            }

            strict = previousStrict;
            state.allowYield = previousAllowYield;

            return node.finishFunctionDeclaration(id, @params, defaults, body, isGenerator);
        }

        public Node parseFunctionExpression()
        {
            Token token;
            Node id = null;
            Token stricted;
            Token firstRestricted = null;
            string message = "";
            Options tmp;
            List<Node> @params = new List<Node>();

            List<Node> defaults = new List<Node>();
            List<Node> body;
            bool previousStrict;
            Node node = new Node();
            bool isGenerator;
            bool previousAllowYield;

            previousAllowYield = state.allowYield;

            expectKeyword("function");

            isGenerator = match("*");
            if (isGenerator)
            {
                lex();
            }

            state.allowYield = !isGenerator;
            if (!match("("))
            {
                token = lookahead;
                id = (!strict && !isGenerator && matchKeyword("yield"))
                    ? parseNonComputedProperty()
                    : parseVariableIdentifier();
                if (strict)
                {
                    if (isRestrictedWord(token.value))
                    {
                        tolerateUnexpectedToken(token, Messages.StrictFunctionName);
                    }
                }
                else
                {
                    if (isRestrictedWord(token.value))
                    {
                        firstRestricted = token;
                        message = Messages.StrictFunctionName;
                    }
                    else if (isStrictModeReservedWord(token.value))
                    {
                        firstRestricted = token;
                        message = Messages.StrictReservedWord;
                    }
                }
            }

            tmp = parseParams(firstRestricted);
            @params = tmp.@params;
            defaults = tmp.defaults;
            stricted = tmp.stricted;
            firstRestricted = tmp.firstRestricted;
            if (tmp.message != null)
            {
                message = tmp.message;
            }

            previousStrict = strict;
            body = new List<Node>() { parseFunctionSourceElements() };
            if (strict && firstRestricted != null)
            {
                throwUnexpectedToken(firstRestricted, message);
            }
            if (strict && stricted != null)
            {
                tolerateUnexpectedToken(stricted, message);
            }
            strict = previousStrict;
            state.allowYield = previousAllowYield;

            return node.finishFunctionExpression(id, @params, defaults, body, isGenerator);
        }

        // ECMA-262 14.5 Class Definitions
        public Node parseClassBody()
        {
            Node classBody;
            Token token;
            bool isStatic, hasConstructor = false;
            List<Node> body;

            Node method;
            bool computed;
            Node key = null;

            classBody = new Node();

            expect("{");
            body = new List<Node>();
            while (!match("}"))
            {
                if (match(";"))
                {
                    lex();
                }
                else
                {
                    method = new Node();
                    token = lookahead;
                    isStatic = false;
                    computed = match("[");
                    if (match("*"))
                    {
                        lex();
                    }
                    else
                    {
                        key = parseObjectPropertyKey();
                        if (key.name == "static" && (lookaheadPropertyName() || match("*")))
                        {
                            token = lookahead;
                            isStatic = true;
                            computed = match("[");
                            if (match("*"))
                            {
                                lex();
                            }
                            else
                            {
                                key = parseObjectPropertyKey();
                            }
                        }
                    }
                    method = tryParseMethodDefinition(token, key, computed, method);
                    if (method != null)
                    {
                        // method["static"] = isStatic; // jscs:ignore requireDotNotation
                        if (method.kind == "init")
                        {
                            method.kind = "method";
                        }
                        if (!isStatic)
                        {
                            if (!method.computed && (method.key.name != null || method.key.value.ToString() == "constructor"))
                            {
                                if (method.kind != "method" || !method.method || method.value.generator)
                                {
                                    throwUnexpectedToken(token, Messages.ConstructorSpecialMethod);
                                }
                                if (hasConstructor)
                                {
                                    throwUnexpectedToken(token, Messages.DuplicateConstructor);
                                }
                                else
                                {
                                    hasConstructor = true;
                                }
                                method.kind = "constructor";
                            }
                        }
                        else
                        {
                            if (!method.computed && (method.key.name != null || method.key.value.ToString() == "prototype"))
                            {
                                throwUnexpectedToken(token, Messages.StaticPrototype);
                            }
                        }
                        method.type = Syntax.MethodDefinition;
                        //delete
                        method.method = false;
                        //delete
                        method.shorthand = false;
                        body.Add(method);
                    }
                    else
                    {
                        throwUnexpectedToken(lookahead);
                    }
                }
            }
            lex();
            return classBody.finishClassBody(body);
        }

        public Node parseClassDeclaration(bool identifierIsOptional)
        {
            Node id = null;
            Node superClass = null;
            Node classNode = new Node();
            List<Node> classBody;
            bool previousStrict = strict;
            strict = true;

            expectKeyword("class");

            if (!identifierIsOptional || lookahead.type == TokenType.Identifier)
            {
                id = parseVariableIdentifier();
            }

            if (matchKeyword("extends"))
            {
                lex();
                superClass = isolateCoverGrammar(parseLeftHandSideExpressionAllowCall);
            }
            classBody = new List<Node>() { parseClassBody() };
            strict = previousStrict;

            return classNode.finishClassDeclaration(id, superClass, classBody);
        }

        public Node parseClassExpression()
        {
            Node id = null;
            Node superClass = null;
            Node classNode = new Node();
            List<Node> classBody;
            bool previousStrict = strict;
            strict = true;

            expectKeyword("class");

            if (lookahead.type == TokenType.Identifier)
            {
                id = parseVariableIdentifier();
            }

            if (matchKeyword("extends"))
            {
                lex();
                superClass = isolateCoverGrammar(parseLeftHandSideExpressionAllowCall);
            }
            classBody = new List<Node>() { parseClassBody() };
            strict = previousStrict;

            return classNode.finishClassExpression(id, superClass, classBody);
        }

        // ECMA-262 15.2 Modules

        public Node parseModuleSpecifier()
        {
            var node = new Node();

            if (lookahead.type != TokenType.StringLiteral)
            {
                throwError(Messages.InvalidModuleSpecifier);
            }
            return node.finishLiteral(lex());
        }

        // ECMA-262 15.2.3 Exports
        public Node parseExportSpecifier()
        {
            Node exported = null;
            Node local;
            Node node = new Node();
            Node def;
            if (matchKeyword("default"))
            {
                // export {default} from "something";
                def = new Node();
                lex();
                local = def.finishIdentifier("default");
            }
            else
            {
                local = parseVariableIdentifier();
            }
            if (matchContextualKeyword("as"))
            {
                lex();
                exported = parseNonComputedProperty();
            }
            return node.finishExportSpecifier(local, exported);
        }

        public Node parseExportNamedDeclaration(Node node)
        {
            Node declaration = null;
            bool isExportFromIdentifier = false;
            string src = null;
            List<Node> specifiers = new List<Node>();

            // non-default export
            if (lookahead.type == TokenType.Keyword)
            {
                // covers:
                // export var f = 1;
                switch (lookahead.value)
                {
                    case "let":
                    case "const":
                    case "var":
                    case "class":
                    case "function":
                        declaration = parseStatementListItem();
                        return node.finishExportNamedDeclaration(declaration, specifiers, null);
                }
            }

            expect("{");
            while (!match("}"))
            {
                isExportFromIdentifier = isExportFromIdentifier || matchKeyword("default");
                specifiers.Add(parseExportSpecifier());
                if (!match("}"))
                {
                    expect(",");
                    if (match("}"))
                    {
                        break;
                    }
                }
            }
            expect("}");

            if (matchContextualKeyword("from"))
            {
                // covering:
                // export {default} from "foo";
                // export {foo} from "foo";
                lex();
                //src = parseModuleSpecifier();
                consumeSemicolon();
            }
            else if (isExportFromIdentifier)
            {
                // covering:
                // export {default}; // missing fromClause
                throwError(lookahead.value != null
                    ? Messages.UnexpectedToken
                    : Messages.MissingFromClause, lookahead.value);
            }
            else
            {
                // cover
                // export {foo};
                consumeSemicolon();
            }
            return node.finishExportNamedDeclaration(declaration, specifiers, src);
        }

        public Node parseExportDefaultDeclaration(Node node)
        {
            Node declaration = null,
                expression = null;

            // covers:
            // export default ...
            expectKeyword("default");

            if (matchKeyword("function"))
            {
                // covers:
                // export default function foo () {}
                // export default function () {}
                declaration = parseFunctionDeclaration(new Node(), true);
                return node.finishExportDefaultDeclaration(declaration);
            }
            if (matchKeyword("class"))
            {
                declaration = parseClassDeclaration(true);
                return node.finishExportDefaultDeclaration(declaration);
            }

            if (matchContextualKeyword("from"))
            {
                throwError(string.Format(Messages.UnexpectedToken, lookahead.value));
            }

            // covers:
            // export default {};
            // export default [];
            // export default (1 + 2);
            if (match("{"))
            {
                expression = parseObjectInitializer();
            }
            else if (match("["))
            {
                expression = parseArrayInitializer();
            }
            else
            {
                expression = parseAssignmentExpression();
            }
            consumeSemicolon();
            return node.finishExportDefaultDeclaration(expression);
        }

        public Node parseExportAllDeclaration(Node node)
        {
            string src = null;

            // covers:
            // export * from "foo";
            expect("*");
            if (!matchContextualKeyword("from"))
            {
                throwError(lookahead.value != null
                    ? Messages.UnexpectedToken
                    : Messages.MissingFromClause, lookahead.value);
            }
            lex();
            //src = parseModuleSpecifier();
            consumeSemicolon();

            return node.finishExportAllDeclaration(src);
        }

        public Node parseExportDeclaration()
        {
            var node = new Node();
            if (state.inFunctionBody)
            {
                throwError(Messages.IllegalExportDeclaration);
            }

            expectKeyword("export");

            if (matchKeyword("default"))
            {
                return parseExportDefaultDeclaration(node);
            }
            if (match("*"))
            {
                return parseExportAllDeclaration(node);
            }
            return parseExportNamedDeclaration(node);
        }

        // ECMA-262 15.2.2 Imports
        public Node parseImportSpecifier()
        {
            // import {<foo as bar>} ...;
            Node local = null;
            Node imported;
            Node node = new Node();

            imported = parseNonComputedProperty();
            if (matchContextualKeyword("as"))
            {
                lex();
                local = parseVariableIdentifier();
            }

            return node.finishImportSpecifier(local, imported);
        }

        private List<Node> parseNamedImports()
        {
            List<Node> specifiers = new List<Node>();
            // {foo, bar as bas}
            expect("{");
            while (!match("}"))
            {
                specifiers.Add(parseImportSpecifier());
                if (!match("}"))
                {
                    expect(",");
                    if (match("}"))
                    {
                        break;
                    }
                }
            }
            expect("}");
            return specifiers;
        }

        public Node parseImportDefaultSpecifier()
        {
            // import <foo> ...;
            Node local;
            Node node = new Node();

            local = parseNonComputedProperty();

            return node.finishImportDefaultSpecifier(local);
        }

        public Node parseImportNamespaceSpecifier()
        {
            // import <* as foo> ...;
            Node local;
            Node node = new Node();

            expect("*");
            if (!matchContextualKeyword("as"))
            {
                throwError(Messages.NoAsAfterImportNamespace);
            }
            lex();
            local = parseNonComputedProperty();

            return node.finishImportNamespaceSpecifier(local);
        }

        public Node parseImportDeclaration()
        {
            List<Node> specifiers = new List<Node>();
            Node src;
            Node node = new Node();

            if (state.inFunctionBody)
            {
                throwError(Messages.IllegalImportDeclaration);
            }

            expectKeyword("import");

            if (lookahead.type == TokenType.StringLiteral)
            {
                // import "foo";
                src = parseModuleSpecifier();
            }
            else
            {

                if (match("{"))
                {
                    // import {bar}
                    specifiers = specifiers.Concat(parseNamedImports()).ToList();
                }
                else if (match("*"))
                {
                    // import * as foo
                    specifiers.Add(parseImportNamespaceSpecifier());
                }
                else if (isIdentifierName(lookahead) && !matchKeyword("default"))
                {
                    // import foo
                    specifiers.Add(parseImportDefaultSpecifier());
                    if (match(","))
                    {
                        lex();
                        if (match("*"))
                        {
                            // import foo, * as foo
                            specifiers.Add(parseImportNamespaceSpecifier());
                        }
                        else if (match("{"))
                        {
                            // import foo, {bar}
                            specifiers = specifiers.Concat(parseNamedImports()).ToList();
                        }
                        else
                        {
                            throwUnexpectedToken(lookahead);
                        }
                    }
                }
                else
                {
                    throwUnexpectedToken(lex());
                }

                if (!matchContextualKeyword("from"))
                {
                    throwError(
                        string.IsNullOrEmpty(lookahead.value) ? Messages.UnexpectedToken : Messages.MissingFromClause,
                        lookahead.value);
                }
                lex();
                src = parseModuleSpecifier();
            }

            consumeSemicolon();
            return node.finishImportDeclaration(specifiers, src.ToString());
        }

        // ECMA-262 15.1 Scripts
        public List<Node> parseScriptBody()
        {
            Node statement;
            List<Node> body = new List<Node>();
            Token token;
            string directive;
            Token firstRestricted = null;

            while (startIndex < length)
            {
                token = lookahead;
                if (token.type != TokenType.StringLiteral)
                {
                    break;
                }

                statement = parseStatementListItem();
                body.Add(statement);
                if (statement.expression.type != Syntax.Literal)
                {
                    // this is not directive
                    break;
                }
                directive = source.Substring(token.start + 1, token.end - 1);
                if (directive == "use strict")
                {
                    strict = true;
                    if (firstRestricted != null)
                    {
                        tolerateUnexpectedToken(firstRestricted, Messages.StrictOctalLiteral);
                    }
                }
                else
                {
                    if (firstRestricted == null && token.octal)
                    {
                        firstRestricted = token;
                    }
                }
            }

            while (startIndex < length)
            {
                statement = parseStatementListItem();
                /* istanbul ignore if */
                if (statement == null)
                {
                    break;
                }
                body.Add(statement);
            }
            return body;
        }

        public Node parseProgram()
        {
            List<Node> body;
            Node node;

            peek();
            node = new Node();

            body = parseScriptBody();
            return node.finishProgram(body, state.sourceType);
        }

        public void filterTokenLocation()
        {
            int i;
            Token entry, token;
            List<Token> tokens = new List<Token>();

            for (i = 0; i < extra.tokens.Count; ++i)
            {
                entry = extra.tokens[i];
                token = entry;// new Token()
                //{
                //    type = entry.type,
                //    value = entry.value
                //};
                if (entry.regex != null)
                {
                    token.regex = new Regex()
                    {
                        pattern = entry.regex.pattern,
                        flags = entry.regex.flags
                    };
                }
                if (extra.range)
                {
                    token.range = entry.range;
                }
                if (extra.loc != null)
                {
                    token.loc = entry.loc;
                }
                tokens.Add(token);
            }

            extra.tokens = tokens;
        }

        public List<Token> tokenize(string code, Options options)
        {
            //var toString;
            List<Token> tokens;

            //toString = String;
            //if (typeof code !== "string" && !(code instanceof String)) {
            //    code = toString(code);
            //}

            source = code;
            index = 0;
            lineNumber = (source.Length > 0) ? 1 : 0;
            lineStart = 0;
            startIndex = index;
            startLineNumber = lineNumber;
            startLineStart = lineStart;
            length = source.Length;
            lookahead = null;
            state = new State()
            {
                allowIn = true,
                allowYield = true,
                labelSet = new List<string>(),
                inFunctionBody = false,
                inIteration = false,
                inSwitch = false,
                lastCommentStart = -1,
                curlyStack = new Stack<string>()
            };

            extra = new Extra();

            // Options matching.
            options = options ?? new Options();

            // Of course we collect tokens here.
            options.tokens = true;
            extra.tokens = new List<Token>();
            extra.tokenize = true;
            // The following two fields are necessary to compute the Regex tokens.
            extra.openParenToken = -1;
            extra.openCurlyToken = -1;

            extra.range = options.range;
            extra.loc = options.loc;

            if (options.comment)
            {
                extra.comments = new List<Comment>();
            }
            if (options.tolerant)
            {
                extra.errors = new List<Error>();
            }

            //try
            //{
            peek();
            if (lookahead.type == TokenType.EOF)
            {
                return extra.tokens;
            }

            lex();
            while (lookahead.type != TokenType.EOF)
            {
                try
                {
                    lex();
                }
                catch (Error lexError)
                {
                    if (extra.errors != null)
                    {
                        recordError(lexError);
                        // We have to break on the first error
                        // to avoid infinite loops.
                        break;
                    }
                    else
                    {
                        throw lexError;
                    }
                }
            }

            filterTokenLocation();
            tokens = extra.tokens;
            //tokens.comments = extra.comments;
            //tokens.errors = extra.errors;

            //}
            //catch (Exception e)
            //{
            //    throw e;
            //}
            //finally
            //{
            //    extra = new Extra();
            //}
            return tokens;
        }

        public Node parse(string code, Options options)
        {
            // Program program, toString;

            //toString = String;
            //if (typeof code !== "string" && !(code instanceof String)) {
            //    code = toString(code);
            //}

            source = code;
            index = 0;
            lineNumber = (source.Length > 0) ? 1 : 0;
            lineStart = 0;
            startIndex = index;
            startLineNumber = lineNumber;
            startLineStart = lineStart;
            length = source.Length;
            lookahead = null;
            state = new State()
            {
                allowIn = true,
                allowYield = true,
                labelSet = new List<string>(),
                inFunctionBody = false,
                inIteration = false,
                inSwitch = false,
                lastCommentStart = -1,
                curlyStack = new Stack<string>(),
                sourceType = "script"
            };
            strict = false;

            extra = new Extra();
            if (options != null)
            {
                extra.range = options.range;
                extra.loc = options.loc;
                extra.attachComment = options.attachComment;

                //if (extra.loc && options.source != null && options.source != undefined) {
                //    extra.source = toString(options.source);
                //}

                if (options.tokens)
                {
                    extra.tokens = new List<Token>();
                }
                if (options.comment)
                {
                    extra.comments = new List<Comment>();
                }
                if (options.tolerant)
                {
                    extra.errors = new List<Error>();
                }
                if (extra.attachComment)
                {
                    extra.range = true;
                    extra.comments = new List<Comment>();
                    extra.bottomRightStack = new List<Token>();
                    extra.trailingComments = new List<Comment>();
                    extra.leadingComments = new List<Comment>();
                }
                if (options.sourceType == "module")
                {
                    // very restrictive condition for now
                    state.sourceType = options.sourceType;
                    strict = true;
                }
            }

            //try
            //{
            var program = parseProgram();
            //if (typeof extra.comments !== "undefined") {
            //    program.comments = extra.comments;
            //}
            //if (typeof extra.tokens !== "undefined") {
            //    filterTokenLocation();
            //    program.tokens = extra.tokens;
            //}
            //if (typeof extra.errors !== "undefined") {
            //    program.errors = extra.errors;
            //}
            //}
            //catch (Exception e)
            //{


            //    throw e;
            //}
            //finally
            //{
            //    extra = new Extra();
            //}

            return program;
        }

        // Sync with *.json manifests.
        //exports.version = "2.5.0";

        //exports.tokenize = tokenize;

        //exports.parse = parse;

        //// Deep copy.
        /* istanbul ignore next */
        //exports.Syntax = (function () {
        //    var name, types = {};

        //if (typeof Object.create == "function") {
        //    types = Object.create(null);
        //}

        //for (name in Syntax) {
        //    if (Syntax.hasOwnProperty(name)) {
        //        types[name] = Syntax[name];
        //    }
        //}

        //if (typeof Object.freeze == "function") {
        //    Object.freeze(types);
        //}

    }

    // return types;
    // }());

};
/* vim: set sw=4 ts=4 et tw=80 : */