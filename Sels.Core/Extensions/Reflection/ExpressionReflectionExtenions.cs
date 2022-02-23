using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sels.Core.Extensions.Reflection
{
    /// <summary>
    /// Contains extension methods for working with <see cref="Expression{TDelegate}"/>.
    /// </summary>
    public static class ExpressionReflectionExtenions
    {
        #region PropertyExtractor
        /// <summary>
        /// Tries to extract the <see cref="PropertyInfo"/> if <paramref name="expression"/> points to a property.
        /// </summary>
        /// <typeparam name="T">Type of the delegate in <paramref name="expression"/></typeparam>
        /// <param name="expression">The expression to check</param>
        /// <param name="property">The property if it could be extracted</param>
        /// <returns>True if <paramref name="expression"/> points to a property, otherwise false</returns>
        public static bool TryExtractProperty<T>(this Expression<T> expression, out PropertyInfo property)
        {
            property = null;
            expression.ValidateArgument(nameof(expression));

            MemberExpression memberExpression = null;

            if(expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression unaryMemberExpression)
            {
                memberExpression = unaryMemberExpression;
            }
            else if (expression.Body is MemberExpression bodyMemberExpression)
            {
                memberExpression = bodyMemberExpression;
            }

            if (memberExpression.HasValue())
            {
                property = (PropertyInfo)memberExpression.Member;
                return true;
            }

            return false;
        }
        /// <summary>
        /// Extracts <see cref="PropertyInfo"/> from <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="T">Type of the delegate in <paramref name="expression"/></typeparam>
        /// <param name="expression">The expression to check</param>
        /// <param name="variableName">The name of the expression variable</param>
        /// <returns>The extracted property</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="expression"/> doesn't point to a property</exception>
        public static PropertyInfo ExtractProperty<T>(this Expression<T> expression, string variableName)
        {
            if(expression.TryExtractProperty(out var property))
            {
                return property;
            }
            else
            {
                throw new ArgumentException($"{variableName} must have a property as body");
            }
        }
        #endregion
    }
}
