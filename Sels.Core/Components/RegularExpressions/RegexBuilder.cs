using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Sels.Core.RegularExpressions
{
    /// <summary>
    /// Represents a instance used to create regular expressions.
    /// </summary>
    public class RegexBuilder 
    {
        // Fields
        private StringBuilder _builder = new StringBuilder();

        /// <summary>
        /// Contains constant values for certain regular expressions.
        /// </summary>
        public static class Expressions
        {
            /// <summary>
            /// Matches everything except linebreaks.
            /// </summary>
            public const string AnyWord = @"(.+)";
            /// <summary>
            /// Matches everything exception linebreaks. Can also match empty string.
            /// </summary>
            public const string AnyWordOptional = @"(.*)";
            /// <summary>
            /// Matches everything.
            /// </summary>
            public const string Any = @"([\s\S]+)";
            /// <summary>
            /// Matches everything. Can also match empty string.
            /// </summary>
            public const string AnyOptional = @"([\s\S]*)";
        }

        /// <summary>
        /// Contains formats for certain regular expressions.
        /// </summary>
        public class Formats
        {
            /// <summary>
            /// Matches a group after the main expression without including it in the result.
            /// </summary>
            public const string PositiveLookAhead = "(?={0})";
            /// <summary>
            /// Specifies a group that can not match after the main expression without including it in the result.
            /// </summary>
            public const string NegativeLookAhead = "(?!{0})";
            /// <summary>
            /// Matches a group before the main expression without including it in the result.
            /// </summary>
            public const string PositiveLookBehind = "(?<={0})";
            /// <summary>
            /// Specifies a group that can not match before the main expression without including it in the result.
            /// </summary>
            public const string NegativeLookBehind = "(?<!{0})";
        }

        #region Append
        /// <summary>
        /// Appends <paramref name="value"/> to the internal regex.
        /// </summary>
        /// <param name="value">The value to append</param>
        /// <param name="escape">If <paramref name="value"/> should be escaped</param>
        /// <returns>Current builder for method chaining</returns>
        public RegexBuilder Append(string value, bool escape = true)
        {
            value.ValidateArgument(nameof(value));
            value = escape ? Regex.Escape(value) : value;

            _builder.Append(value);
            return this;
        }

        /// <summary>
        /// Appends <see cref="object.ToString()"/> value from <paramref name="value"/> to the internal regex.
        /// </summary>
        /// <param name="value">The value to append</param>
        /// <param name="escape">If <paramref name="value"/> should be escaped</param>
        /// <returns>Current builder for method chaining</returns>
        public RegexBuilder Append(object value, bool escape = true)
        {
            value.ValidateArgument(nameof(value));
            
            return Append(value.ToString(), escape);
        }
        #endregion

        #region Insert
        /// <summary>
        /// Inserts <paramref name="value"/> at character index <paramref name="index"/> of the internal regex.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins</param>
        /// <param name="value">The value to insert</param>
        /// <param name="escape">If <paramref name="value"/> should be escaped</param>
        /// <returns>Current builder for method chaining</returns>
        public RegexBuilder Insert(int index, string value, bool escape = false)
        {
            index.ValidateArgumentLargerOrEqual(nameof(index), 0);
            value.ValidateArgument(nameof(value));
            value = escape ? Regex.Escape(value) : value;

            _builder.Insert(index, value);
            return this;
        }

        /// <summary>
        /// Inserts <see cref="object.ToString()"/> value from <paramref name="value"/> at character index <paramref name="index"/> of the internal regex.
        /// </summary>
        /// <param name="index">The position in this instance where insertion begins</param>
        /// <param name="value">The value to insert</param>
        /// <param name="escape">If <paramref name="value"/> should be escaped</param>
        /// <returns>Current builder for method chaining</returns>
        public RegexBuilder Insert(int index, object value, bool escape = false)
        {
            index.ValidateArgumentLargerOrEqual(nameof(index), 0);
            value.ValidateArgument(nameof(value));
            
            return Insert(index, value.ToString(), escape);
        }
        #endregion

        #region Replace
        /// <summary>
        /// Replaces <paramref name="oldValue"/> with <paramref name="newValue"/> in the internal regex.
        /// </summary>
        /// <param name="oldValue">The value to replace</param>
        /// <param name="newValue">The value to replace with</param>
        /// <param name="escape">If <paramref name="newValue"/> should be escaped</param>
        /// <returns>Current builder for method chaining</returns>
        public RegexBuilder Replace(string oldValue, string newValue, bool escape = false)
        {
            oldValue.ValidateArgument(nameof(oldValue));
            newValue.ValidateArgument(nameof(newValue));

            newValue = escape ? Regex.Escape(newValue) : newValue;

            _builder.Replace(oldValue, newValue);
            return this;
        }

        /// <summary>
        /// Replaces <see cref="object.ToString()"/> value from <paramref name="oldValue"/> with <see cref="object.ToString()"/> value from <paramref name="newValue"/> in the internal regex.
        /// </summary>
        /// <param name="oldValue">The value to replace</param>
        /// <param name="newValue">The value to replace with</param>
        /// <param name="escape">If <paramref name="newValue"/> should be escaped</param>
        /// <returns>Current builder for method chaining</returns>
        public RegexBuilder Replace(object oldValue, object newValue, bool escape = false)
        {
            oldValue.ValidateArgument(nameof(oldValue));
            newValue.ValidateArgument(nameof(newValue));

           return Replace(oldValue.ToString(), newValue.ToString(), escape);
        }
        #endregion

        #region Expressions
        /// <summary>
        /// Appends an expression that matches any combination excluding linebreaks.
        /// </summary>
        /// <param name="isOptional">Set to false if at least 1 characters needs to be matched</param>
        /// <returns>Current builder for method chaining</returns>
        public RegexBuilder MatchAnyWord(bool isOptional = false)
        {
            var expression = isOptional ? Expressions.AnyWordOptional : Expressions.AnyWord;

            _builder.Append(expression);
            return this;
        }
        /// <summary>
        /// Appends an expression that matches any combination.
        /// </summary>
        /// <param name="isOptional">Set to false if at least 1 characters needs to be matched</param>
        /// <returns>Current builder for method chaining</returns>
        public RegexBuilder MatchAny(bool isOptional = false)
        {
            var expression = isOptional ? Expressions.AnyOptional : Expressions.Any;

            _builder.Append(expression);
            return this;
        }
        #endregion

        #region Formats
        /// <summary>
        /// Appends a lookahead with <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to add to the lookahead</param>
        /// <param name="isMatch">Set to true for a positive lookahead and false for a negative lookahead</param>
        /// <param name="escape">If <paramref name="value"/> should be escaped</param>
        /// <returns>Current builder for method chaining</returns>
        public RegexBuilder LookAhead(string value, bool isMatch = true, bool escape = false)
        {
            value.ValidateArgument(nameof(value));

            var format = isMatch ? Formats.PositiveLookAhead : Formats.NegativeLookAhead;
            value = escape ? Regex.Escape(value) : value;

            _builder.Append(format.FormatString(value));
            return this;
        }
        /// <summary>
        /// Appends a lookbehind with <paramref name="value"/>.
        /// </summary>
        /// <param name="value">The value to add to the lookbehind</param>
        /// <param name="isMatch">Set to true for a positive lookbehind and false for a negative lookbehind</param>
        /// <param name="escape">If <paramref name="value"/> should be escaped</param>
        /// <returns>Current builder for method chaining</returns>
        public RegexBuilder LookBehind(string value, bool isMatch = true, bool escape = false)
        {
            value.ValidateArgument(nameof(value));

            var format = isMatch ? Formats.PositiveLookBehind : Formats.NegativeLookBehind;
            value = escape ? Regex.Escape(value) : value;

            _builder.Append(format.FormatString(value));
            return this;
        }
        #endregion

        /// <summary>
        /// Builds the regex of the current builder.
        /// </summary>
        /// <returns>The regex string representing the current instance</returns>
        public override string ToString()
        {
            return _builder.ToString();
        }
        /// <summary>
        /// Builds the regex of the current builder.
        /// </summary>
        /// <returns>The regex representing the current instance</returns>
        public Regex ToRegex()
        {
            return new Regex(this.ToString());
        }
    }
}
