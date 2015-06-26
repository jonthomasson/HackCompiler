using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HackCompiler.Modules
{
    /// <summary>
    /// The analyzer program operates on a given source, where source is either a file name
    /// of the form xxx.jack or a directory name containing one or more such files. For
    /// each source xxx.jack file, the analyzer goes through the following logic:
    /// 
    /// 1. Create a JackTokenizer from the xxx.jack input file.
    /// 2. Create an output file called xxx.xml and prepare it for writing.
    /// 3. Use the CompilationEngine to compile the input JackTokenizer into the output file.
    /// </summary>
    public class JackAnalyzer
    {
    }
}
