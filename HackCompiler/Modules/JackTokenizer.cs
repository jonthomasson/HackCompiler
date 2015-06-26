using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HackCompiler.Enums;

namespace HackCompiler.Modules
{
    /// <summary>
    /// Removes all comments and white space from the input stream and breaks it into Jack-language tokens, as specified by the Jack grammar
    /// </summary>
    public class JackTokenizer
    {
        /// <summary>
        /// Constructor: opens the input file/stream and gets ready to tokenize it.
        /// </summary>
        /// <param name="inputFile">input file stream</param>
        public JackTokenizer(string inputFile)
        {

        }

        /// <summary>
        /// do we have more tokens in the input?
        /// </summary>
        /// <returns></returns>
        public bool hasMoreTokens()
        {
            return true;
        }

        /// <summary>
        /// Gets the next token from the input and makes it the current token. 
        /// This method should only be called if hasMoreTokens() is true. 
        /// Initially there is no current token.
        /// </summary>
        public void advance()
        {

        }

        /// <summary>
        /// Returns the type of the current token.
        /// </summary>
        /// <returns></returns>
        public Enumerations.TokenType tokenType()
        {
            return Enumerations.TokenType.IDENTIFIER;
        }

        /// <summary>
        /// Returns the keyword which is the current token. Should be called only when tokenType() is KEYWORD
        /// </summary>
        /// <returns></returns>
        public Enumerations.Keyword keyWord()
        {
            return Enumerations.Keyword.BOOLEAN;
        }

        /// <summary>
        /// Returns the character which is the current token. Should be called only when tokenType() is SYMBOL
        /// </summary>
        /// <returns></returns>
        public string symbol()
        {
            return "";
        }

        /// <summary>
        /// Returns the identifier which is the current token. Should be called only when tokenType() is IDENTIFIER
        /// </summary>
        /// <returns></returns>
        public string identifier()
        {
            return "";
        }

        /// <summary>
        /// Returns the integer value of the current token. Should be called only when tokenType() is INT_CONST
        /// </summary>
        /// <returns></returns>
        public int intVal()
        {
            return 0;
        }

        /// <summary>
        /// Returns the string value of the current token, without the double quotes. Should be called only when tokenType() is STRING_CONST
        /// </summary>
        /// <returns></returns>
        public string stringVal()
        {
            return "";
        }

    }
}
