using Sels.Core.Data.SQL.Query;
using Sels.Core.Testing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.MySQL.Test
{
    public class MySql_Update
    {
        [Test]
        public void BuildsCorrectUpdateQuery()
        {
            // Arrange
            var expected = "UPDATE Person Set Name = 'Jens' WHERE Name = 'jens'".GetWithoutWhitespace().ToLower();
            var builder = MySql.Update().Table("Person")
                                    .SetColumnTo("Name").Value("Jens")
                                    .Where(x => x.Column("Name").EqualTo("jens"));

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectUpdateQueryWithJoin()
        {
            // Arrange
            var expected = "UPDATE Person P INNER JOIN Residence R ON P.ResidenceId = R.Id SET P.BirthDay = GETDATE(), R.HouseNumber = 78 WHERE R.Id = 1998".GetWithoutWhitespace().ToLower();
            var builder = MySql.Update<Person>().Table()
                                    .Join<Residence>(Joins.Inner, x => x.On(x => x.ResidenceId).To<Residence>(x => x.Id))
                                    .SetColumnTo(x => x.BirthDay).Expression("GETDATE()")
                                    .SetColumnTo<Residence>(x => x.HouseNumber).Value(78)
                                    .Where(x => x.Column<Residence>(c => c.Id).EqualTo(1998));

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectUpdateQueryFromObject()
        {
            // Arrange
            var expected = "UPDATE Person P SET P.Name = 'Jens', P.SurName = 'Sels', P.BirthDay = '1998-01-04 00:13:45', P.ResidenceId = 90 WHERE P.Id = 1".GetWithoutWhitespace().ToLower();
            var person = new Person()
            {
                Id = 5,
                Name = "Jens",
                SurName = "Sels",
                BirthDay = DateTime.ParseExact("04/01/1998 00:13:45", "dd/MM/yyyy HH:mm:ss", null),
                ResidenceId = 90
            };
            var builder = MySql.Update<Person>().Table().OutAlias<Person>(out var alias)
                                    .SetUsing(person, excludedProperties: nameof(Person.Id))
                                    .Where(x => x.Column(alias, nameof(Person.Id)).EqualTo(1));

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectUpdateQueryFromObjectUsingParameters()
        {
            // Arrange
            var expected = "UPDATE Person P SET P.Name = @Name, P.SurName = @SurName, P.BirthDay = @BirthDay, P.ResidenceId = @ResidenceId WHERE P.Id = 1".GetWithoutWhitespace().ToLower();
            var builder = MySql.Update<Person>().Table()
                                    .SetFrom<Person>(excludedProperties: nameof(Person.Id))
                                    .Where(x => x.Column(typeof(Person), nameof(Person.Id)).EqualTo(1));

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectUpdateQueryWithConditions()
        {
            // Arrange
            var expected = "UPDATE Person P SET P.Name = 'Jens' WHERE EXISTS (SELECT * FROM Residence R WHERE R.PostalCode BETWEEN 2500 AND 2599 AND R.Id > P.ResidenceId)".GetWithoutWhitespace().ToLower();
            var builder = MySql.Update<Person>().Table()
                                    .SetColumnTo(x => x.Name).Value("Jens")
                                    .Where(x => 
                                            x.ExistsIn(MySql.Select<Residence>().All().From().Where(w => w.Column(c => c.PostalCode).Between(2500, 2599).And().Column(c => c.Id).GreaterThan().Column<Person>(x => x.ResidenceId)))
                                    );

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }
    }
}
