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
            var expected = "SELECT Q.Name, Q.Amount FROM (SELECT P.Name, Count(*) as Amount FROM Person P GROUP BY P.Name) Q WHERE Q.Amount > 1".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select().Column("Q.Name").Column("Q.Amount")
                                .FromQuery(MySql.Select<Person>().Column(x => x.Name).CountAll("Amount").From().GroupBy(x => x.Name), "Q")
                                .Where(x => x.Column("Q.Amount").GreaterThan(1));

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
                                .Join<Person>(Joins.Inner, x => x.On<Person>(x => x.ResidenceId).To(x => x.Id))
                                .Where(w => w.Column<Person>(x => x.Id).EqualTo().Parameter(x => x.Id));

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
            var expected = "SELECT * From Person P WHERE P.Id >= 250 AND NOT (P.Name != @Name OR NOT P.SurName != @SurName) AND NOT P.ResidenceId NOT IN (1,2,3,4,5)".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().All().From()
                                .Where(w => w.Column(x => x.Id).GreaterOrEqualTo(250).And()
                                             .Not().WhereGroup(g => g.Column(x => x.Name).NotEqualTo().Parameter(x => x.Name).Or()
                                                                     .Not().Column(x => x.SurName).NotEqualTo().Parameter(x => x.SurName)).And()
                                             .Not().Column(x => x.ResidenceId).NotIn().Values(1,2,3,4,5));

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithExistsCondition()
        {
            // Arrange
            var expected = "SELECT * From Person P WHERE NOT EXISTS (SELECT * FROM Residence R WHERE R.Id = P.ResidenceId)".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().All().From()
                                .Where(x => x.Not().ExistsIn(MySql.Select<Residence>().All().From().Where(x => x.Column(x => x.Id).EqualTo().Column<Person>(x => x.ResidenceId))));

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithInSubQueryCondition()
        {
            // Arrange
            var expected = "SELECT * From Person P WHERE P.ResidenceId IN (SELECT R.Id FROM Residence R WHERE R.Id <= 100)".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().All().From()
                                .Where(x => x.Column(x => x.ResidenceId).In()
                                             .Query(MySql.Select<Residence>().Column(x => x.Id).From().Where(x => x.Column(x => x.Id).LesserOrEqualTo().Value(100))));

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }
    }
}
