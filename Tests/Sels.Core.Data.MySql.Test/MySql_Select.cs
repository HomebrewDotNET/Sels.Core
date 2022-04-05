using Sels.Core.Data.SQL.Query;
using Sels.Core.Testing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.MySQL.Test
{
    public class MySql_Select
    {
        [Test]
        public void BuildsCorrectSelectAllQuery()
        {
            // Arrange
            var expected = "SELECT * FROM Person P".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().All().From();

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithDifferentColumns()
        {
            // Arrange
            var expected = "SELECT P.Name AS FirstName, P.SurName AS FamilyName, P.BirthDay FROM Person P".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().Column(x => x.Name, "FirstName").Column(x => x.SurName, "FamilyName").Column(x => x.BirthDay).From();

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithExcludedColumns()
        {
            // Arrange
            var expected = "SELECT P.Id, P.Name, P.SurName, P.BirthDay FROM Person P".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().ColumnsOf(nameof(Person.ResidenceId)).From();

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithSubQuery()
        {
            // Arrange
            var expected = "SELECT Q.Name, Q.Amount FROM (SELECT P.Name, Count(*) as Amount FROM Person P GROUP BY P.Name) Q".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select().Column("Q.Name").Column("Q.Amount")
                                .FromQuery(MySql.Select<Person>().Column(x => x.Name).CountAll("Amount").From().GroupBy(x => x.Name), "Q");

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithOrderBy()
        {
            // Arrange
            var expected = "SELECT P.* FROM Person P ORDER BY P.BirthDay DESC, P.SurName ASC, P.Name".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().AllOf().From().OrderBy(x => x.BirthDay, SortOrders.Descending).OrderBy(x => x.SurName, SortOrders.Ascending).OrderBy(x => x.Name);

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithGroupBy()
        {
            // Arrange
            var expected = "SELECT P.Name, Count(*) as Amount FROM Person P GROUP BY P.Name".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().Column(x => x.Name).CountAll("Amount").From().GroupBy(x => x.Name);

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithJoin()
        {
            // Arrange
            var expected = "SELECT R.* FROM Residence R INNER JOIN Person P ON P.ResidenceId = R.Id WHERE P.Id = @Id".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Residence>().AllOf<Residence>().From()
                                .Join<Person>(Joins.Inner).On<Person>(x => x.ResidenceId).To(x => x.Id).Exit()
                                .WhereColumn<Person>(x => x.Id).EqualTo().Parameter("Id").Exit();

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithConditions()
        {
            // Arrange
            var expected = "SELECT * From Person P WHERE P.Id >= 250 AND NOT (P.Name != @Name OR NOT P.SurName != @SurName) AND NOT P.ResidenceId = 5".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().All().From()
                                .WhereColumn(x => x.Id).GreaterOrEqualTo(250).And()
                                .Not().WhereGroup(g => 
                                            g.WhereColumn(x => x.Name).NotEqualTo().Parameter(x => x.Name).Or()
                                             .Not().WhereColumn(x => x.SurName).NotEqualTo().Parameter(x => x.SurName)
                                ).And().Not().WhereColumn(x => x.ResidenceId).EqualTo(5).Exit();

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }
    }
}
