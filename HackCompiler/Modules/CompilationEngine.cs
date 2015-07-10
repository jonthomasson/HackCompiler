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
        private XmlWriter _xmlTokens;
        /// <summary>
        /// Constructor: creates a new compilation engine with the given input and output. The next routine called must be compileClass().
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        public CompilationEngine(string inputFile, string outputFile)
        {
            //1. call Tokenizer
            var stream = new MemoryStream();
            _xmlTokens = XmlWriter.Create(stream);
            _tokenizer = new JackTokenizer(inputFile);

            _xmlTokens.WriteStartElement("tokens"); //<tokens>

            //first check for class element

            _tokenizer.Advance();

            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD && _tokenizer.KeyWord() == "class")
            {
                CompileClass();
            }
            else
            {
                //error: no class struct
            }


            _xmlTokens.WriteEndElement(); //</tokens>

            _xmlTokens.Flush();
            stream.Position = 0; //rewind the stream. This will eventually be the input to the CompilationEngine class.
            string xml = new StreamReader(stream).ReadToEnd();
          
        }

        /// <summary>
        /// Compiles a complete class.
        /// We'll use the jack programming grammar found on page 208/209 to check for compilation errors etc.
        /// </summary>
        public void CompileClass()
        {
            _xmlTokens.WriteStartElement("class"); //<class>
            
            //according to the grammar we should next have a className identifier
            _tokenizer.Advance();

            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER)
            {
                _xmlTokens.WriteStartElement("identifier"); //<identifier>
                _xmlTokens.WriteString(_tokenizer.Identifier()); //className
                _xmlTokens.WriteEndElement();  //</identifier>

                _tokenizer.Advance();

                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL)
                {
                    _xmlTokens.WriteStartElement("symbol"); //<symbol>
                    _xmlTokens.WriteString(_tokenizer.Symbol()); //{
                    _xmlTokens.WriteEndElement();  //</identifier>

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
                    }
                }
                else
                {
                    //error: expecting '{'
                }
            }
            else
            {
                //error: expecting className
            }
 
            _xmlTokens.WriteEndElement(); //</class>
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
            _xmlTokens.WriteStartElement("subroutineDec"); //<subroutineDec>

            if (subType == "constructor" || subType == "function" || subType == "method")
            {
                _xmlTokens.WriteStartElement("keyword"); //<keyword>
                _xmlTokens.WriteString(subType);

                _xmlTokens.WriteEndElement();  //</keyword>

                _tokenizer.Advance();

                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD) //should either be 'void' or a type
                {
                    _xmlTokens.WriteStartElement("keyword"); //<keyword>
                    _xmlTokens.WriteString(_tokenizer.KeyWord());

                    _xmlTokens.WriteEndElement();  //</keyword>

                    _tokenizer.Advance();

                    if (_tokenizer.TokenType == Enums.Enumerations.TokenType.IDENTIFIER) //subroutineName
                    {
                        _xmlTokens.WriteStartElement("identifier"); //<identifier>
                        _xmlTokens.WriteString(_tokenizer.Identifier());

                        _xmlTokens.WriteEndElement();  //</identifier>

                        _tokenizer.Advance();

                        if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == "(")
                        {
                            _xmlTokens.WriteStartElement("symbol"); //<symbol>
                            _xmlTokens.WriteString(_tokenizer.Symbol()); //(

                            _xmlTokens.WriteEndElement();  //</symbol>

                            _tokenizer.Advance();

                            CompileParameterList(); //this will handle empty parameterLists as well

                            _xmlTokens.WriteStartElement("symbol"); //<symbol>
                            _xmlTokens.WriteString(_tokenizer.Symbol()); //)

                            _xmlTokens.WriteEndElement();  //</symbol>

                            _tokenizer.Advance(); //moving on to the subroutineBody

                            CompileSubroutineBody();

                          
                        }
                        else
                        {
                            //error: expected (
                        }
                    }
                    else
                    {
                        //error: expected subroutineName
                    }
                }
                else
                {
                    //error: expected 'void' or type
                }
            }
            else
            {
                //error: expected (constructor | function | method)
            }
            

            
           
            _xmlTokens.WriteEndElement();  //</subroutineDec>
        }

        public void CompileSubroutineBody()
        {
            _xmlTokens.WriteStartElement("subroutineBody"); //<subroutineBody>

            if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == "{")
            {
                _xmlTokens.WriteStartElement("symbol"); //<symbol>
                _xmlTokens.WriteString(_tokenizer.Symbol()); //{

                _xmlTokens.WriteEndElement();  //</symbol>
                
                //check varDec  
                _tokenizer.Advance();

                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.KEYWORD && _tokenizer.KeyWord() == "var")//we have some variable declarations
                {
                    CompileVarDec();
                }

                //now check to see what kind of statements we have in our subroutine...


                if (_tokenizer.TokenType == Enums.Enumerations.TokenType.SYMBOL && _tokenizer.Symbol() == "}")
                {
                    _xmlTokens.WriteStartElement("symbol"); //<symbol>
                    _xmlTokens.WriteString(_tokenizer.Symbol()); //}

                    _xmlTokens.WriteEndElement();  //</symbol>
                }
                else
                {
                    //error: expected }
                }
            }
            else
            {
                //error: expected {
            }

            _xmlTokens.WriteEndElement(); //</subroutineBody>
        }

        /// <summary>
        /// Compiles a (possibly empty) parameter list, not including the enclosing "()".
        /// </summary>
        public void CompileParameterList()
        {
            _xmlTokens.WriteStartElement("parameterList");//<parameterList>
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
            }

            _xmlTokens.WriteEndElement(); //</parameterList>
        }

        /// <summary>
        /// Compiles a var declaration.
        /// </summary>
        public void CompileVarDec()
        {
            _xmlTokens.WriteStartElement("varDec"); //<varDec>

            _xmlTokens.WriteStartElement("keyword"); //<keyword>
            _xmlTokens.WriteString(_tokenizer.KeyWord()); //var
            _xmlTokens.WriteEndElement();  //</keyword>

            _xmlTokens.WriteStartElement("identifier"); //<identifier>
            _xmlTokens.WriteString(_tokenizer.Identifier()); //type
            _xmlTokens.WriteEndElement();  //</identifier>

            _xmlTokens.WriteStartElement("identifier"); //<identifier>
            _xmlTokens.WriteString(_tokenizer.Identifier()); //varName
            _xmlTokens.WriteEndElement();  //</identifier>

            _xmlTokens.WriteStartElement("symbol"); //<symbol>
            _xmlTokens.WriteString(_tokenizer.Symbol()); //;
            _xmlTokens.WriteEndElement();  //</symbol>

            //may need to refactor later to allow for multiple var decs here...

            _xmlTokens.WriteEndElement(); //</varDec> 
        }

        /// <summary>
        /// Compiles a sequence of state-ments, not including the enclosing "{}".
        /// </summary>
        public void CompileStatements()
        {

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
