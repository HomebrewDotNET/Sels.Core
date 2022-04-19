using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL
{
    /// <summary>
    /// Contains sql related constant values.
    /// </summary>
    public static class Sql
    {
        /// <summary>
        /// Contains the sql statements.
        /// </summary>
        public static class Statements
        {
            /// <summary>
            /// The sql statement for creating data.
            /// </summary>
            public const string Insert = "INSERT INTO";
            /// <summary>
            /// The sql statement for reading data.
            /// </summary>
            public const string Select = "SELECT";
            /// <summary>
            /// The sql statement for deleting data.
            /// </summary>
            public const string Delete = "DELETE";
        }

        /// <summary>
        /// Contains the sql clauses.
        /// </summary>
        public static class Clauses
        {
            /// <summary>
            /// The sql clause for defining the values to insert.
            /// </summary>
            public const string Values = "VALUES";
            /// <summary>
            /// The sql clause for defining where to select from.
            /// </summary>
            public const string From = "FROM";
            /// <summary>
            /// The sql clause for defining conditions.
            /// </summary>
            public const string Where = "WHERE";
            /// <summary>
            /// The sql clause for defining how to order the query results.
            /// </summary>
            public const string OrderBy = "ORDER BY";
            /// <summary>
            /// The sql clause for defining how to group the query results.
            /// </summary>
            public const string GroupBy = "GROUP BY";
        }

        /// <summary>
        /// Contains the keywords for joins.
        /// </summary>
        public static class Joins
        {
            /// <summary>
            /// The keyword for an inner join.
            /// </summary>
            public const string InnerJoin = "INNER JOIN";
            /// <summary>
            /// The keyword for a full (outer) join.
            /// </summary>
            public const string FullJoin = "FULL JOIN";
            /// <summary>
            /// The keyword for a left (outer) join.
            /// </summary>
            public const string LeftJoin = "LEFT JOIN";
            /// <summary>
            /// The keyword for a right (outer) join.
            /// </summary>
            public const string RightJoin = "RIGHT JOIN";
        }
        /// <summary>
        /// Contains the sql function names.
        /// </summary>
        public static class Functions
        {
            /// <summary>
            /// The name of the max function.
            /// </summary>
            public const string Max = "MAX";
            /// <summary>
            /// The name of the min function.
            /// </summary>
            public const string Min = "MIN";
            /// <summary>
            /// The name of the avg function.
            /// </summary>
            public const string Avg = "AVG";
            /// <summary>
            /// The name of the sum function.
            /// </summary>
            public const string Sum = "SUM";
            /// <summary>
            /// The name of the count function.
            /// </summary>
            public const string Count = "COUNT";
        }
        /// <summary>
        /// Contains the sql sort orders.
        /// </summary>
        public static class SortOrders
        {
            /// <summary>
            /// Sorts from smallest to largest.
            /// </summary>
            public const string Asc = "ASC";
            /// <summary>
            /// Sorts from largest to smallest.
            /// </summary>
            public const string Desc = "DESC";
        }
        /// <summary>
        /// Contains the sql operators for comparing values.
        /// </summary>
        public static class ConditionOperators
        {
            /// <summary>
            /// Expressions should be equal.
            /// </summary>
            public const string Equal = "=";
            /// <summary>
            /// Expressions should not be equal.
            /// </summary>
            public const string NotEqual = "!=";
            /// <summary>
            /// Expression should be greater than other expression.
            /// </summary>
            public const string Greater = ">";
            /// <summary>
            /// Expression should be lesser than other expression.
            /// </summary>
            public const string Less = "<";
            /// <summary>
            /// Expression should be greater or equal to other expression.
            /// </summary>
            public const string GreaterOrEqual = ">=";
            /// <summary>
            /// Expression should be lesser or equal to other expression.
            /// </summary>
            public const string LessOrEqual = "<=";
            /// <summary>
            /// Expression should be in list of values.
            /// </summary>
            public const string In = "IN";
            /// <summary>
            /// Expression should not be in list of values.
            /// </summary>
            public const string NotIn = Not + Constants.Strings.Space + In;
            /// <summary>
            /// Expression should be like a pattern.
            /// </summary>
            public const string Like = "LIKE";
            /// <summary>
            /// Expression should not be like a pattern.
            /// </summary>
            public const string NotLike = Not + Constants.Strings.Space + Like;
            /// <summary>
            /// Expression should exist in another expression.
            /// </summary>
            public const string Exists = "EXISTS";
            /// <summary>
            /// Expression should not exist in another expression.
            /// </summary>
            public const string NotExists = Not + Constants.Strings.Space + Exists;
            /// <summary>
            /// Expression should be between 2 values.
            /// </summary>
            public const string Between = "BETWEEN";
            /// <summary>
            /// Expression should not be between 2 values.
            /// </summary>
            public const string NotBetween = Not + Constants.Strings.Space + Between;
            /// <summary>
            /// Expression should be equal to a contant value.
            /// </summary>
            public const string Is = "IS";
            /// <summary>
            /// Expression should not be equal to a contant value.
            /// </summary>
            public const string IsNot = Not + Constants.Strings.Space + Is;
        }
        /// <summary>
        /// Contains the sql logic operators for comparing conditions.
        /// </summary>
        public static class LogicOperators
        {
            /// <summary>
            /// Current and next condition both need to be true.
            /// </summary>
            public const string And = "AND";
            /// <summary>
            /// Current and next condition either need to be true.
            /// </summary>
            public const string Or = "OR";
        }

        /// <summary>
        /// The sql value for selecting everything from a dataset.
        /// </summary>
        public const char All = '*';
        /// <summary>
        /// The sql keyword for defining an alias.
        /// </summary>
        public const string As = "AS";
        /// <summary>
        /// The sql keyword for defining what columns to join on.
        /// </summary>
        public const string On = "ON";
        /// <summary>
        /// The sql keyword for null.
        /// </summary>
        public const string Null = "NULL";
        /// <summary>
        /// The sql keyword for inverting a condition result.
        /// </summary>
        public const string Not = "NOT";
    }
}
