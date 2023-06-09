using Sels.SQL.QueryBuilder.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.SQL.QueryBuilder.MySQL.Test
{
    public class MySql_Set
    {
        [TestCase("Hello", "'Hello'")]
        [TestCase(2, "2")]
        [TestCase(2.2d, "2.2")]
        public void BuildsCorrectQueryWithConstantAssignment(object constant, string expectedConstant)
        {
            // Arrange
            var expected = $"SET @Variable = {expectedConstant};".GetWithoutWhitespace().ToLower();

            // Act
            var query = MySql.Set().Variable("Variable").To.Value(constant).Build(ExpressionCompileOptions.AppendSeparator);

            // Assert
            Assert.That(query?.GetWithoutWhitespace()?.ToLower(), Is.EqualTo(expected));
        }

        [Test]
        public void BuildsCorrectQueryWithSubQuery()
        {
            // Arrange
            var expected = $"SET @Id = (SELECT `Id` FROM `Queues` ORDER BY `CreatedAt` ASC LIMIT 1 FOR UPDATE);".GetWithoutWhitespace().ToLower();

            // Act
            var query = MySql.Set().Variable("@Id").To.Query(MySql.Select().Column("Id").From("Queues").ForUpdate().OrderBy("CreatedAt", SortOrders.Ascending).Limit(1)).Build(ExpressionCompileOptions.AppendSeparator);

            // Assert
            Assert.That(query?.GetWithoutWhitespace()?.ToLower(), Is.EqualTo(expected));
        }
    }
}
