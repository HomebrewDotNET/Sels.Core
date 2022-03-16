using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Cli.ArgumentParsing
{
    /// <summary>
    /// A null instance without any properties to use with a <see cref="IArgumentParser{T}"/> when no properties need to be parsed to.
    /// </summary>
    public class NullArguments
    {
        private NullArguments()
        {

        }
        /// <summary>
        /// Static instance to use.
        /// </summary>
        public static NullArguments Instance = new NullArguments();
    }
}
