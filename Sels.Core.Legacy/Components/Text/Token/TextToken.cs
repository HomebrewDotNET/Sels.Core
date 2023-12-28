using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sels.Core.Text.Token
{
    /// <summary>
    /// Represents a token parsed from a template that consists of text.
    /// </summary>
    public class TextToken
    {
        /// <summary>
        /// The text that this token consists of.
        /// </summary>
        public string Text { get; }

        /// <inheritdoc cref="TextToken"/>
        /// <param name="text"><inheritdoc cref="Text"/></param>
        public TextToken(IEnumerable<char> text)
        {
            Text = new string(text.ValidateArgument(nameof(text)).ToArray());
        }
    }
}
