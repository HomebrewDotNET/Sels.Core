using Sels.SQL.QueryBuilder.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.SQL.QueryBuilder.MySQL.Test
{
    public class MySql_Build
    {
        [Test]
        public void BuildsCorrectQueryWithMultipleStatements()
        {
            // Arrange
            var expected = "WITH `cte` AS ( SELECT * FROM `Queue` Q ORDER BY Q.`Created` ASC LIMIT 5 FOR UPDATE) UPDATE `Queue` Q INNER JOIN `cte` C ON Q.`Id` = C.`Id` SET Q.`ProcessId` = @ProcessId; SELECT * FROM `Queue` Q WHERE Q.`ProcessId` = @ProcessId;".GetWithoutWhitespace().ToLower();
            var builder = MySql.Build();

            // Act
            var cteBuilder = MySql.With().Cte("cte")
                                            .Using(MySql.Select().All()
                                                        .From("Queue", datasetAlias: "Q").ForUpdate()
                                                        .OrderBy("Q", "Created", SortOrders.Ascending)
                                                        .Limit(5))
                                         .Execute(MySql.Update().Table("Queue", datasetAlias: "Q")
                                                                .InnerJoin().Table("cte", datasetAlias: "C").On(x => x.Column("Q", "Id").EqualTo.Column("C", "Id"))
                                                                .Set.Column("Q", "ProcessId").To.Parameter("ProcessId"));
            var selectBuilder = MySql.Select().All()
                                     .From("Queue", datasetAlias: "Q")
                                     .Where(x => x.Column("Q", "ProcessId").EqualTo.Parameter("ProcessId"));
            builder.Append(cteBuilder);
            builder.Append(selectBuilder);
            var query = builder.Build(ExpressionCompileOptions.Format | ExpressionCompileOptions.AppendSeparator);

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectQueryWithExpressions()
        {
            // Arrange
            var expected = "SET @ProcessId = 1; SELECT * FROM `Queue` Q WHERE Q.`ProcessId` = @ProcessId;".GetWithoutWhitespace().ToLower();
            var builder = MySql.Build();

            // Act
            builder.Append("SET @ProcessId = 1");
            var selectBuilder = MySql.Select().All()
                                     .From("Queue", datasetAlias: "Q")
                                     .Where(x => x.Column("Q", "ProcessId").EqualTo.Parameter("ProcessId"));
            builder.Append(selectBuilder);
            var query = builder.Build(ExpressionCompileOptions.Format | ExpressionCompileOptions.AppendSeparator);

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }
    }
}
