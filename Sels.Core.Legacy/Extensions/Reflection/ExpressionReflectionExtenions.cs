using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Sels.Core.Extensions.Reflection
{
    /// <summary>
    /// Contains extension methods for working with <see cref="Expression{TDelegate}"/>.
    /// </summary>
    public static class Expression
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

            if(expression.Body != null) property = expression.Body.ExtractPropertyOrDefault();

            return property != null;
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
            expression.ValidateArgument(nameof(expression));

            if(expression.TryExtractProperty(out var property))
            {
                return property;
            }
            else
            {
                throw new ArgumentException($"{variableName} must have a property as body");
            }
        }

        /// <summary>
        /// Extracts <see cref="PropertyInfo"/> from <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="T">Type of the delegate in <paramref name="expression"/></typeparam>
        /// <param name="expression">The expression to check</param>
        /// <returns>The extracted property</returns>
        public static PropertyInfo ExtractProperty<T>(this Expression<T> expression)
        {
            expression.ValidateArgument(nameof(expression));

            return expression.ExtractProperty("Expression");
        }
        /// <summary>
        /// Extracts <see cref="PropertyInfo"/> from <paramref name="expression"/> if the expression points to a property.
        /// </summary>
        /// <param name="expression">The expression to check</param>
        /// <returns>The extracted property or null if <paramref name="expression"/> doesn't point to an expression</returns>
        public static PropertyInfo ExtractPropertyOrDefault(this System.Linq.Expressions.Expression expression)
        {
            expression.ValidateArgument(nameof(expression));

            MemberExpression memberExpression = null;

            if(expression is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression unaryMemberExpression)
            {
                memberExpression = unaryMemberExpression;
            }
            else if (expression is MemberExpression memberSourceExpression)
            {
                memberExpression= memberSourceExpression;
            }

            if(memberExpression != null && memberExpression.Member is PropertyInfo propertyInfo)
            {
                return propertyInfo;
            }

            return null;
        }
        /// <summary>
        /// Extract the property + any nested properties from <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="T">The type of the lambda expression to extract the properties from</typeparam>
        /// <param name="expression">The expression to get the properties from</param>
        /// <returns>All properties extracted from <paramref name="expression"/> in the same order they are defined in <paramref name="expression"/></returns>
        /// <exception cref="NotSupportedException"></exception>
        public static PropertyInfo[] ExtractProperties<T>(this Expression<T> expression)
        {
            expression.ValidateArgument(nameof(expression));
            expression.Body.ValidateArgument(x => x != null, $"{nameof(expression)} body cannot be null");

            List<PropertyInfo> properties = new List<PropertyInfo>();

            System.Linq.Expressions.Expression currentExpression = expression.Body;

            while(currentExpression != null)
            {
                if(currentExpression is MemberExpression memberExpression && memberExpression.Member is PropertyInfo property)
                {
                    properties.Insert(0, property);
                    currentExpression = memberExpression.Expression;
                }
                else if (currentExpression is ConstantExpression constantExpression)
                {
                    // Reached root
                    break;
                }
                else
                {
                    throw new NotSupportedException($"Expected {nameof(MemberExpression)} but got expression of type <{currentExpression.Type}>");
                }
            }

            return properties.ToArray(); 
        }
        #endregion

        #region MethodExtractor
        /// <summary>
        /// Tries to extract the <see cref="MethodInfo"/> if <paramref name="expression"/> points to a method.
        /// </summary>
        /// <typeparam name="T">Type of the delegate in <paramref name="expression"/></typeparam>
        /// <param name="expression">The expression to check</param>
        /// <param name="method">The method if it could be extracted</param>
        /// <returns>True if <paramref name="expression"/> points to a method, otherwise false</returns>
        public static bool TryExtractMethod<T>(this Expression<T> expression, out MethodInfo method)
        {
            method = null;
            expression.ValidateArgument(nameof(expression));

            if (expression.Body != null) method = expression.Body.ExtractMethodOrDefault();

            return method != null;
        }
        /// <summary>
        /// Extracts <see cref="MethodInfo"/> from <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="T">Type of the delegate in <paramref name="expression"/></typeparam>
        /// <param name="expression">The expression to check</param>
        /// <param name="variableName">The name of the expression variable</param>
        /// <returns>The extracted method</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="expression"/> doesn't point to a method</exception>
        public static MethodInfo ExtractMethod<T>(this Expression<T> expression, string variableName)
        {
            expression.ValidateArgument(nameof(expression));

            if (expression.TryExtractMethod(out var method))
            {
                return method;
            }
            else
            {
                throw new ArgumentException($"{variableName} must have a method as body");
            }
        }

        /// <summary>
        /// Extracts <see cref="MethodInfo"/> from <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="T">Type of the delegate in <paramref name="expression"/></typeparam>
        /// <param name="expression">The expression to check</param>
        /// <returns>The extracted method</returns>
        public static MethodInfo ExtractMethod<T>(this Expression<T> expression)
        {
            expression.ValidateArgument(nameof(expression));

            return expression.ExtractMethod("Expression");
        }
        /// <summary>
        /// Extracts <see cref="MethodInfo"/> from <paramref name="expression"/> if the expression points to a method.
        /// </summary>
        /// <param name="expression">The expression to check</param>
        /// <returns>The extracted method or null if <paramref name="expression"/> doesn't point to an expression</returns>
        public static MethodInfo ExtractMethodOrDefault(this System.Linq.Expressions.Expression expression)
        {
            expression.ValidateArgument(nameof(expression));

            if (expression is MethodCallExpression callExpression)
            {
                return callExpression.Method;
            }

            return null;
        }
        #endregion
    }
}
