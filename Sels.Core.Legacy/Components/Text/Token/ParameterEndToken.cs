using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Text.Token
{
    /// <summary>
    /// Represents a token parsed from a template that represents the end of a named parameter.
    /// </summary>
    public class ParameterEndToken
    {
        /// <summary>
        /// The value used to denote the end of a parameter.
        /// </summary>
        public string Value { get; }

        /// <inheritdoc cref="ParameterStartToken"/>
        /// <param name="value"><inheritdoc cref="Value"/></param>
        public ParameterEndToken(string value)
        {
            Value = value.ValidateArgument(nameof(value));
        }
    }
}
