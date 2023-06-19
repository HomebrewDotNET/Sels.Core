using Dapper;
using Sels.Core.Extensions;
using Sels.SQL.QueryBuilder.Builder.Statement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Sels.Core.Extensions.Reflection;

namespace Sels.Core.Data.SQL.SearchCriteria
{
    /// <summary>
    /// Converts an object of type <typeparamref name="TSearchCriteria"/> to a SQL where clause.
    /// </summary>
    /// <typeparam name="T">The main entity to create the query for</typeparam>
    /// <typeparam name="TSearchCriteria">Type of the search criteria</typeparam>
    public interface ISearchCriteriaConverter<T, TSearchCriteria>
    {
        /// <summary>
        /// Creates a SQL where clause using <paramref name="searchCriteria"/> and <paramref name="builder"/>.
        /// </summary>
        /// <param name="builder">The builder that will be used to create the conditions</param>
        /// <param name="searchCriteria">The search criteria that will be converted</param>
        /// <param name="parameters">Optional parameter bag that can be provided. Implicit conditions that use parameters will automatically add the values to the bag</param>
        /// <returns>The final builder after the creating the conditions or null if no conditions were created</returns>
        IChainedBuilder<T, IStatementConditionExpressionBuilder<T>> Build(IStatementConditionExpressionBuilder<T> builder, TSearchCriteria searchCriteria, DynamicParameters? parameters = null);
    }

    /// <summary>
    /// Builder for configuring how search criteria of type <typeparamref name="T"/> is converted to a SQL where clause.
    /// </summary>
    /// <typeparam name="T">The main entity to create the query for</typeparam>
    /// <typeparam name="TSearchCriteria">Type of the search criteria</typeparam>
    public interface ISearchCriteriaConverterBuilder<T, TSearchCriteria>
    {
        /// <summary>
        /// Returns a builder on how to convert the property selected by <paramref name="property"/> to a SQL condition.
        /// </summary>
        /// <typeparam name="TValue">Value of the property</typeparam>
        /// <param name="property">Expression pointing to the property to use</param>
        /// <param name="allowNull">If the builder will also be used if the property value is null, nulls are ignored by default</param>
        /// <returns>A builder for configuring how to convert the property selected by <paramref name="property"/> to a SQL condition</returns>
        ISearchCriteriaConverterPropertyBuilder<T, TSearchCriteria, TValue> With<TValue>(Expression<Func<TSearchCriteria, TValue>> property, bool allowNull = false);
        /// <summary>
        /// Defines the names of the properties not to create search criteria for.
        /// </summary>
        /// <param name="name">The name of the property to exclude</param>
        /// <param name="additionalNames">Optional additional names of properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        ISearchCriteriaConverterBuilder<T, TSearchCriteria> Excluding(string name, params string[] additionalNames);
        /// <summary>
        /// Defines the properties not to create search criteria for.
        /// </summary>
        /// <param name="property">Expression pointing to the property to exclude</param>
        /// <param name="additionalProperties">Optional additional properties to exclude</param>
        /// <returns>Current builder for method chaining</returns>
        ISearchCriteriaConverterBuilder<T, TSearchCriteria> Excluding(Expression<Func<TSearchCriteria, object>> property, params Expression<Func<TSearchCriteria, object>>[] additionalProperties) => Excluding(property.ValidateArgument(nameof(property)).ExtractProperty(nameof(property)).Name, additionalProperties != null ? additionalProperties.Select((x, i) => x.ValidateArgument($"{nameof(additionalProperties)}[{i}]").ExtractProperty($"{nameof(additionalProperties)}[{i}]").Name).ToArray() : null);

        /// <summary>
        /// Defines a delegate for converting a property to a column name used for implicit conversions.
        /// </summary>
        /// <param name="nameSelector">Delegate that returns the column name for the supplied property, if null is returned the default logic will be used</param>
        /// <returns>Current builder for method chaining</returns>
        ISearchCriteriaConverterBuilder<T, TSearchCriteria> WithDefaultColumnName(Func<PropertyInfo, string?> nameSelector);
        /// <summary>
        /// Defines a delegate for selecting the data set alias for the column mapped to the supplied property.
        /// </summary>
        /// <param name="aliasSelector">Delegate that returns the data set alias for the supplied property, if null is returned the default logic will be used</param>
        /// <returns>Current builder for method chaining</returns>
        ISearchCriteriaConverterBuilder<T, TSearchCriteria> WithDefaultAlias(Func<PropertyInfo, object?> aliasSelector);
        /// <summary>
        /// Defines a delegate for selecting the operator that will be used to compare the column to the property value.
        /// </summary>
        /// <param name="operatorSelector">Delegate that returns the logical operator for the supplied property, if null is returned the default logic will be used</param>
        /// <returns>Current builder for method chaining</returns>
        ISearchCriteriaConverterBuilder<T, TSearchCriteria> WithDefaultOperator(Func<PropertyInfo, IStatementConditionOperatorExpressionBuilder<T>, IStatementConditionRightExpressionBuilder<T>?> operatorSelector);
        /// <summary>
        /// Defines a delegate for converting the property value to SQL.
        /// </summary>
        /// <param name="valueSelector">Delegate that converts the property value to SQL for the supplied property, if null is returned the default logic will be used</param>
        /// <returns>Current builder for method chaining</returns>
        ISearchCriteriaConverterBuilder<T, TSearchCriteria> WithDefaultValue(Func<PropertyInfo, object, DynamicParameters, IStatementConditionRightExpressionBuilder<T>, IChainedBuilder<T, IStatementConditionExpressionBuilder<T>>?> valueSelector);
    }

    /// <summary>
    /// Builder for configuring how a property must be converted to a SQL condition.
    /// </summary>
    /// <typeparam name="T">Type of the search criteria</typeparam>
    /// <typeparam name="TSearchCriteria">Type of the search criteria</typeparam>
    /// <typeparam name="TValue">Type of the property value</typeparam>
    public interface ISearchCriteriaConverterPropertyBuilder<T, TSearchCriteria, TValue> : ISearchCriteriaConverterBuilder<T, TSearchCriteria>
    {
        /// <summary>
        /// Defines the sql object that the current property is search criteria for. The default is the column with the property name where the alias is the same as the one defined for type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="objectSelector">Delegate that selects the sql object. First parameter is the property info, the second is the builder to use</param>
        /// <returns>Current builder for method chaining</returns>
        ISearchCriteriaConverterPropertyBuilder<T, TSearchCriteria, TValue> As(Func<PropertyInfo, IStatementConditionExpressionBuilder<T>, IStatementConditionOperatorExpressionBuilder<T>> objectSelector);
        /// <summary>
        /// Defines the sql operator to use to compare the sql object to the property value. The default is LIKE for <see cref="string"/>, IN for <see cref="IEnumerable{T}"/> and &lt;= for the rest.
        /// </summary>
        /// <param name="operatorSelector">Delegate that selects the sql operator, First parameter is the property info, the second is the builder to use</param>
        /// <returns>Current builder for method chaining</returns>
        ISearchCriteriaConverterPropertyBuilder<T, TSearchCriteria, TValue> CompareUsing(Func<PropertyInfo, IStatementConditionOperatorExpressionBuilder<T>, IStatementConditionRightExpressionBuilder<T>> operatorSelector);
        /// <summary>
        /// Defines how the property value is converted to SQL. The default is a parameter list where the property name is taken as the prefix (e.g. @Category0, @Category1, @Category2) for types deriving from <see cref="IEnumerable{T}"/> (except <see cref="string"/>) and a parameter where the property name is taken as the parameter name (e.g. @Name) for all the rest
        /// </summary>
        /// <param name="valueSelector">Delegate that selects the sql operator, First parameter is the property info, second parameter is the property value and the third is the builder to use</param>
        /// <returns></returns>
        ISearchCriteriaConverterPropertyBuilder<T, TSearchCriteria, TValue> ValueAs(Func<PropertyInfo, TValue, DynamicParameters?, IStatementConditionRightExpressionBuilder<T>, IChainedBuilder<T, IStatementConditionExpressionBuilder<T>>> valueSelector);
    }
}
