using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HackCompiler.Enums;
using System.Text.RegularExpressions;
using System.Diagnostics;


namespace HackCompiler.Modules
{
    /// <summary>
    /// Removes all comments and white space from the input stream and breaks it into Jack-language tokens, as specified by the Jack grammar
    /// </summary>
    public class JackTokenizer
    {

        public string CurrentToken { get; set; }
        public bool HasMoreTokens { get; set; }
        private List<TokenizedObject> _tokens;
        public List<TokenizedObject> Tokens
        {
            get { return _tokens; }
        }
        public bool HasErrors { get; set; }
        private int _currentTokenIdx;
        private int _lineNo = 0;
        private int _charNo = 0;
        private TokenizedObject _currentToken;
        public TokenizedObject NextToken { get; set; }
        public Enumerations.TokenType TokenType { get; set; }
        public string[] Symbols = { "(", ")", "{", "}", "[", "]", ".", ",", ";", "+", "-", "*", "/", "&", "|", "<", ">", "=", "~" };
        public string[] Keywords = { "class", "constructor", "function", "method", "field", "static", "var", "int", "char", "boolean", "void", "true", "false", "null", "this", "let", "do", "if", "else", "while", "return" };
        /// <summary>
        /// Constructor: opens the input file/stream and gets ready to tokenize it.
        /// </summary>
        /// <param name="inputFile">input file stream</param>
        public JackTokenizer(string inputFile)
        {
            _tokens = new List<TokenizedObject>();
            //read through file line by line first and put individual tokens into a dictionary
            ParseTokens(inputFile);
            _lineNo = 0;
            _currentTokenIdx = 0;
            _charNo = 0;
            HasMoreTokens = _tokens.Count > 0 ? true : false;
            HasErrors = false;
        }



        public void ParseTokens(string inputFile)
        {
            var sr = new System.IO.StreamReader(inputFile);
            var line = "";

            var buff = "";
            var isStringConstant = false;
            var checkComment = 0;
            

            while ((line = sr.ReadLine()) != null)
            {
                
                _lineNo++; //increment our line number
                _charNo = 0; //set charno = 0 for current line
                checkComment = 0;

                if (!line.TrimStart().StartsWith("/") && !string.IsNullOrWhiteSpace(line) && !line.TrimStart().StartsWith("*"))//skip comments and blank lines
                {
                    foreach (var part in line)
                    {
                        _charNo++;

                        if (part.ToString() == "/")
                        {
                            if (checkComment == _charNo - 1)
                            {
                                //the rest of the line is a comment, so ignore it
                                _tokens.Remove(_tokens.Last()); //remove the last token, since it ended up just being a comment
                                break;

                            }
                            else
                            {
                                checkComment = _charNo;
                                ProcessToken(part.ToString());
                            }
                        }
                        else
                        {
                            if ((string.IsNullOrWhiteSpace(part.ToString()) || Symbols.Contains(part.ToString())) && !isStringConstant)
                            {
                                //if it is a symbol or space, then need to write out our buffer to a tokenizedObject and clear the buffer
                                if (buff.Length > 0 && !string.IsNullOrWhiteSpace(buff))
                                {
                                    ProcessToken(buff); //flush our buffer
                                }
                                if (Symbols.Contains(part.ToString()))
                                {
                                    ProcessToken(part.ToString());
                                }
                                buff = ""; //clear buffer to start reading next token
                            }
                            else
                            {
                                if (part.ToString().Contains("\""))
                                {
                                    isStringConstant = !isStringConstant;
                                }
                                //write our part to a temp buffer
                                buff += part;
                            }
                        }

                      
                    }
                }


            }

            sr.Close();

        }

        public void RecordError(string error)
        {
            if (string.IsNullOrEmpty(_currentToken.Error)) //we want to capture the first error since that'll be the one with the most detail.
            {
                _currentToken.Error = error;

                HasErrors = true;
            }

            _currentToken.StackTrace += "@" + GetStackTrace(error) + Environment.NewLine;

        }

        /// <summary>
        /// method for getting the current stack trace for error handling/recording
        /// </summary>
        public static string GetStackTrace(string message)
        {
            StackTrace stackTrace = new StackTrace(true);
            StackFrame sf = stackTrace.GetFrame(2); //using index 2 should give us the calling function, not the current one.
            var retMessage = 
                sf.GetMethod().Name + " "
                + sf.GetFileName() + ":"
                + sf.GetFileLineNumber() + Environment.NewLine
                + "Error Message: " + message + Environment.NewLine;

            return retMessage;
        }

        public void ProcessToken(string token)
        {
            int buff;
            if (Symbols.Contains(token))
            {
                _tokens.Add(new TokenizedObject { Token = token, Type = Enumerations.TokenType.SYMBOL, CharNo = _charNo, LineNo = _lineNo });
            }
            else if (Keywords.Contains(token))
            {
                _tokens.Add(new TokenizedObject { Token = token, Type = Enumerations.TokenType.KEYWORD, CharNo = _charNo, LineNo = _lineNo });
            }
            else if (int.TryParse(token, out buff))
            {
                _tokens.Add(new TokenizedObject { Token = token, Type = Enumerations.TokenType.INT_CONST, CharNo = _charNo, LineNo = _lineNo });
            }
            else if (token.StartsWith("\""))
            {
                _tokens.Add(new TokenizedObject { Token = token.Replace("\"", ""), Type = Enumerations.TokenType.STRING_CONST, CharNo = _charNo, LineNo = _lineNo });
            }
            else
            {
                _tokens.Add(new TokenizedObject { Token = token, Type = Enumerations.TokenType.IDENTIFIER, CharNo = _charNo, LineNo = _lineNo });
            }
        }


        /// <summary>
        /// Gets the next token from the input and makes it the current token. 
        /// This method should only be called if hasMoreTokens() is true. 
        /// Initially there is no current token.
        /// </summary>
        public void Advance()
        {
            // var tokenBuffer = _tokens[_currentTokenIdx];

            HasMoreTokens = _tokens.Count > _currentTokenIdx + 1 ? true : false;

            if (HasMoreTokens)
            {
                _currentToken = _tokens[_currentTokenIdx];
                TokenType = _currentToken.Type;
                CurrentToken = _currentToken.Token;

                _currentTokenIdx++;


                NextToken = _tokens[_currentTokenIdx];
            }
        }


        /// <summary>
        /// Returns the keyword which is the current token. Should be called only when tokenType() is KEYWORD
        /// </summary>
        /// <returns></returns>
        public string KeyWord()
        {
            return (string)_currentToken.Token;
        }

        /// <summary>
        /// Returns the character which is the current token. Should be called only when tokenType() is SYMBOL
        /// </summary>
        /// <returns></returns>
        public string Symbol()
        {
            return (string)_currentToken.Token;
        }

        /// <summary>
        /// Returns the identifier which is the current token. Should be called only when tokenType() is IDENTIFIER
        /// </summary>
        /// <returns></returns>
        public string Identifier()
        {
            return (string)_currentToken.Token;
        }

        /// <summary>
        /// Returns the integer value of the current token. Should be called only when tokenType() is INT_CONST
        /// </summary>
        /// <returns></returns>
        public int IntVal()
        {
            return int.Parse(_currentToken.Token);
        }

        /// <summary>
        /// Returns the string value of the current token, without the double quotes. Should be called only when tokenType() is STRING_CONST
        /// </summary>
        /// <returns></returns>
        public string StringVal()
        {
            return (string)_currentToken.Token;
        }



    }
    public class TokenizedObject
    {
        public Enumerations.TokenType Type { get; set; }
        public dynamic Token { get; set; }
        public int LineNo { get; set; }
        public int CharNo { get; set; }
        public string Error { get; set; }
        public string StackTrace { get; set; }

    }
}
