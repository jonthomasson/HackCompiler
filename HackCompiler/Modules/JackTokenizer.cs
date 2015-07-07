using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HackCompiler.Enums;
using System.Text.RegularExpressions;

namespace HackCompiler.Modules
{
    /// <summary>
    /// Removes all comments and white space from the input stream and breaks it into Jack-language tokens, as specified by the Jack grammar
    /// </summary>
    public class JackTokenizer
    {
        private System.IO.StreamReader _sr;
        public string CurrentToken { get; set; }
        public bool HasMoreTokens { get; set; }
        public List<TokenizedObject> _tokens;
        public Enumerations.TokenType TokenType { get; set; }
        public string[] Symbols = { "(", ")", "{", "}", "[", "]", ".", ",", ";", "+", "-", "*", "/", "&", "|", "<", ">", "=", "~" };
        public string[] Keywords = { "class", "constructor", "function", "method", "field", "static", "var", "int", "char", "boolean", "void", "true", "false", "null", "this", "let", "do", "if", "else", "while", "return" };
        /// <summary>
        /// Constructor: opens the input file/stream and gets ready to tokenize it.
        /// </summary>
        /// <param name="inputFile">input file stream</param>
        public JackTokenizer(string inputFile)
        {
            //open the file stream 
            //_sr = new System.IO.StreamReader(inputFile);
            _tokens = new List<TokenizedObject>();
            //read through file line by line first and put individual tokens into a dictionary
            ParseTokens(inputFile);

            //HasMoreTokens = !_sr.EndOfStream;
        }

        

        public void ParseTokens(string inputFile)
        {
            var sr = new System.IO.StreamReader(inputFile);
            var line = "";
            //int buff;
            var buff = "";
            var isStringConstant = false;

            while ((line = sr.ReadLine()) != null)
            {
                if (!line.StartsWith("/") && line != "")//skip comments and blank lines
                {
                    foreach (var part in line)
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

        public void ProcessToken(string token)
        {
            int buff;
            if (Symbols.Contains(token))
            {
                _tokens.Add(new TokenizedObject { Token = token, Type = Enumerations.TokenType.SYMBOL });
            }
            else if (Keywords.Contains(token))
            {
                _tokens.Add(new TokenizedObject { Token = token, Type = Enumerations.TokenType.KEYWORD });
            }
            else if (int.TryParse(token, out buff))
            {
                _tokens.Add(new TokenizedObject { Token = token, Type = Enumerations.TokenType.INT_CONST });
            }
            else if (token.StartsWith("\""))
            {
                _tokens.Add(new TokenizedObject { Token = token.Replace("\"", ""), Type = Enumerations.TokenType.STRING_CONST });
            }
            else
            {
                _tokens.Add(new TokenizedObject { Token = token, Type = Enumerations.TokenType.IDENTIFIER });
            }
        }


        /// <summary>
        /// Gets the next token from the input and makes it the current token. 
        /// This method should only be called if hasMoreTokens() is true. 
        /// Initially there is no current token.
        /// </summary>
        public void Advance()
        {
            var tokenBuffer = _sr.ReadLine();
            //CurrentToken = 

            //handle commented lines  and white space
            if (string.IsNullOrWhiteSpace(tokenBuffer))
            {
                tokenBuffer = "";
            }
            bool isComment = tokenBuffer.Contains("//") || tokenBuffer.Contains("/**");
            while (isComment || string.IsNullOrWhiteSpace(tokenBuffer))
            {
                tokenBuffer = _sr.ReadLine();
                if (string.IsNullOrWhiteSpace(tokenBuffer))
                {
                    tokenBuffer = "";
                }
                isComment = tokenBuffer.Contains("//") || tokenBuffer.Contains("/**");
            }

            //determine what type of token we have


            HasMoreTokens = !_sr.EndOfStream;

        }


        /// <summary>
        /// Returns the keyword which is the current token. Should be called only when tokenType() is KEYWORD
        /// </summary>
        /// <returns></returns>
        public Enumerations.Keyword KeyWord()
        {
            return Enumerations.Keyword.BOOLEAN;
        }

        /// <summary>
        /// Returns the character which is the current token. Should be called only when tokenType() is SYMBOL
        /// </summary>
        /// <returns></returns>
        public string Symbol()
        {
            return "";
        }

        /// <summary>
        /// Returns the identifier which is the current token. Should be called only when tokenType() is IDENTIFIER
        /// </summary>
        /// <returns></returns>
        public string Identifier()
        {
            return "";
        }

        /// <summary>
        /// Returns the integer value of the current token. Should be called only when tokenType() is INT_CONST
        /// </summary>
        /// <returns></returns>
        public int IntVal()
        {
            return 0;
        }

        /// <summary>
        /// Returns the string value of the current token, without the double quotes. Should be called only when tokenType() is STRING_CONST
        /// </summary>
        /// <returns></returns>
        public string StringVal()
        {
            return "";
        }

        public void GarbageCollection()
        {
            _sr.Close();
        }

    }
    public class TokenizedObject
    {
        public Enumerations.TokenType Type { get; set; }
        public dynamic Token { get; set; }

    }
}
