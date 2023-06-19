using Sels.SQL.QueryBuilder.Builder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.SQL.QueryBuilder.MySQL.Test
{
    public class MySql_If
    {
        [Test]
        public void BuildsCorrectQueryWithIfStatement()
        {
            // Arrange
            var expected = "IF @PrintStuff = 1 THEN Print('Stuff'); END IF;".GetWithoutWhitespace().ToLower();

            // Act
            var builder = MySql.If().Condition(x => x.Parameter("PrintStuff").EqualTo.Value(1))
                                    .Then("Print('Stuff')", true);
            var query = builder.Build(ExpressionCompileOptions.Format | ExpressionCompileOptions.AppendSeparator);

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectQueryWithIfAndElseStatement()
        {
            // Arrange
            var expected = "IF EXISTS(SELECT 1 FROM `Queue`) THEN SELECT * FROM `Queue` ORDER BY `Priority` ASC LIMIT 1; ELSE Print('Queue Empty'); END IF;".GetWithoutWhitespace().ToLower();

            // Act
            var builder = MySql.If().Condition(x => x.ExistsIn(MySql.Select().Value(1).From("Queue")))
                                    .Then(MySql.Select().All().From("Queue").OrderBy("Priority", SortOrders.Ascending).Limit(1))
                               .Else
                                    .Then("Print('Queue Empty')");
            var query = builder.Build(ExpressionCompileOptions.Format | ExpressionCompileOptions.AppendSeparator);

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectQueryWithIfAndElseIfAndElseStatement()
        {
            // Arrange
            var expected = "IF @Mode = 2 THEN Print('Mode 2 selected'); Select * From `TableOne`; ELSE IF @Mode = 1 THEN Print('Mode 1 selected'); Select * From `TableTwo`; ELSE Print('Unknown mode selected'); END IF;".GetWithoutWhitespace().ToLower();

            // Act
            var builder = MySql.If().Condition(x => x.Parameter("@Mode").EqualTo.Value(2))
                                    .Then("Print('Mode 2 selected')")
                                    .Then(MySql.Select().All().From("TableOne"))
                               .ElseIf.Condition(x => x.Parameter("@Mode").EqualTo.Value(1))
                                    .Then("Print('Mode 1 selected')")
                                    .Then(MySql.Select().All().From("TableTwo"))
                               .Else
                                    .Then("Print('Unknown mode selected')");
            var query = builder.Build(ExpressionCompileOptions.Format | ExpressionCompileOptions.AppendSeparator);

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }
    }
}
