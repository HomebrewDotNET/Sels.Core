using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.SQL.QueryBuilder.Builder.Compilation
{
    /// <summary>
    /// Exposes extra options for query compilers.
    /// </summary>
    public interface ICompilerOptions
    {
        /// <summary>
        /// Defines a delegate for converting an object that represents a dataset into it's sql equivalent. (e.g. Type MyNamespace.MyObject converted to M)
        /// </summary>
        /// <param name="converterDelegate">The delegate to use for the conversion</param>
        /// <returns>Current options for method chaining</returns>
        ICompilerOptions SetDataSetConverter(Func<object, string> converterDelegate);
    }
}
