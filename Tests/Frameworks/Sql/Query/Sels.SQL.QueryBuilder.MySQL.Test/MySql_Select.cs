using Sels.SQL.QueryBuilder.Builder;
using Sels.Core.Testing.Models;
using System;
using Sels.SQL.QueryBuilder.Expressions;

namespace Sels.SQL.QueryBuilder.MySQL.Test
{
    public class MySql_Select
    {
        [Test]
        public void BuildsCorrectSelectAllQuery()
        {
            // Arrange
            var expected = "SELECT * FROM `Person` P".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().All().From();

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithDistinctKeyword()
        {
            // Arrange
            var expected = "SELECT DISTINCT P.`SurName` FROM `Person` P".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().Distinct().Column(x => x.SurName).From();

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithLimitKeyword()
        {
            // Arrange
            var expected = "SELECT * FROM `Person` P WHERE P.`Id` = 52 LIMIT 1".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().All().From().Where(w => w.Column(x => x.Id).EqualTo.Value(52)).Limit(1);

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithLimitKeywordIncludingAOffset()
        {
            // Arrange
            var expected = "SELECT * FROM `Person` P WHERE P.`Name` LIKE CONCAT('%', @Name, '%') LIMIT 1, 10".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().All().From()
                                    .Where(w =>
                                        w.Column(x => x.Name).LikeParameter("Name")
                                    ).Limit(1, 10);

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithForUpdateKeyword()
        {
            // Arrange
            var expected = "SELECT @Id = P.Id FROM `Person` P LIMIT 1 FOR UPDATE".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>()
                                    .Expression("@Id = P.Id")
                                    .From()
                                    .Limit(1)
                                    .ForUpdate();

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
            var expected = "SELECT P.`Name` AS FirstName, P.`SurName` AS FamilyName, P.`BirthDay` FROM `Person` P".GetWithoutWhitespace().ToLower();
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
            var expected = "SELECT P.`Id`, P.`Name`, P.`SurName`, P.`BirthDay` FROM `Person` P".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().ColumnsOf(nameof(Person.ResidenceId)).From();

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithConstantValues()
        {
            // Arrange
            var expected = "SELECT 1, 'Jens'".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select().Value(1).Value("Jens");

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
            var expected = "SELECT Q.`Name`, Q.`Amount` FROM (SELECT P.`Name`, Count(*) as Amount FROM `Person` P GROUP BY P.`Name`) Q WHERE Q.`Amount` > 1".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select().Column("Q", "Name").Column("Q", "Amount")
                                .FromQuery(MySql.Select<Person>().Column(x => x.Name).CountAll("Amount").From().GroupBy(x => x.Name), "Q")
                                .Where(x => x.Column("Q", "Amount").GreaterThan.Value(1));

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
            var expected = "SELECT P.* FROM `Person` P ORDER BY P.`BirthDay` DESC, P.`SurName` ASC, P.`Name`".GetWithoutWhitespace().ToLower();
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
            var expected = "SELECT P.`Name`, Count(*) as Amount FROM `Person` P GROUP BY P.`Name`".GetWithoutWhitespace().ToLower();
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
            var expected = "SELECT R.* FROM `Residence` R INNER JOIN `Person` P ON P.`ResidenceId` = R.`Id` WHERE P.`Id` = @Id".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Residence>().AllOf<Residence>().From()
                                .InnerJoin().Table<Person>().On(x => x.Column<Person>(c => c.ResidenceId).EqualTo.Column(c => c.Id))
                                .Where(w => w.Column<Person>(x => x.Id).EqualTo.Parameter(x => x.Id));

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
            var expected = "SELECT * From `Person` P WHERE P.`Id` >= 250 AND NOT (P.`Name` != @Name OR NOT P.`SurName` != @SurName) AND NOT P.`ResidenceId` NOT IN (1,2,3,4,5)".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().All().From()
                                .Where(w => w.Column(x => x.Id).GreaterOrEqualTo.Value(250).And
                                             .Not().WhereGroup(g => g.Column(x => x.Name).NotEqualTo.Parameter(x => x.Name).Or
                                                                     .Not().Column(x => x.SurName).NotEqualTo.Parameter(x => x.SurName)).And
                                             .Not().Column(x => x.ResidenceId).NotIn.Values(1, 2, 3, 4, 5));

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
            var expected = "SELECT * From `Person` P WHERE NOT EXISTS (SELECT * FROM `Residence` R WHERE R.`Id` = P.`ResidenceId`)".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().All().From()
                                    .Where(x => x.Not().ExistsIn(
                                        MySql.Select<Residence>().All().From()
                                                .Where(x => x.Column(x => x.Id).EqualTo.Column<Person>(x => x.ResidenceId))
                                    ));

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
            var expected = "SELECT * From `Person` P WHERE P.`ResidenceId` IN (SELECT R.`Id` FROM `Residence` R WHERE R.`Id` <= 100)".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().All().From()
                                .Where(x => x.Column(x => x.ResidenceId).In
                                             .Query(MySql.Select<Residence>().Column(x => x.Id).From().Where(x => x.Column(x => x.Id).LesserOrEqualTo.Value(100))));

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithUnion()
        {
            // Arrange
            var expected = "SELECT COUNT(P.`Name`) As Amount FROM `Person` P UNION SELECT COUNT(R.`Street`) as Amount FROM `Residence` R".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().Count(x => x.Name, "Amount").From()
                            .Union(MySql.Select<Residence>().Count(x => x.Street, "Amount").From());

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithConcatFunction()
        {
            // Arrange
            var expected = "SELECT * From `Person` P WHERE P.`SurName` LIKE CONCAT('%', 'Sel', '%')".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().All().From()
                                    .Where(w =>
                                        w.Column(x => x.SurName).Like.Concat("%", "Sel", "%")
                                    );

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithCaseWhenCondition()
        {
            // Arrange
            var expected = "SELECT (CASE WHEN P.`ProductCategory` = 'NSFW' THEN 1 ELSE 0 END) AS `Hidden` FROM `Products` P".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select()
                                   .Case(x => x.When(w => w.Column("P", "ProductCategory").EqualTo.Value("NSFW")).Then.Value(1)
                                               .Else.Value(0),
                                        "Hidden")
                                .From("Products", datasetAlias: "P");

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithCaseRawCondition()
        {
            // Arrange
            var expected = "SELECT (CASE WHEN P.`Id` = MAX(P.`Id`) THEN 1 ELSE 0 END) AS `IsLast` FROM `Products` P".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select()
                                   .Case(x => x.When("P.`Id` = MAX(P.`Id`)").Then.Value(1)
                                               .Else.Value(0),
                                        "IsLast")
                                .From("Products", datasetAlias: "P");

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithOrderByCase()
        {
            // Arrange
            var expected = "SELECT * FROM `Products` P ORDER BY (CASE WHEN P.`ProductCategory` = 'NSFW' THEN 0 ELSE 1 END) DESC".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select().All()
                                .From("Products", datasetAlias: "P")
                                .OrderByCase(c => c.When(w => w.Column("P", "ProductCategory").EqualTo.Value("NSFW")).Then.Value(0)
                                                   .Else.Value(1)
                                            , SortOrders.Descending);

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithVariableAssignment()
        {
            // Arrange
            var expected = "SELECT @Id := P.`Id` FROM `Person` P LIMIT 1".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>()
                                 .Expression(b => b.AssignVariable("Id", v => v.Column(c => c.Id)))
                               .From()
                               .Limit(1);

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithVariable()
        {
            // Arrange
            var expected = "SELECT @Id".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select().Variable("Id");

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectSelectQueryWithCurrentDateAndTypeServer()
        {
            // Arrange
            var expected = "SELECT NOW(6)".GetWithoutWhitespace().ToLower();

            // Act
            var query = MySql.Select().Expression(b => b.CurrentDate(DateType.Server)).Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }
        [Test]
        public void BuildsCorrectSelectQueryWithCurrentDateAndTypeUtc()
        {
            // Arrange
            var expected = "SELECT UTC_TIMESTAMP(6)".GetWithoutWhitespace().ToLower();

            // Act
            var query = MySql.Select().Expression(b => b.CurrentDate(DateType.Utc)).Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [TestCase(1, DateInterval.Minute, "1", "MINUTE")]
        [TestCase(-1500, DateInterval.Millisecond, "-1500 * 1000", "MICROSECOND")]
        [TestCase(24, DateInterval.Month, "24", "MONTH")]
        [TestCase(1.5, DateInterval.Hour, "1.5", "HOUR")]
        public void BuildsCorrectSelectQueryWithModifyDateInterval(double amount, DateInterval interval, string expectedAmount, string expectedInterval)
        {
            // Arrange
            var expected = $"SELECT DATE_ADD(NOW(6), INTERVAL {expectedAmount} {expectedInterval})".GetWithoutWhitespace().ToLower();

            // Act
            var query = MySql.Select().Expression(b => b.ModifyDate(b => b.CurrentDate(DateType.Server), amount, interval)).Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }
    }
}
