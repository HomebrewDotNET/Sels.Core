using Sels.SQL.QueryBuilder.Builder.Expressions;
using System;
using System.Text;
using Sels.Core.Extensions;

namespace Sels.SQL.QueryBuilder.Builder.Statement
{
    /// <summary>
    /// Builder that adds comparison operators.
    /// </summary>
    /// <typeparam name="TEntity">The main entity to build the query for</typeparam>
    /// <typeparam name="TReturn">The type to return for the fluent syntax</typeparam>
    public interface IComparisonExpressionBuilder<TEntity, out TReturn>
    {
        #region Expression
        /// <summary>
        /// Compares 2 expressions using the operator defined in <paramref name="sqlExpression"/>.
        /// </summary>
        /// <param name="sqlExpression">The sql expression containing the operator</param>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn CompareTo(IExpression sqlExpression);
        /// <summary>
        /// Compares 2 expressions using the operator defined in <paramref name="sqlExpression"/>.
        /// </summary>
        /// <param name="sqlExpression">String containing the sql operator</param>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn CompareTo(string sqlExpression) => CompareTo(new RawExpression(sqlExpression.ValidateArgumentNotNullOrEmpty(nameof(sqlExpression))));
        /// <summary>
        /// Compares 2 expressions using the operator defined in <paramref name="sqlExpression"/>.
        /// </summary>
        /// <param name="sqlExpression">Delegate that adds the sql operator to compare to the provided string builder</param>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn CompareTo(Action<StringBuilder, ExpressionCompileOptions> sqlExpression) => CompareTo(new DelegateExpression(sqlExpression.ValidateArgument(nameof(sqlExpression))));
        #endregion

        #region Operators
        /// <summary>
        /// The expressions should be equal.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn EqualTo => CompareTo(new EnumExpression<Operators>(Operators.Equal));
        /// <summary>
        /// The expressions should not be equal.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn NotEqualTo => CompareTo(new EnumExpression<Operators>(Operators.NotEqual));
        /// <summary>
        /// First expression should be greater than second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn GreaterThan => CompareTo(new EnumExpression<Operators>(Operators.Greater));
        /// <summary>
        /// First expression should be lesser than second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn LesserThan => CompareTo(new EnumExpression<Operators>(Operators.Less));
        /// <summary>
        /// First expression should be greater or equal to second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn GreaterOrEqualTo => CompareTo(new EnumExpression<Operators>(Operators.GreaterOrEqual));
        /// <summary>
        /// First expression should be lesser or equal to second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn LesserOrEqualTo => CompareTo(new EnumExpression<Operators>(Operators.LessOrEqual));
        /// <summary>
        /// First expression should be like the pattern defined in the second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn Like => CompareTo(new EnumExpression<Operators>(Operators.Like));
        /// <summary>
        /// First expression should not be like the pattern defined in the second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn NotLike => CompareTo(new EnumExpression<Operators>(Operators.NotLike));
        /// <summary>
        /// First expression should exist in a list of values defined by the second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn In => CompareTo(new EnumExpression<Operators>(Operators.In));
        /// <summary>
        /// First expression should not exist in a list of values defined by the second expression.
        /// </summary>
        /// <returns>Builder for selecting what to compare to the first expression</returns>
        TReturn NotIn => CompareTo(new EnumExpression<Operators>(Operators.NotIn));
        #endregion       
    }
}
