using Sels.Core.Extensions;
using Sels.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Sels.Core.Extensions.Reflection
{
    public static class ExpressionReflectionExtenions
    {
        #region PropertyExtractor
        public static bool TryExtractProperty<T>(this Expression<T> expression, out PropertyInfo property)
        {
            property = null;
            expression.ValidateVariable(nameof(expression));

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
