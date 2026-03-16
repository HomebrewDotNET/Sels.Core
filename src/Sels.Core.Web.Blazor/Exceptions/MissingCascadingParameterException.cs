using Sels.Core.Extensions.Text;
using System.Runtime.CompilerServices;

namespace Sels.Core.Web.Blazor.Exceptions
{
    /// <summary>
    /// Exception thrown when a page expects a cascading parameter but none is provided.
    /// </summary>
    public class MissingCascadingParameterException : InvalidOperationException
    {
        // Constants
        private const string MessageFormat = "<{0}> requires a cascading parameter of type <{1}>. <{2}> was null";

        /// <inheritdoc cref="MissingCascadingParameterException"/>
        /// <param name="page">The page that is missing the cascading parameter</param>
        /// <param name="parameterType">The type of the cascading parameter</param>
        /// <param name="parameterName">The name of the cascading parameter property</param>
        public MissingCascadingParameterException(object page, Type parameterType, string parameterName) : base(CreateMessage(page, parameterType, parameterName))
        {

        }

        private static string CreateMessage(object page, Type parameterType, string parameterName)
        {
            page.ValidateArgument(nameof(page));
            parameterType.ValidateArgument(nameof(parameterType));
            parameterName.ValidateArgumentNotNullOrWhitespace(nameof(parameterName));

            return MessageFormat.FormatString(page, parameterType, parameterName);
        }

        /// <summary>
        /// Throws a new <see cref="MissingCascadingParameterException"/> if <paramref name="parameter"/> is null.
        /// </summary>
        /// <typeparam name="T">Type of the cascading parameter</typeparam>
        /// <param name="page">The page that is missing the cascading parameter</param>
        /// <param name="parameter">The cascading parameter that was null. Compiler attributes are used to get the property name</param>
        /// <param name="parameterName">Optional property name. Is filled out by the compiler</param>
        public static void ThrowOnNull<T>(object page, T? parameter, [CallerArgumentExpression("parameter")] string parameterName = "")
        {
            page.ValidateArgument(nameof(page));
            parameterName.ValidateArgumentNotNullOrWhitespace(nameof(parameterName));

            if(parameter == null) throw new MissingCascadingParameterException(page, typeof(T), parameterName);
        }

        /// <summary>
        /// Throws a new <see cref="MissingCascadingParameterException"/> if <paramref name="parameter"/> is null.
        /// </summary>
        /// <typeparam name="T">Type of the cascading parameter</typeparam>
        /// <param name="page">The page that is missing the cascading parameter</param>
        /// <param name="parameter">The cascading parameter that was null. Compiler attributes are used to get the property name</param>
        /// <param name="parameterName">Optional property name. Is filled out by the compiler</param>
        public static void ThrowOnNull<T>(object page, Task<T>? parameter, [CallerArgumentExpression("parameter")] string parameterName = "")
        {
            page.ValidateArgument(nameof(page));
            parameterName.ValidateArgumentNotNullOrWhitespace(nameof(parameterName));

            if (parameter == null) throw new MissingCascadingParameterException(page, typeof(T), parameterName);
        }
    }
}
