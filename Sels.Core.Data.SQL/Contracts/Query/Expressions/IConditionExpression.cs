using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.SQL.Query.Expressions
{
    /// <summary>
    /// Expression that represents a condition in a where clause.
    /// </summary>
    public interface IConditionExpression : IExpressionContainer
    {
        /// <summary>
        /// If the current clause should be inverted 
        /// </summary>
        public bool IsNot { get; set; }
        /// <summary>
        /// How the current condition and the next condition (if one exists) should be joined together.
        /// </summary>
        public LogicOperators? LogicOperator { get; set; }
    }
}
