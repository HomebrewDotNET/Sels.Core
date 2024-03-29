﻿using Sels.Core.Testing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.Core.Data.MySQL.Test
{
    public class MySql_With
    {
        [Test]
        public void BuildsCorrectCteQuery()
        {
            // Arrange
            var expected = "WITH `cte` AS ( SELECT * FROM `Person` P ) SELECT * FROM `cte`".GetWithoutWhitespace().ToLower();
            var builder = MySql.With()
                                .Cte("cte").Using(MySql.Select<Person>().All().From())
                                .Execute(MySql.Select().All().From("cte"));

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectCteQueryWithMultipleCtes()
        {
            // Arrange
            var expected = "WITH `personCte` AS ( SELECT P.`Id` FROM `Person` P ), `residenceCte` AS ( SELECT R.`Id` FROM `Residence` R ) SELECT * FROM `personCte` UNION SELECT * FROM `residenceCte`".GetWithoutWhitespace().ToLower();
            var builder = MySql.With()
                                .Cte("personCte").Using(MySql.Select<Person>().Column(c => c.Id).From())
                                .Cte("residenceCte").Using(MySql.Select<Residence>().Column(c => c.Id).From())
                                .Execute(
                                    MySql.Select().All().From("personCte")
                                    .Union(
                                    MySql.Select().All().From("residenceCte"))
                                    );

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }

        [Test]
        public void BuildsCorrectRecursiveCteQuery()
        {
            // Arrange
            var expected = "WITH Recursive `cte` AS ( SELECT 1 as n UNION ALL SELECT n+1 FROM `cte` WHERE n < 20) SELECT * FROM `cte`".GetWithoutWhitespace().ToLower();
            var builder = MySql.With()
                                .RecursiveCte("cte").Using(
                                    MySql.Select().Value(1, "n")
                                    .UnionAll(
                                    MySql.Select().Expression("n+1").From("cte").Where(x => x.Expression("n").LesserThan.Value(20))    
                                    ))                                   
                                .Execute(
                                    MySql.Select().All().From("cte")
                                    );

            // Act
            var query = builder.Build();

            // Assert
            Assert.IsNotNull(query);
            Assert.AreEqual(expected, query.GetWithoutWhitespace().ToLower());
        }
    }
}
