using Sels.Core.Testing.Models;
using System;

namespace Sels.SQL.QueryBuilder.MySQL.Test
{
    public class MySql_Delete
    {
        [Test]
        public void BuildsCorrectDeleteQuery()
        {
            // Arrange
            var expected = "DELETE FROM `Person`".GetWithoutWhitespace().ToLower();
            var builder = MySql.Delete().From("Person");

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectDeleteQueryWithTableAlias()
        {
            // Arrange
            var expected = "DELETE P FROM `Person` P".GetWithoutWhitespace().ToLower();
            var builder = MySql.Delete<Person>().From();

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectDeleteQueryWithJoin()
        {
            // Arrange
            var expected = "DELETE P FROM `Person` P FULL JOIN `Residence` R ON R.`Id` = P.`ResidenceId` WHERE P.`Id` = @Id".GetWithoutWhitespace().ToLower();
            var builder = MySql.Delete<Person>().From()
                                .FullJoin().Table<Residence>().On(x => x.Column<Residence>(x => x.Id).EqualTo.Column(c => c.ResidenceId))
                                .Where(x => x.Column(x => x.Id).EqualTo.Parameter(x => x.Id));

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectDeleteQueryWithCondition()
        {
            // Arrange
            var expected = "DELETE P FROM `Person` P WHERE P.`Name` LIKE '%Sels%'".GetWithoutWhitespace().ToLower();
            var builder = MySql.Delete<Person>().From()
                                .Where(x => x.Column(x => x.Name).Like.Value("%Sels%"));

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectDeleteQueryWithReturningKeyword()
        {
            // Arrange
            var expected = "DELETE P FROM `Person` P RETURNING P.`Id`".GetWithoutWhitespace().ToLower();
            var builder = MySql.Delete<Person>().From()
                                .Returning(x => x.Column(c => c.Id));

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }
    }
}
