using System.Collections.Generic;
using Esprima.Net;

namespace Esprima.NET.Nodes
{
    public class Node //= Node.prototype = {
    {
        public Node right { get; set; }
        public Node left { get; set; }
        public string type { get; set; }
        public bool generator { get; set; }
        public bool shorthand { get; set; }
        public bool method { get; set; }
        public string kind { get; set; }
        public Node value { get; set; }
        public bool computed { get; set; }
        public Node key { get; set; }
        public object defaults { get; set; }
        public Node expression { get; set; }
        public string @operator { get; set; }
        public object arguments { get; set; }
        public object callee { get; set; }
        public List<Node> elements { get; set; }
        public object alternate { get; set; }
        public object consequent { get; set; }
        public object test { get; set; }
        public bool each { get; set; }
        public Node id { get; set; }
        public Node param { get; set; }
        public Node superClass { get; set; }
        public object @object { get; set; }
        public List<Node> body { get; set; }
        public object init { get; set; }
        public object update { get; set; }
        public object label { get; set; }
        public List<Node> @params { get; set; }
        public string name { get; set; }
        public object property { get; set; }
        public List<Node> properties { get; set; }
        public object cases { get; set; }
        public object quasi { get; set; }
        public Node local { get; set; }
        public string source { get; set; }
        public List<Node> specifiers { get; set; }
        public object imported { get; set; }
        public object declaration { get; set; }
        public Node exported { get; set; }
        public bool? @delegate { get; set; }
        public object tag { get; set; }
        public object tail { get; set; }
        public object discriminant { get; set; }
        public List<Node> expressions { get; set; }
        public object meta { get; set; }
        public object finalizer { get; set; }
        public object handler { get; set; }
        public List<object> handlers { get; set; }
        public List<object> guardedHandlers { get; set; }
        public object block { get; set; }
        public object declarations { get; set; }
        public bool prefix { get; set; }
        public object sourceType { get; set; }
        public Range range { get; set; }
        public List<Comment> trailingComments { get; set; }
        public List<Comment> leadingComments { get; set; }
        public object quasis { get; set; }
        public Node argument { get; set; }

        public override string ToString()
        {
            return base.ToString() + ": " + this.type;
        }

        public Node()
        {
            if (Esprima.extra != null)
            {
                if (Esprima.extra.range)
                {
                    this.range = new Range() { Start = Esprima.startIndex, End = 0 };
                }
                if (Esprima.extra.loc != null)
                {
                    this.loc = new Loc();
                }
            }
        }

        public Node(Token startToken)
        {
            if (Esprima.extra != null)
            {
                //if (extra.range)
                //{
                this.range = new Range() { Start = startToken.start, End = 0 };
                // }
                if (Esprima.extra.loc != null)
                {
                    this.loc = Esprima.WrappingSourceLocation(startToken);
                }
            }
        }

        public Loc loc { get; set; }

        public void processComment()
        {
            Extra lastChild = null;
            List<Comment> leadingComments = null;
            List<Comment> trailingComments = null;
            var bottomRight = Esprima.extra.bottomRightStack;
            int i;
            Comment comment;
            var last = bottomRight[bottomRight.Count - 1];

            if (this.type == Syntax.Program)
            {
                if (this.body.Count > 0)
                {
                    return;
                }
            }

            if (Esprima.extra.trailingComments.Count > 0)
            {
                trailingComments = new List<Comment>();
                for (i = Esprima.extra.trailingComments.Count - 1; i >= 0; --i)
                {
                    comment = Esprima.extra.trailingComments[i];
                    //if (comment.range[0] >= this.range[1])
                    //{
                    trailingComments.Insert(0, comment);
                    Esprima.extra.trailingComments.RemoveRange(i, 1);
                    //}
                }
                Esprima.extra.trailingComments = new List<Comment>();
            }
            else
            {
                //if (last != null && last.trailingComments != null &&
                //    last.trailingComments[0].range[0] >= this.range[1])
                //{
                //    trailingComments = last.trailingComments;
                //    last.trailingComments.Clear();
                //}
            }

            // Eating the stack.
            //while (last && last.range[0] >= this.range[0])
            //{
            //   // lastChild = bottomRight.pop();
            //    last = bottomRight.Last();
            //}

            if (lastChild != null)
            {
                if (lastChild.leadingComments != null)
                {
                    leadingComments = new List<Comment>();
                    for (i = lastChild.leadingComments.Count - 1; i >= 0; --i)
                    {
                        comment = lastChild.leadingComments[i];
                        //if (comment.range[1] <= this.range[0])
                        //{
                        //    leadingComments.Insert(0, comment);
                        //    lastChild.leadingComments.RemoveRange(i, 1);
                        //}
                    }

                    //if (lastChild.leadingComments== null) {
                    //    lastChild.leadingComments = undefined;
                    //}
                }
            }
            else if (Esprima.extra.leadingComments.Count > 0)
            {
                leadingComments = new List<Comment>();
                for (i = Esprima.extra.leadingComments.Count - 1; i >= 0; --i)
                {
                    comment = Esprima.extra.leadingComments[i];
                    //if (comment.range[1] <= this.range[0])
                    //{
                    //    leadingComments.Insert(0, comment);
                    //    extra.leadingComments.RemoveRange(i, 1);
                    //}
                }
            }


            if (leadingComments != null && leadingComments.Count > 0)
            {
                this.leadingComments = leadingComments;
            }
            if (trailingComments != null && trailingComments.Count > 0)
            {
                this.trailingComments = trailingComments;
            }

            //  bottomRight.Add(this);
        }

        public void finish()
        {
            if (Esprima.extra.range)
            {
                this.range.End = Esprima.lastIndex;
            }
            if (Esprima.extra.loc != null)
            {
                this.loc.end = new Loc.Position()
                {
                    line = Esprima.lastLineNumber,
                    column = Esprima.lastIndex - Esprima.lastLineStart
                };
                if (Esprima.extra.source != null)
                {
                    this.loc.source = Esprima.extra.source;
                }
            }

            if (Esprima.extra.attachComment)
            {
                this.processComment();
            }
        }

        public Node finishArrayExpression(List<Node> elements)
        {
            this.type = Syntax.ArrayExpression;
            this.elements = elements;
            this.finish();
            return this;
        }

        public Node finishArrayPattern(List<Node> elements)
        {
            this.type = Syntax.ArrayPattern;
            this.elements = elements;
            this.finish();
            return this;
        }

        public Node finishArrowFunctionExpression(List<Node> @params, object defaults, List<Node> body, Node expression)
        {
            this.type = Syntax.ArrowFunctionExpression;
            this.id = null;
            this.@params = @params;
            this.defaults = defaults;
            this.body = body;
            this.generator = false;
            this.expression = expression;
            this.finish();
            return this;
        }


        public Node finishAssignmentExpression(string @operator, Node left, Node right)
        {
            this.type = Syntax.AssignmentExpression;
            this.@operator = @operator;
            this.left = left;
            this.right = right;
            this.finish();
            return this;
        }


        public Node finishAssignmentPattern(Node left, Node right)
        {
            this.type = Syntax.AssignmentPattern;
            this.left = left;
            this.right = right;
            this.finish();
            return this;
        }

        public Node finishBinaryExpression(string @operator, Node left, Node right)
        {
            this.type = (@operator == "||" || @operator == "&&")
                ? Syntax.LogicalExpression
                : Syntax.BinaryExpression;
            this.@operator = @operator;
            this.left = left;
            this.right = right;
            this.finish();
            return this;
        }

        public Node finishBlockStatement(List<Node> body)
        {
            this.type = Syntax.BlockStatement;
            this.body = body;
            this.finish();
            return this;
        }

        public Node finishBreakStatement(object label)
        {
            this.type = Syntax.BreakStatement;
            this.label = label;
            this.finish();
            return this;
        }

        public Node finishCallExpression(object callee, object args)
        {
            this.type = Syntax.CallExpression;
            this.callee = callee;
            this.arguments = args;
            this.finish();
            return this;
        }


        public Node finishCatchClause(Node param, List<Node> body)
        {
            this.type = Syntax.CatchClause;
            this.param = param;
            this.body = body;
            this.finish();
            return this;
        }

        public Node finishClassBody(List<Node> body)
        {
            this.type = Syntax.ClassBody;
            this.body = body;
            this.finish();
            return this;
        }

        public Node finishClassDeclaration(Node id, Node superClass, List<Node> body)
        {
            this.type = Syntax.ClassDeclaration;
            this.id = id;
            this.superClass = superClass;
            this.body = body;
            this.finish();
            return this;
        }

        public Node finishClassExpression(Node id, Node superClass, List<Node> body)
        {
            this.type = Syntax.ClassExpression;
            this.id = id;
            this.superClass = superClass;
            this.body = body;
            this.finish();
            return this;
        }


        public Node finishConditionalExpression(object test, object consequent, object alternate)
        {
            this.type = Syntax.ConditionalExpression;
            this.test = test;
            this.consequent = consequent;
            this.alternate = alternate;
            this.finish();
            return this;
        }


        public Node finishContinueStatement(object label)
        {
            this.type = Syntax.ContinueStatement;
            this.label = label;
            this.finish();
            return this;
        }


        public Node finishDebuggerStatement()
        {
            this.type = Syntax.DebuggerStatement;
            this.finish();
            return this;
        }

        public Node finishDoWhileStatement(List<Node> body, object test)
        {
            this.type = Syntax.DoWhileStatement;
            this.body = body;
            this.test = test;
            this.finish();
            return this;
        }

        public Node finishEmptyStatement()
        {
            this.type = Syntax.EmptyStatement;
            this.finish();
            return this;
        }

        public Node finishExpressionStatement(Node expression)
        {
            this.type = Syntax.ExpressionStatement;
            this.expression = expression;
            this.finish();
            return this;
        }

        public Node finishForStatement(object init, object test, object update, List<Node> body)
        {
            this.type = Syntax.ForStatement;
            this.init = init;
            this.test = test;
            this.update = update;
            this.body = body;
            this.finish();
            return this;
        }


        public Node finishForOfStatement(Node left, Node right, List<Node> body)
        {
            this.type = Syntax.ForOfStatement;
            this.left = left;
            this.right = right;
            this.body = body;
            this.finish();
            return this;
        }

        public Node finishForInStatement(Node left, Node right, List<Node> body)
        {
            this.type = Syntax.ForInStatement;
            this.left = left;
            this.right = right;
            this.body = body;
            this.each = false;
            this.finish();
            return this;
        }


        public Node finishFunctionDeclaration(Node id, List<Node> @params, object defaults, List<Node> body,
            bool generator)
        {
            this.type = Syntax.FunctionDeclaration;
            this.id = id;
            this.@params = @params;
            this.defaults = defaults;
            this.body = body;
            this.generator = generator;
            this.expression = null;
            this.finish();
            return this;
        }

        public Node finishFunctionExpression(Node id, List<Node> @params, object defaults, List<Node> body,
            bool generator)
        {
            this.type = Syntax.FunctionExpression;
            this.id = id;
            this.@params = @params;
            this.defaults = defaults;
            this.body = body;
            this.generator = generator;
            this.expression = null;
            this.finish();
            return this;
        }


        public Node finishIdentifier(string name)
        {
            this.type = Syntax.Identifier;
            this.name = name;
            this.finish();
            return this;
        }


        public Node finishIfStatement(object test, object consequent, object alternate)
        {
            this.type = Syntax.IfStatement;
            this.test = test;
            this.consequent = consequent;
            this.alternate = alternate;
            this.finish();
            return this;
        }

        public Node finishLabeledStatement(object label, List<Node> body)
        {
            this.type = Syntax.LabeledStatement;
            this.label = label;
            this.body = body;
            this.finish();
            return this;
        }

        public Node finishLiteral(Token token)
        {
            this.type = Syntax.Literal;
            // this.value = ;
            this.raw = token.value;// source.Substring(token.start, token.end - token.start);
            if (token.regex != null)
            {
                this.regex = token.regex;
            }
            this.finish();
            return this;
        }

        public Regex regex { get; set; }

        public string raw { get; set; }

        public Node finishMemberExpression(string accessor, object @object, object property)
        {
            this.type = Syntax.MemberExpression;
            this.computed = accessor == "[";
            this.@object = @object;
            this.property = property;
            this.finish();
            return this;
        }


        public Node finishMetaProperty(object meta, object property)
        {
            this.type = Syntax.MetaProperty;
            this.meta = meta;
            this.property = property;
            this.finish();
            return this;
        }


        public Node finishNewExpression(object callee, object args)
        {
            this.type = Syntax.NewExpression;
            this.callee = callee;
            this.arguments = args;
            this.finish();
            return this;
        }

        public Node finishObjectExpression(List<Node> properties)
        {
            this.type = Syntax.ObjectExpression;
            this.properties = properties;
            this.finish();
            return this;
        }


        public Node finishObjectPattern(List<Node> properties)
        {
            this.type = Syntax.ObjectPattern;
            this.properties = properties;
            this.finish();
            return this;
        }

        public Node finishPostfixExpression(string @operator, Node argument)
        {
            this.type = Syntax.UpdateExpression;
            this.@operator = @operator;
            this.argument = argument;
            this.prefix = false;
            this.finish();
            return this;
        }

        public Node finishProgram(List<Node> body, object sourceType)
        {
            this.type = Syntax.Program;
            this.body = body;
            this.sourceType = sourceType;
            this.finish();
            return this;
        }


        public Node finishProperty(string kind, Node key, bool computed, Node value, bool method, bool shorthand)
        {
            this.type = Syntax.Property;
            this.key = key;
            this.computed = computed;
            this.value = value;
            this.kind = kind;
            this.method = method;
            this.shorthand = shorthand;
            this.finish();
            return this;
        }


        public Node finishRestElement(Node argument)
        {
            this.type = Syntax.RestElement;
            this.argument = argument;
            this.finish();
            return this;
        }

        public Node finishReturnStatement(Node argument)
        {
            this.type = Syntax.ReturnStatement;
            this.argument = argument;
            this.finish();
            return this;
        }

        public Node finishSequenceExpression(List<Node> expressions)
        {
            this.type = Syntax.SequenceExpression;
            this.expressions = expressions;
            this.finish();
            return this;
        }


        public Node finishSpreadElement(Node argument)
        {
            this.type = Syntax.SpreadElement;
            this.argument = argument;
            this.finish();
            return this;
        }


        public Node finishSwitchCase(object test, object consequent)
        {
            this.type = Syntax.SwitchCase;
            this.test = test;
            this.consequent = consequent;
            this.finish();
            return this;
        }

        public Node finishSuper()
        {
            this.type = Syntax.Super;
            this.finish();
            return this;
        }

        public Node finishSwitchStatement(object discriminant, object cases)
        {
            this.type = Syntax.SwitchStatement;
            this.discriminant = discriminant;
            this.cases = cases;
            this.finish();
            return this;
        }

        public Node finishTaggedTemplateExpression(object tag, object quasi)
        {
            this.type = Syntax.TaggedTemplateExpression;
            this.tag = tag;
            this.quasi = quasi;
            this.finish();
            return this;
        }


        public Node finishTemplateElement(Node value, object tail)
        {
            this.type = Syntax.TemplateElement;
            this.value = value;
            this.tail = tail;
            this.finish();
            return this;
        }

        public Node finishTemplateLiteral(object quasis, List<Node> expressions)
        {
            this.type = Syntax.TemplateLiteral;
            this.quasis = quasis;
            this.expressions = expressions;
            this.finish();
            return this;
        }

        public Node finishThisExpression()
        {
            this.type = Syntax.ThisExpression;
            this.finish();
            return this;
        }

        public Node finishThrowStatement(Node argument)
        {
            this.type = Syntax.ThrowStatement;
            this.argument = argument;
            this.finish();
            return this;
        }

        public Node finishTryStatement(Node block, Node handler, Node finalizer)
        {
            this.type = Syntax.TryStatement;
            this.block = block;
            this.guardedHandlers = new List<object>();
            this.handlers = handler != null ? new List<object>() { handler } : new List<object>();
            this.handler = handler;
            this.finalizer = finalizer;
            this.finish();
            return this;
        }

        public Node finishUnaryExpression(string @operator, Node argument)
        {
            this.type = (@operator == "++" || @operator == "--") ? Syntax.UpdateExpression : Syntax.UnaryExpression;
            this.@operator = @operator;
            this.argument = argument;
            this.prefix = true;
            this.finish();
            return this;
        }

        public Node finishVariableDeclaration(object declarations)
        {
            this.type = Syntax.VariableDeclaration;
            this.declarations = declarations;
            this.kind = "var";
            this.finish();
            return this;
        }

        public Node finishLexicalDeclaration(object declarations, string kind)
        {
            this.type = Syntax.VariableDeclaration;
            this.declarations = declarations;
            this.kind = kind;
            this.finish();
            return this;
        }

        public Node finishVariableDeclarator(Node id, object init)
        {
            this.type = Syntax.VariableDeclarator;
            this.id = id;
            this.init = init;
            this.finish();
            return this;
        }

        public Node finishWhileStatement(object test, List<Node> body)
        {
            this.type = Syntax.WhileStatement;
            this.test = test;
            this.body = body;
            this.finish();
            return this;
        }

        public Node finishWithStatement(object @object, List<Node> body)
        {
            this.type = Syntax.WithStatement;
            this.@object = @object;
            this.body = body;
            this.finish();
            return this;
        }

        public Node finishExportSpecifier(Node local, Node exported)
        {
            this.type = Syntax.ExportSpecifier;
            this.exported = exported ?? local;
            this.local = local;
            this.finish();
            return this;
        }


        public Node finishImportDefaultSpecifier(Node local)
        {
            this.type = Syntax.ImportDefaultSpecifier;
            this.local = local;
            this.finish();
            return this;
        }

        public Node finishImportNamespaceSpecifier(Node local)
        {
            this.type = Syntax.ImportNamespaceSpecifier;
            this.local = local;
            this.finish();
            return this;
        }

        public Node finishExportNamedDeclaration(object declaration, List<Node> specifiers, string src)
        {
            this.type = Syntax.ExportNamedDeclaration;
            this.declaration = declaration;
            this.specifiers = specifiers;
            this.source = src;
            this.finish();
            return this;
        }

        public Node finishExportDefaultDeclaration(object declaration)
        {
            this.type = Syntax.ExportDefaultDeclaration;
            this.declaration = declaration;
            this.finish();
            return this;
        }

        public Node finishExportAllDeclaration(string src)
        {
            this.type = Syntax.ExportAllDeclaration;
            this.source = src;
            this.finish();
            return this;
        }

        public Node finishImportSpecifier(Node local, Node imported)
        {
            this.type = Syntax.ImportSpecifier;
            this.local = local ?? imported;
            this.imported = imported;
            this.finish();
            return this;
        }

        public Node finishImportDeclaration(List<Node> specifiers, string src)
        {
            this.type = Syntax.ImportDeclaration;
            this.specifiers = specifiers;
            this.source = src;
            this.finish();
            return this;
        }

        public Node finishYieldExpression(Node argument, bool @delegate)
        {
            this.type = Syntax.YieldExpression;
            this.argument = argument;
            this.@delegate = @delegate;
            this.finish();
            return this;
        }
    };
}