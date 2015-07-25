using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;

namespace HackCompiler.Modules
{
    /// <summary>
    /// Effects the actual compilation output.
    /// helpful link: http://nand2tetris-questions-and-answers-forum.32033.n3.nabble.com/CompilationEngine-Input-stream-td4027136.html
    /// JackAnalyzer is the top level of the compiler.  In my compiler it is, in fact, a function named main(). 
    ///main() parses the command line arguments and builds a list of input .jack files. For each file in the list, it creates a CompileEngine object with the input file name and corresponding output file name. It then calls CompileEngine.CompileClass(). 
    ///Each CompileEngine creates a JackTokenizer to read the input file and an XmlWriter (ch 10 version) or a VmWriter (ch 11 version). 
    /// </summary>
    public class CompilationEngine
    {
        private JackTokenizer _tokenizer;
        private StringBuilder _xmlTokens;
        private string[] statements = { "let", "if", "while", "do", "return" };
        private string[] decTypes = { "static", "field" };
        private string[] subTypes = { "constructor", "function", "method" };



        public bool HasErrors
        {
            get { return _tokenizer.HasErrors; }
        }
        public List<TokenizedObject> Tokens
        {
            get { return _tokenizer.Tokens; }
        }

        public string Xml { get; set; }
        /// <summary>
        /// Constructor: creates a new compilation engine with the given input and output. The next routine called must be compileClass().
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        public CompilationEngine(string inputFile, string outputFile)
        {
            //1. call Tokenizer

            _xmlTokens = new StringBuilder();
            _tokenizer = new JackTokenizer(inputFile);

            //WriteXml("<tokens>"); //<tokens>

            //first check for class element

            _tokenizer.Advance();

            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD && _tokenizer.KeyWord() == "class")
            {
                CompileClass();
            }
            else
            {
                //error: no class struct
                _tokenizer.RecordError("expecting class struct");
            }


            //WriteXml("</tokens>"); //</tokens>

            string xml = _xmlTokens.ToString();

            Xml = xml;
        }

        private void WriteXml(string writeThis)
        {
            _xmlTokens.Append(writeThis + Environment.NewLine);

        }

        /// <summary>
        /// Compiles a complete class.
        /// We'll use the jack programming grammar found on page 208/209 to check for compilation errors etc.
        /// </summary>
        public void CompileClass()
        {
            WriteXml("<class>"); //<class>
            WriteCurrentToken();

            //according to the grammar we should next have a className identifier
            _tokenizer.Advance();

            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER)
            {
                WriteCurrentToken();

                _tokenizer.Advance();

                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL)
                {
                    WriteCurrentToken();

                    _tokenizer.Advance();

                    //now we determine if we have class variable declarations or subroutine declaration
                    //looping through the variable declarations first, if there are any.
                    while (_tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD && decTypes.Contains(_tokenizer.KeyWord()) && !_tokenizer.HasErrors)
                    {
                        CompileClassVarDec();
                    }

                    //looping through the subroutine declarations, if there are any.
                    while (_tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD && subTypes.Contains(_tokenizer.KeyWord()) && !_tokenizer.HasErrors)
                    {
                        CompileSubroutine();
                    }
                    
                }
                else
                {
                    //error: expecting '{'
                    _tokenizer.RecordError("expecting '{'");

                }
            }
            else
            {
                //error: expecting className
                _tokenizer.RecordError("expecting className");

            }


            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == "}")
            {
                WriteCurrentToken();
            }
            else
            {
                _tokenizer.RecordError("expecting '}' end of class");

            }

            WriteXml("</class>"); //</class>
        }

        private void WriteCurrentToken()
        {
            switch (_tokenizer.TokenType)
            {
                case Enums.Enumerations.TokenType.SYMBOL:
                    WriteXml("<symbol>"); //<symbol>
                    WriteXml(_tokenizer.Symbol()); //{
                    WriteXml("</symbol>");  //</symbol>
                    break;
                case Enums.Enumerations.TokenType.KEYWORD:
                    WriteXml("<keyword>"); //<keyword>
                    WriteXml(_tokenizer.KeyWord()); //{
                    WriteXml("</keyword>");  //</keyword>
                    break;
                case Enums.Enumerations.TokenType.IDENTIFIER:
                    WriteXml("<identifier>"); //<identifier>
                    WriteXml(_tokenizer.Identifier()); //{
                    WriteXml("</identifier>");  //</identifier>
                    break;
                case Enums.Enumerations.TokenType.INT_CONST:
                    WriteXml("<integerConstant>"); //<integerConstant>
                    WriteXml(_tokenizer.IntVal().ToString()); //{
                    WriteXml("</integerConstant>");  //</integerConstant>
                    break;
                case Enums.Enumerations.TokenType.STRING_CONST:
                    WriteXml("<stringConstant>"); //<stringConstant>
                    WriteXml(_tokenizer.StringVal()); //{
                    WriteXml("</stringConstant>");  //</stringConstant>
                    break;
                default:
                    _tokenizer.RecordError("unknown token type");
                    break;
            }
        }

        /// <summary>
        /// Compiles a static declaration or a field declaration.
        /// </summary>
        public void CompileClassVarDec()
        {
            WriteXml("<classVarDec>");

            WriteCurrentToken(); //either field or static

            _tokenizer.Advance();

            //'int' | 'char' | 'boolean' | className
            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD || _tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER)
            {
                WriteCurrentToken();

                //varName
                _tokenizer.Advance();

                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER)
                {
                    WriteCurrentToken();

                    _tokenizer.Advance();

                    if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == ",")
                    {
                        WriteCurrentToken();

                        //need to iterate through and collect all possible varName identifiers...
                        var hasMore = true;

                        while (hasMore && !_tokenizer.HasErrors)
                        {
                            

                            _tokenizer.Advance();

                            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER)
                            {
                                WriteCurrentToken();

                                _tokenizer.Advance();

                                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == ",")
                                {
                                    WriteCurrentToken();


                                }
                                else
                                {
                                    hasMore = false;
                                }
                            }
                            else
                            {
                                _tokenizer.RecordError("expected identifier");
                            }
                        }
                    }
                    
                    if(_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == ";")
                    {
                        WriteCurrentToken();
                        _tokenizer.Advance();
                    }
                    else
                    {
                        _tokenizer.RecordError("expected ';'");
                    }
                }
                else
                {
                    _tokenizer.RecordError("expected 'varName' identifier");
                }
            }
            else
            {
                _tokenizer.RecordError("expected 'int' | 'char' | 'boolean' | className");
            }

            WriteXml("</classVarDec>");
        }

        /// <summary>
        /// Compiles a complete method, function, or constructor.
        /// </summary>
        public void CompileSubroutine()
        {
            var subType = _tokenizer.KeyWord();
            WriteXml("<subroutineDec>"); //<subroutineDec>

            if (subTypes.Contains(subType))
            {
                WriteCurrentToken();

                _tokenizer.Advance();

                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD || _tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER) //should either be 'void' or a type
                {
                    WriteCurrentToken();

                    _tokenizer.Advance();

                    if (_tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER) //subroutineName
                    {
                        WriteCurrentToken();

                        _tokenizer.Advance();

                        if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == "(")
                        {
                            WriteCurrentToken();

                            _tokenizer.Advance();

                            CompileParameterList(); //this will handle empty parameterLists as well

                            WriteCurrentToken();

                            _tokenizer.Advance(); //moving on to the subroutineBody

                            CompileSubroutineBody();


                        }
                        else
                        {
                            //error: expected (
                            _tokenizer.RecordError("expected '(' parameterList ')'");
                        }
                    }
                    else
                    {
                        //error: expected subroutineName
                        _tokenizer.RecordError("expected subroutineName");

                    }
                }
                else
                {
                    //error: expected 'void' or type
                    _tokenizer.RecordError("expected 'void' or type");

                }
            }
            else
            {
                //error: expected (constructor | function | method)
                _tokenizer.RecordError("expected (constructor | function | method)");

            }

            WriteXml("</subroutineDec>");  //</subroutineDec>
        }

        public void CompileSubroutineBody()
        {
            WriteXml("<subroutineBody>"); //<subroutineBody>

            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == "{")
            {
                WriteCurrentToken();

                //check varDec  
                _tokenizer.Advance();

                //if (_tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD && _tokenizer.KeyWord() == "var")//we have some variable declarations
                //{
                //    CompileVarDec();
                //}

                while (_tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD && _tokenizer.KeyWord() == "var" && !_tokenizer.HasErrors)
                {
                    CompileVarDec();
                }

                //now check to see what kind of statements we have in our subroutine...
                CompileStatements();


                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == "}")
                {
                    WriteCurrentToken();

                    _tokenizer.Advance();
                }
                else
                {
                    //error: expected }
                    _tokenizer.RecordError("expected '}'");
                }
            }
            else
            {
                //error: expected {
                _tokenizer.RecordError("expected '{'");
            }

            WriteXml("</subroutineBody>"); //</subroutineBody>
        }

        /// <summary>
        /// Compiles a (possibly empty) parameter list, not including the enclosing "()".
        /// </summary>
        public void CompileParameterList()
        {
            WriteXml("<parameterList>");//<parameterList>
            //check to see if it's an empty parameter list
            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == ")")
            {
                //empty parameter list
            }
            else if (_tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD || _tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER) //type 
            {
                WriteCurrentToken();

                _tokenizer.Advance();

                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER) //varName
                {
                    WriteCurrentToken();
                    _tokenizer.Advance();

                    if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == ",")
                    {
                        WriteCurrentToken(); //,

                        //need to iterate through and collect all possible parameters...
                        var hasMore = true;

                        while (hasMore && !_tokenizer.HasErrors)
                        {


                            _tokenizer.Advance();

                            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD || _tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER) //type
                            {
                                WriteCurrentToken();

                                _tokenizer.Advance();

                                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER) //varName
                                {
                                    WriteCurrentToken();

                                    _tokenizer.Advance();
                                }

                                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == ",")
                                {
                                    WriteCurrentToken();


                                }
                                else
                                {
                                    hasMore = false;
                                }
                            }
                            else
                            {
                                _tokenizer.RecordError("expected identifier");
                            }
                        }
                    }
                }
                else
                {
                    _tokenizer.RecordError("expected varName");
                }

               
            }
            else
            {
                //error: expected type
                _tokenizer.RecordError("expected type");

            }

            WriteXml("</parameterList>"); //</parameterList>
        }

        /// <summary>
        /// Compiles a var declaration.
        /// </summary>
        public void CompileVarDec()
        {
            WriteXml("<varDec>"); //<varDec>

            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD && _tokenizer.KeyWord() == "var")
            {
                WriteCurrentToken();

                _tokenizer.Advance();

                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD || _tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER)
                {
                    WriteCurrentToken();

                    _tokenizer.Advance();

                    if (_tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER)
                    {
                        WriteCurrentToken();

                        _tokenizer.Advance();

                       
                        if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == ",")
                        {
                            WriteCurrentToken();

                            //need to iterate through and collect all possible varName identifiers...
                            var hasMore = true;

                            while (hasMore && !_tokenizer.HasErrors)
                            {


                                _tokenizer.Advance();

                                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER)
                                {
                                    WriteCurrentToken();
                                    _tokenizer.Advance();


                                    if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == ",")
                                    {
                                        WriteCurrentToken();


                                    }
                                    else
                                    {
                                        hasMore = false;
                                    }
                                }
                                else
                                {
                                    _tokenizer.RecordError("expected identifier");
                                }
                            }
                        }
                      
                    }
                    else
                    {
                        _tokenizer.RecordError("expected 'varName'");
                    }
                }
                else
                {
                    _tokenizer.RecordError("expected 'type'");
                }
            }
            else
            {
                _tokenizer.RecordError("expected 'var' keyword");
            }
            //WriteCurrentToken();

            //_tokenizer.Advance();

            //WriteCurrentToken();

            //_tokenizer.Advance();

            //WriteCurrentToken();

            //_tokenizer.Advance();

            //WriteCurrentToken();

            //may need to refactor later to allow for multiple var decs here...
            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == ";")
            {
                WriteCurrentToken();

                _tokenizer.Advance();
            }
            else
            {
                _tokenizer.RecordError("expected ';'");
            }

            WriteXml("</varDec>"); //</varDec> 

            //_tokenizer.Advance(); //following lookahead procedure to stay consistent here...
        }

        /// <summary>
        /// Compiles a sequence of state-ments, not including the enclosing "{}".
        /// </summary>
        public void CompileStatements()
        {
            WriteXml("<statements>"); //<statements>

            while (_tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD && statements.Contains(_tokenizer.KeyWord()) && _tokenizer.HasErrors == false)
            {
                var statementType = _tokenizer.KeyWord();

                if (statementType == "let")
                {
                    CompileLet();
                }
                else if (statementType == "if")
                {
                    CompileIf();
                }
                else if (statementType == "while")
                {
                    CompileWhile();
                }
                else if (statementType == "do")
                {
                    CompileDo();
                }
                else if (statementType == "return")
                {
                    CompileReturn();
                }
            }
           

            WriteXml("</statements>"); //</statements>

        }

        /// <summary>
        /// Compiles a do statement.
        /// Grammar:
        /// 'do' subroutineCall ';'
        /// </summary>
        public void CompileDo()
        {
            WriteXml("<doStatement>");

            WriteCurrentToken();

            _tokenizer.Advance();

            CompileSubroutineCall();

            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == ";")
            {
                WriteCurrentToken();

                _tokenizer.Advance(); //look ahead token
            }
            else
            {
                _tokenizer.RecordError("expected: ';'");
            }

            WriteXml("</doStatement>");

        }

        private void CompileSubroutineCall()
        {
            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER)
            {
                WriteCurrentToken();

                _tokenizer.Advance();

                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL)
                {
                    var symbol = _tokenizer.Symbol();

                    if (symbol == "(")
                    {
                        WriteCurrentToken();

                        //expressionList

                        CompileExpressionList();

                        if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == ")")
                        {
                            WriteCurrentToken();

                            _tokenizer.Advance(); //look ahead
                        }
                        else
                        {
                            _tokenizer.RecordError("expected: ')'");
                        }

                    }
                    else if (symbol == ".")
                    {
                        WriteCurrentToken();

                        _tokenizer.Advance();

                        if (_tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER)
                        {
                            WriteCurrentToken();

                            _tokenizer.Advance();

                            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == "(")
                            {
                                WriteCurrentToken();

                                //expressionList

                                CompileExpressionList();

                                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == ")")
                                {
                                    WriteCurrentToken();

                                    _tokenizer.Advance(); //look ahead
                                }
                                else
                                {
                                    _tokenizer.RecordError("expected: ')'");
                                }
                            }
                            else
                            {
                                _tokenizer.RecordError("expected: '('");
                            }
                        }
                        else
                        {
                            _tokenizer.RecordError("expected: identifier");
                        }
                    }
                    else
                    {
                        _tokenizer.RecordError("expected: '(' or '.'");
                    }
                }
            }
            else
            {
                _tokenizer.RecordError("expected: subroutineName, className, or varName");
            }
        }

        /// <summary>
        /// Compiles a let statement.
        /// grammar:
        /// 'let' varName('[' expression ']')? '=' expression ';'
        /// </summary>
        public void CompileLet()
        {
            WriteXml("<letStatement>");

            WriteCurrentToken();

            _tokenizer.Advance();

            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER)
            {
                WriteCurrentToken();

                _tokenizer.Advance();

                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL)
                {
                    WriteCurrentToken();

                    _tokenizer.Advance();

                    //expression
                    CompileExpression();

                    if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == ";")
                    {
                        WriteCurrentToken();

                        _tokenizer.Advance(); //here's a look ahead token
                    }
                    else
                    {
                        _tokenizer.RecordError("expected: ;");
                    }
                }
                else
                {
                    _tokenizer.RecordError("expected: ('[' expression ']')? '=' expression ';'");

                }
            }
            else
            {
                _tokenizer.RecordError("expected: 'let' varName");
            }

            WriteXml("</letStatement>");
        }

        /// <summary>
        /// Compiles a while statement.
        /// </summary>
        public void CompileWhile()
        {
            WriteXml("<whileStatement>");

            WriteCurrentToken(); //while

            _tokenizer.Advance();

            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == "(")
            {
                WriteCurrentToken(); //(

                _tokenizer.Advance();

                CompileExpression();

                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == ")")
                {
                    WriteCurrentToken();

                    _tokenizer.Advance();

                    if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == "{")
                    {
                        WriteCurrentToken();

                        _tokenizer.Advance();

                        CompileStatements();

                        if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == "}")
                        {
                            WriteCurrentToken();

                            _tokenizer.Advance();
                        }
                        else
                        {
                            //error: expected }
                            _tokenizer.RecordError("expected '}'");
                        }
                    }

                }
                else
                {
                    _tokenizer.RecordError("expected '(' expression ')'");
                }
            }
            else
            {
                _tokenizer.RecordError("expected '(' expression ')'");
            }

            WriteXml("</whileStatement>");

        }

        /// <summary>
        /// Compiles a return statement.
        /// </summary>
        public void CompileReturn()
        {
            WriteXml("<returnStatement>");

            WriteCurrentToken();

            _tokenizer.Advance();

            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == ";")
            {
                WriteCurrentToken();

                _tokenizer.Advance(); //look ahead
            }
            else
            {
                CompileExpression();
                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == ";")
                {
                    WriteCurrentToken();

                    _tokenizer.Advance(); //look ahead
                }
                else
                {
                    _tokenizer.RecordError("expected ';'");
                }
            }

            WriteXml("</returnStatement>");
        }

        /// <summary>
        /// Compiles an if statement, possibly with a trailing else clause.
        /// </summary>
        public void CompileIf()
        {
            WriteXml("<ifStatement>");

            WriteCurrentToken(); //if

            _tokenizer.Advance();

            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == "(")
            {
                WriteCurrentToken(); //(

                _tokenizer.Advance();

                CompileExpression();

                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == ")")
                {
                    WriteCurrentToken();

                    _tokenizer.Advance();

                    if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == "{")
                    {
                        WriteCurrentToken();

                        _tokenizer.Advance();

                        CompileStatements();

                        if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == "}")
                        {
                            WriteCurrentToken();

                            _tokenizer.Advance();
                        }
                        else
                        {
                            //error: expected }
                            _tokenizer.RecordError("expected '}'");
                        }
                    }

                }
                else
                {
                    _tokenizer.RecordError("expected '(' expression ')'");
                }
            }
            else
            {
                _tokenizer.RecordError("expected '(' expression ')'");
            }

            WriteXml("</ifStatement>");

        }

        /// <summary>
        /// Compiles an expression.
        /// </summary>
        public void CompileExpression()
        {
            WriteXml("<expression>");
            CompileTerm();

            _tokenizer.Advance();
            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL)
            {
                //(op term)*

            }
            WriteXml("</expression>");
        }

        /// <summary>
        /// Compiles a term. This routine is faced with a slight difficulty when trying to decide between some of the alternative parsing rules. 
        /// Specifically, if the current token is an identifier, the routine must distinguish between a variable, an array entry, and a subroutine call.
        /// A single look ahead token, which may be one of "[", "(", or "." suffices to distinguisn
        /// between the three possibilities. Any other token is not part of this term and should not be advanced over.
        /// </summary>
        public void CompileTerm()
        {
            WriteXml("<term>");
            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER)
            {
                WriteCurrentToken();
            }
            WriteXml("</term>");
        }

        /// <summary>
        /// Compiles a (possibly empty) comma-separated list of expressions.
        /// </summary>
        public void CompileExpressionList()
        {
            WriteXml("<expressionList>");

            _tokenizer.Advance(); 

            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER)
            {
                CompileExpression();
            }

            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == ",")
            {
                WriteCurrentToken();

                //need to iterate through and collect all possible varName identifiers...
                var hasMore = true;

                while (hasMore && !_tokenizer.HasErrors)
                {


                    _tokenizer.Advance();

                    if (_tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER)
                    {
                        CompileExpression();

                        if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == ",")
                        {
                            WriteCurrentToken();


                        }
                        else
                        {
                            hasMore = false;
                        }
                    }
                    else
                    {
                        _tokenizer.RecordError("expected identifier");
                    }
                }
            }
            
            WriteXml("</expressionList>");

        }

    }
}
