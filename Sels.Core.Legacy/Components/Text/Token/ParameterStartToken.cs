using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sels.Core.Text.Token
{
    /// <summary>
    /// Represents a token parsed from a template that represents the start of a named parameter.
    /// </summary>
    public class ParameterStartToken
    {
        /// <summary>
        /// The value used to denote the start of a parameter.
        /// </summary>
        public string Value { get; }

        /// <inheritdoc cref="ParameterStartToken"/>
        /// <param name="value"><inheritdoc cref="Value"/></param>
        public ParameterStartToken(string value)
        {
            Value = value.ValidateArgument(nameof(value));
        }
    }
}
