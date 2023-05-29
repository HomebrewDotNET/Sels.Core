using Sels.SQL.QueryBuilder.Builder.Statement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.SQL.QueryBuilder.Builder.Compilation
{
    /// <summary>
    /// Compiler that converts various builders and expressions into SQL.
    /// </summary>
    public interface ISqlCompiler :
        IQueryCompiler<InsertExpressionPositions>,
        IQueryCompiler<SelectExpressionPositions>,
        IQueryCompiler<UpdateExpressionPositions>,
        IQueryCompiler<DeleteExpressionPositions>,
        IIfStatementCompiler,
        IExpressionCompiler
    {
    }
}
