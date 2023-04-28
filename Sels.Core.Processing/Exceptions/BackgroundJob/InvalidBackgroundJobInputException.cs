using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Processing.BackgroundJob
{
    /// <summary>
    /// Throw when input provided to a <see cref="IBackgroundJob"/> is invalid.
    /// </summary>
    public class InvalidBackgroundJobInputException : Exception
    {
        /// <inheritdoc cref="InvalidBackgroundJobInputException"/>
        /// <param name="message">message containing the details why the input is invalid</param>
        public InvalidBackgroundJobInputException(string message) : base(Guard.IsNotNullOrWhitespace(message))
        {
            
        }
    }
}
