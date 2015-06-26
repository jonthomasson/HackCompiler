using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackCompiler.Modules
{
    /// <summary>
    /// Effects the actual compilation output.
    /// </summary>
    public class CompilationEngine
    {
        /// <summary>
        /// Constructor: creates a new compilation engine with the given input and output. The next routine called must be compileClass().
        /// </summary>
        /// <param name="inputFile"></param>
        /// <param name="outputFile"></param>
        public CompilationEngine(string inputFile, string outputFile)
        {

        }

        /// <summary>
        /// Compiles a complete class.
        /// </summary>
        public void CompileClass()
        {

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

        }

        /// <summary>
        /// Compiles a (possibly empty) parameter list, not including the enclosing "()".
        /// </summary>
        public void CompileParameterList()
        {

        }

        /// <summary>
        /// Compiles a var declaration.
        /// </summary>
        public void CompileVarDec()
        {

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
