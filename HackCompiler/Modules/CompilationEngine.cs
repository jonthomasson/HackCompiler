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
        public bool HasErrors {
            get { return _tokenizer.HasErrors; }
        }
        public List<TokenizedObject> Tokens {
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

            WriteXml("<tokens>"); //<tokens>

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


            WriteXml("</tokens>"); //</tokens>

            string xml = _xmlTokens.ToString();

            Xml = xml;
        }

        private void WriteXml(string writeThis){
            _xmlTokens.Append(writeThis + Environment.NewLine);
      
        }

        /// <summary>
        /// Compiles a complete class.
        /// We'll use the jack programming grammar found on page 208/209 to check for compilation errors etc.
        /// </summary>
        public void CompileClass()
        {
            WriteXml("<class>"); //<class>
            WriteXml("<keyword>"); //<keyword>
            WriteXml(_tokenizer.KeyWord());
            WriteXml("</keyword>");//</keyword>

            //according to the grammar we should next have a className identifier
            _tokenizer.Advance();

            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER)
            {
                WriteXml("<identifier>"); //<identifier>
                WriteXml(_tokenizer.Identifier()); //className
                WriteXml("</identifier>");  //</identifier>

                _tokenizer.Advance();

                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL)
                {
                    WriteXml("<symbol>"); //<symbol>
                    WriteXml(_tokenizer.Symbol()); //{
                    WriteXml("</symbol>");  //</symbol>

                    _tokenizer.Advance(); 

                    //now we determine if we have class variable declarations or subroutine declaration
                    if (_tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD)
                    {
                        var keyword = _tokenizer.KeyWord();
                        if (keyword == "static" || keyword == "field")
                        {
                            //classVarDec
                            CompileClassVarDec();
                        }
                        else
                        {
                            //subroutineDec
                            CompileSubroutine();
                        }
                    }
                    else
                    {
                        //error: expecting class variable declarations or subroutine declaration
                        _tokenizer.RecordError("expecting class variable declarations or subroutine declaration");
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

            WriteXml("</class>"); //</class>
        }

        /// <summary>
        /// Compiles a static declaration or a field declaration.
        /// </summary>
        public void CompileClassVarDec()
        {

        }

        /// <summary>
        /// Compiles a complete method, function, or constructor.
        /// </summary>
        public void CompileSubroutine()
        {
            var subType = _tokenizer.KeyWord();
            WriteXml("<subroutineDec>"); //<subroutineDec>

            if (subType == "constructor" || subType == "function" || subType == "method")
            {
                WriteXml("<keyword>"); //<keyword>
                WriteXml(subType);

                WriteXml("</keyword>");  //</keyword>

                _tokenizer.Advance();

                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD) //should either be 'void' or a type
                {
                    WriteXml("<keyword>"); //<keyword>
                    WriteXml(_tokenizer.KeyWord());

                    WriteXml("</keyword>");  //</keyword>

                    _tokenizer.Advance();

                    if (_tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER) //subroutineName
                    {
                        WriteXml("<identifier>"); //<identifier>
                        WriteXml(_tokenizer.Identifier());

                        WriteXml("</identifier>");  //</identifier>

                        _tokenizer.Advance();

                        if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == "(")
                        {
                            WriteXml("<symbol>"); //<symbol>
                            WriteXml(_tokenizer.Symbol()); //(

                            WriteXml("</symbol>");  //</symbol>

                            _tokenizer.Advance();

                            CompileParameterList(); //this will handle empty parameterLists as well

                            WriteXml("<symbol>"); //<symbol>
                            WriteXml(_tokenizer.Symbol()); //)

                            WriteXml("</symbol>");  //</symbol>

                            _tokenizer.Advance(); //moving on to the subroutineBody

                            CompileSubroutineBody();

                          
                        }
                        else
                        {
                            //error: expected (
                            _tokenizer.RecordError("expected '('");
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
            WriteXml("subroutineBody"); //<subroutineBody>

            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == "{")
            {
                WriteXml("<symbol>"); //<symbol>
                WriteXml(_tokenizer.Symbol()); //{

                WriteXml("</symbol>");  //</symbol>
                
                //check varDec  
                _tokenizer.Advance();

                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD && _tokenizer.KeyWord() == "var")//we have some variable declarations
                {
                    CompileVarDec();
                }

                //now check to see what kind of statements we have in our subroutine...
                CompileStatements();
               

                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == "}")
                {
                    WriteXml("<symbol>"); //<symbol>
                    WriteXml(_tokenizer.Symbol()); //}

                    WriteXml("</symbol>");  //</symbol>
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
            else if (_tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD)
            {

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

            WriteXml("<keyword>"); //<keyword>
            WriteXml(_tokenizer.KeyWord()); //var
            WriteXml("</keyword>");  //</keyword>

            _tokenizer.Advance();

            WriteXml("<identifier>"); //<identifier>
            WriteXml(_tokenizer.Identifier()); //type
            WriteXml("</identifier>");  //</identifier>

            _tokenizer.Advance();

            WriteXml("<identifier>"); //<identifier>
            WriteXml(_tokenizer.Identifier()); //varName
            WriteXml("</identifier>");  //</identifier>

            _tokenizer.Advance();

            WriteXml("<symbol>"); //<symbol>
            WriteXml(_tokenizer.Symbol()); //;
            WriteXml("</symbol>");  //</symbol>

            //may need to refactor later to allow for multiple var decs here...

            WriteXml("</varDec>"); //</varDec> 

            _tokenizer.Advance(); //following lookahead procedure to stay consistent here...
        }

        /// <summary>
        /// Compiles a sequence of state-ments, not including the enclosing "{}".
        /// </summary>
        public void CompileStatements()
        {
            WriteXml("<statements>"); //<statements>

            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD)
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
        /// </summary>
        public void CompileDo()
        {

        }

        /// <summary>
        /// Compiles a let statement.
        /// </summary>
        public void CompileLet()
        {

        }

        /// <summary>
        /// Compiles a while statement.
        /// </summary>
        public void CompileWhile()
        {

        }

        /// <summary>
        /// Compiles a return statement.
        /// </summary>
        public void CompileReturn()
        {

        }

        /// <summary>
        /// Compiles an if statement, possibly with a trailing else clause.
        /// </summary>
        public void CompileIf()
        {

        }

        /// <summary>
        /// Compiles an expression.
        /// </summary>
        public void CompileExpression()
        {

        }

        /// <summary>
        /// Compiles a term. This routine is faced with a slight difficulty when trying to decide between some of the alternative parsing rules. 
        /// Specifically, if the current token is an identifier, the routine must distinguish between a variable, an array entry, and a subroutine call.
        /// A single look ahead token, which may be one of "[", "(", or "." suffices to distinguisn
        /// between the three possibilities. Any other token is not part of this term and should not be advanced over.
        /// </summary>
        public void CompileTerm()
        {

        }

        /// <summary>
        /// Compiles a (possibly empty) comma-separated list of expressions.
        /// </summary>
        public void CompileExpressionList()
        {

        }

    }
}
