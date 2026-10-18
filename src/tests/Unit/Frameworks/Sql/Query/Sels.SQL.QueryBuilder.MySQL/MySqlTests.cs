using Sels.Core.Testing.Models;
using Sels.SQL.QueryBuilder.Builder;
using Sels.SQL.QueryBuilder.Expressions;
using System.Text;

namespace Sels.SQL.QueryBuilder.MySQL.Tests
{
    /// <summary>Contract tests for <see cref="MySql"/> query building.</summary>
    [Trait("Category", "Unit")]
    public class MySqlTests
    {
        /// <summary>Verifies Build for multiple statements: returns expected query.</summary>
        [Fact]
        public void Build_MultipleStatements_ReturnsExpectedQuery()
        {
            // Arrange
            var expected = "WITH `cte` AS ( SELECT * FROM `Queue` Q ORDER BY Q.`Created` ASC LIMIT 5 FOR UPDATE) UPDATE `Queue` Q INNER JOIN `cte` C ON Q.`Id` = C.`Id` SET Q.`ProcessId` = @ProcessId; SELECT * FROM `Queue` Q WHERE Q.`ProcessId` = @ProcessId;".GetWithoutWhitespace().ToLower();
            var builder = MySql.New();

            // Act
            var cteBuilder = MySql.With().Cte("cte")
                                            .As(MySql.Select().All()
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
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Build for expressions: returns expected query.</summary>
        [Fact]
        public void Build_Expressions_ReturnsExpectedQuery()
        {
            // Arrange
            var expected = "SET @ProcessId = 1; SELECT * FROM `Queue` Q WHERE Q.`ProcessId` = @ProcessId;".GetWithoutWhitespace().ToLower();
            var builder = MySql.New();

            // Act
            builder.Append("SET @ProcessId = 1");
            var selectBuilder = MySql.Select().All()
                                     .From("Queue", datasetAlias: "Q")
                                     .Where(x => x.Column("Q", "ProcessId").EqualTo.Parameter("ProcessId"));
            builder.Append(selectBuilder);
            var query = builder.Build(ExpressionCompileOptions.Format | ExpressionCompileOptions.AppendSeparator);

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Delete for no options: builds expected query.</summary>
        [Fact]
        public void Delete_NoOptions_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "DELETE FROM `Person`".GetWithoutWhitespace().ToLower();
            var builder = MySql.Delete().From("Person");

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Delete for table alias: builds expected query.</summary>
        [Fact]
        public void Delete_TableAlias_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "DELETE P FROM `Person` P".GetWithoutWhitespace().ToLower();
            var builder = MySql.Delete<Person>().From();

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Delete for join: builds expected query.</summary>
        [Fact]
        public void Delete_Join_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "DELETE P FROM `Person` P FULL JOIN `Residence` R ON R.`Id` = P.`ResidenceId` WHERE P.`Id` = @Id".GetWithoutWhitespace().ToLower();
            var builder = MySql.Delete<Person>().From()
                                .FullJoin().Table<Residence>().On(x => x.Column<Residence>(x => x.Id).EqualTo.Column(c => c.ResidenceId))
                                .Where(x => x.Column(x => x.Id).EqualTo.Parameter(x => x.Id));

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Delete for condition: builds expected query.</summary>
        [Fact]
        public void Delete_Condition_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "DELETE P FROM `Person` P WHERE P.`Name` LIKE '%Sels%'".GetWithoutWhitespace().ToLower();
            var builder = MySql.Delete<Person>().From()
                                .Where(x => x.Column(x => x.Name).Like.Value("%Sels%"));

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Delete for returning keyword: builds expected query.</summary>
        [Fact]
        public void Delete_ReturningKeyword_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "DELETE P FROM `Person` P RETURNING P.`Id`".GetWithoutWhitespace().ToLower();
            var builder = MySql.Delete<Person>().From()
                                .Returning(x => x.Column(c => c.Id));

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies If for if statement: builds expected query.</summary>
        [Fact]
        public void If_IfStatement_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "IF @PrintStuff = 1 THEN Print('Stuff'); END IF;".GetWithoutWhitespace().ToLower();

            // Act
            var builder = MySql.If().Condition(x => x.Parameter("PrintStuff").EqualTo.Value(1))
                                    .Then("Print('Stuff')", true);
            var query = builder.Build(ExpressionCompileOptions.Format | ExpressionCompileOptions.AppendSeparator);

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies If for if and else: builds expected query.</summary>
        [Fact]
        public void If_IfAndElse_BuildsExpectedQuery()
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
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies If for if else if and else: builds expected query.</summary>
        [Fact]
        public void If_IfElseIfAndElse_BuildsExpectedQuery()
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
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Insert for no options: builds expected query.</summary>
        [Fact]
        public void Insert_NoOptions_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "INSERT INTO `Person` (`Id`, `Name`, `SurName`)".GetWithoutWhitespace().ToLower();
            var builder = MySql.Insert<Person>().Into().Column(x => x.Id).Column(x => x.Name).Column(x => x.SurName);

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Insert for multiple values: builds expected query.</summary>
        [Fact]
        public void Insert_MultipleValues_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "INSERT INTO `Person` (`Id`, `Name`, `SurName`) VALUES (1, 'Jens', 'Sels'), (2, 'Jarno', 'Sels')".GetWithoutWhitespace().ToLower();
            var builder = MySql.Insert<Person>().Into().Column(x => x.Id).Column(x => x.Name).Column(x => x.SurName)
                            .Values(1, "Jens", "Sels")
                            .ValuesUsing(new Person() { Id = 2, Name = "Jarno", SurName = "Sels" }, nameof(Person.BirthDay), nameof(Person.ResidenceId));

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Insert for multiple parameterized values: builds expected query.</summary>
        [Fact]
        public void Insert_MultipleParameterizedValues_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "INSERT INTO `Person` (`Id`, `Name`, `SurName`) VALUES (@Id, @Name, @SurName), (@Id1, @Name1, @SurName1)".GetWithoutWhitespace().ToLower();
            var builder = MySql.Insert<Person>().Into().Column(x => x.Id).Column(x => x.Name).Column(x => x.SurName)
                            .Parameters("Id", "Name", "SurName")
                            .ParametersFrom(1.ToString(), nameof(Person.BirthDay), nameof(Person.ResidenceId));

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Insert for returning keyword: builds expected query.</summary>
        [Fact]
        public void Insert_ReturningKeyword_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "INSERT INTO `Person` (`Id`, `Name`, `SurName`) VALUES (1, 'Jens', 'Sels') RETURNING *".GetWithoutWhitespace().ToLower();
            var builder = MySql.Insert<Person>().Into().Column(x => x.Id).Column(x => x.Name).Column(x => x.SurName)
                            .Values(1, "Jens", "Sels")
                            .Returning(x => x.All());

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Insert for implicit on duplicate key update: builds expected query.</summary>
        [Fact]
        public void Insert_ImplicitOnDuplicateKeyUpdate_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "INSERT INTO `Person` (`Id`, `Name`, `SurName`) VALUES (1, 'Jens', 'Sels') ON DUPLICATE KEY UPDATE `Name`=VALUES(`Name`), `SurName`=VALUES(`SurName`)".GetWithoutWhitespace().ToLower();
            var builder = MySql.Insert<Person>().Into().Column(x => x.Id).Column(x => x.Name).Column(x => x.SurName)
                            .Values(1, "Jens", "Sels")
                            .OnDuplicateKeyUpdate(0);

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Insert for on duplicate key update: builds expected query.</summary>
        [Fact]
        public void Insert_OnDuplicateKeyUpdate_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "INSERT INTO `Person` (`Id`, `Name`, `SurName`) VALUES (1, 'Jens', 'Sels') ON DUPLICATE KEY UPDATE `Name`=VALUES(`Name`), `SurName`='Sels'".GetWithoutWhitespace().ToLower();
            var builder = MySql.Insert<Person>().Into().Column(x => x.Id).Column(x => x.Name).Column(x => x.SurName)
                            .Values(1, "Jens", "Sels")
                            .OnDuplicateKeyUpdate(x => x.Set.Column(x => x.Name).To.Values(x => x.Name).And.Set.Column(x => x.SurName).To.Value("Sels"));

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for all columns: builds expected query.</summary>
        [Fact]
        public void Select_AllColumns_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT * FROM `Person` P".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().All().From();

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for distinct keyword: builds expected query.</summary>
        [Fact]
        public void Select_DistinctKeyword_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT DISTINCT P.`SurName` FROM `Person` P".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().Distinct().Column(x => x.SurName).From();

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for limit keyword: builds expected query.</summary>
        [Fact]
        public void Select_LimitKeyword_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT * FROM `Person` P WHERE P.`Id` = 52 LIMIT 1".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().All().From().Where(w => w.Column(x => x.Id).EqualTo.Value(52)).Limit(1);

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for limit keyword with offset: builds expected query.</summary>
        [Fact]
        public void Select_LimitKeywordWithOffset_BuildsExpectedQuery()
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
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for for update keyword: builds expected query.</summary>
        [Fact]
        public void Select_ForUpdateKeyword_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT @Id = P.Id FROM `Person` P LIMIT 1 FOR UPDATE".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>()
                                    .ColumnExpression("@Id = P.Id")
                                    .From()
                                    .Limit(1)
                                    .ForUpdate();

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for different columns: builds expected query.</summary>
        [Fact]
        public void Select_DifferentColumns_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT P.`Name` AS `FirstName`, P.`SurName` AS `FamilyName`, P.`BirthDay` FROM `Person` P".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().Column(x => x.Name).As("FirstName").Column(x => x.SurName).As("FamilyName").Column(x => x.BirthDay).From();

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for excluded columns: builds expected query.</summary>
        [Fact]
        public void Select_ExcludedColumns_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT P.`Id`, P.`Name`, P.`SurName`, P.`BirthDay` FROM `Person` P".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().ColumnsOf(nameof(Person.ResidenceId)).From();

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for constant values: builds expected query.</summary>
        [Fact]
        public void Select_ConstantValues_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT 1, 'Jens'".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select().Value(1).Value("Jens");

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for sub query: builds expected query.</summary>
        [Fact]
        public void Select_SubQuery_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT Q.`Name`, Q.`Amount` FROM (SELECT P.`Name`, Count(*) as `Amount` FROM `Person` P GROUP BY P.`Name`) Q WHERE Q.`Amount` > 1".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select().Column("Q", "Name").Column("Q", "Amount")
                                .FromQuery(MySql.Select<Person>().Column(x => x.Name).CountAll().As("Amount").From().GroupBy(x => x.Name), "Q")
                                .Where(x => x.Column("Q", "Amount").GreaterThan.Value(1));

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for order by: builds expected query.</summary>
        [Fact]
        public void Select_OrderBy_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT P.* FROM `Person` P ORDER BY P.`BirthDay` DESC, P.`SurName` ASC, P.`Name`".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().AllOf().From().OrderBy(x => x.BirthDay, SortOrders.Descending).OrderBy(x => x.SurName, SortOrders.Ascending).OrderBy(x => x.Name);

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for group by: builds expected query.</summary>
        [Fact]
        public void Select_GroupBy_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT P.`Name`, Count(*) as `Amount` FROM `Person` P GROUP BY P.`Name`".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().Column(x => x.Name).CountAll().As("Amount").From().GroupBy(x => x.Name);

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for having: builds expected query.</summary>
        [Fact]
        public void Select_Having_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT P.`Name`, Count(*) as `Amount` FROM `Person` P GROUP BY P.`Name` HAVING Count(*) > 1 AND P.`Name` LIKE 'J%'".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().Column(x => x.Name).CountAll().As("Amount")
                               .From()
                               .GroupBy(x => x.Name)
                               .Having(h => h.CountAll().GreaterThan.Value(1)
                                         .And.Column(x => x.Name).Like.Value("J%")
                                      );

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for over clause with partition by: builds expected query.</summary>
        [Fact]
        public void Select_OverClauseWithPartitionBy_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT `Id`, ROW() OVER (PARTITION BY `ServerId`, `Resource`) FROM `Locks`".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select()
                                    .Column("Id")
                                    .AggregatedColumnExpression("ROW()").Over(o => o.PartitionBy(p => p.Column("ServerId"), p => p.Column("Resource")))
                                    .From("Locks");

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for over clause with order by and range between: builds expected query.</summary>
        [Fact]
        public void Select_OverClauseWithOrderByAndRangeBetween_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT `Id`, Count(*) OVER (ORDER BY `CreatedAt` ASC RANGE BETWEEN 1 FOLLOWING AND UNBOUNDED FOLLOWING) AS `PendingAfter` FROM `LockRequests`".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select()
                                    .Column("Id")
                                    .CountAll().Over(o => o.OrderBy("CreatedAt", SortOrders.Ascending).Range.Between.Following(1).And.UnboundedFollowing()).As("PendingAfter")
                                    .From("LockRequests");

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for row number function: builds expected query.</summary>
        [Fact]
        public void Select_RowNumberFunction_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT `Id`, ROW_NUMBER() OVER (PARTITION BY `ServerId`, `Resource` ORDER BY `CreatedAt` ASC) FROM `LockRequests`".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select()
                                    .Column("Id")
                                    .RowNumber().Over(o => o.PartitionBy("ServerId", "Resource").OrderBy("CreatedAt", SortOrders.Ascending))
                                    .From("LockRequests");

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for dense function: builds expected query.</summary>
        [Fact]
        public void Select_DenseFunction_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT `Id`, DENSE() OVER (PARTITION BY `ServerId`, `Resource` ORDER BY `CreatedAt` ASC) FROM `LockRequests`".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select()
                                    .Column("Id")
                                    .Dense().Over(o => o.PartitionBy("ServerId", "Resource").OrderBy("CreatedAt", SortOrders.Ascending))
                                    .From("LockRequests");

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for dense rank function: builds expected query.</summary>
        [Fact]
        public void Select_DenseRankFunction_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT `Id`, DENSE_RANK() OVER (PARTITION BY `ServerId`, `Resource` ORDER BY `CreatedAt` ASC) FROM `LockRequests`".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select()
                                    .Column("Id")
                                    .DenseRank().Over(o => o.PartitionBy("ServerId", "Resource").OrderBy("CreatedAt", SortOrders.Ascending))
                                    .From("LockRequests");

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for ntile function: builds expected query.</summary>
        [Fact]
        public void Select_NtileFunction_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT `Id`, NTILE(4) OVER (PARTITION BY `CreatedAt`) FROM `LockRequests`".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select()
                                    .Column("Id")
                                    .Ntile(4).Over(o => o.PartitionBy("CreatedAt"))
                                    .From("LockRequests");

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for lag function: builds expected query.</summary>
        [Fact]
        public void Select_LagFunction_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT `Id`, LAG(`CreatedAt`, 1) OVER (PARTITION BY `Resource` ORDER BY `CreatedAt` DESC) FROM `LockRequests`".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select()
                                    .Column("Id")
                                    .Lag("CreatedAt", 1).Over(o => o.PartitionBy("Resource").OrderBy("CreatedAt", SortOrders.Descending))
                                    .From("LockRequests");

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for lead function: builds expected query.</summary>
        [Fact]
        public void Select_LeadFunction_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT `Id`, LEAD(`CreatedAt`, 1) OVER (PARTITION BY `Resource` ORDER BY `CreatedAt` DESC) FROM `LockRequests`".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select()
                                    .Column("Id")
                                    .Lead("CreatedAt", 1).Over(o => o.PartitionBy("Resource").OrderBy("CreatedAt", SortOrders.Descending))
                                    .From("LockRequests");

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for join: builds expected query.</summary>
        [Fact]
        public void Select_Join_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT R.* FROM `Residence` R INNER JOIN `Person` P ON P.`ResidenceId` = R.`Id` WHERE P.`Id` = @Id".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Residence>().AllOf<Residence>().From()
                                .InnerJoin().Table<Person>().On(x => x.Column<Person>(c => c.ResidenceId).EqualTo.Column(c => c.Id))
                                .Where(w => w.Column<Person>(x => x.Id).EqualTo.Parameter(x => x.Id));

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for conditions: builds expected query.</summary>
        [Fact]
        public void Select_Conditions_BuildsExpectedQuery()
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
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for exists condition: builds expected query.</summary>
        [Fact]
        public void Select_ExistsCondition_BuildsExpectedQuery()
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
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for in sub query condition: builds expected query.</summary>
        [Fact]
        public void Select_InSubQueryCondition_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT * From `Person` P WHERE P.`ResidenceId` IN (SELECT R.`Id` FROM `Residence` R WHERE R.`Id` <= 100)".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().All().From()
                                .Where(x => x.Column(x => x.ResidenceId).In
                                             .Query(MySql.Select<Residence>().Column(x => x.Id).From().Where(x => x.Column(x => x.Id).LesserOrEqualTo.Value(100))));

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for union: builds expected query.</summary>
        [Fact]
        public void Select_Union_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT COUNT(P.`Name`) As `Amount` FROM `Person` P UNION SELECT COUNT(R.`Street`) as `Amount` FROM `Residence` R".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>().Count(x => x.Name).As("Amount").From()
                            .Union(MySql.Select<Residence>().Count(x => x.Street).As("Amount").From());

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for concat function: builds expected query.</summary>
        [Fact]
        public void Select_ConcatFunction_BuildsExpectedQuery()
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
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for case when condition: builds expected query.</summary>
        [Fact]
        public void Select_CaseWhenCondition_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT (CASE WHEN P.`ProductCategory` = 'NSFW' THEN 1 ELSE 0 END) AS `Hidden` FROM `Products` P".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select()
                                   .Case(x => x.When(w => w.Column("P", "ProductCategory").EqualTo.Value("NSFW")).Then.Value(1)
                                               .Else.Value(0)).As("Hidden")
                                .From("Products", datasetAlias: "P");

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for case raw condition: builds expected query.</summary>
        [Fact]
        public void Select_CaseRawCondition_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT (CASE WHEN P.`Id` = MAX(P.`Id`) THEN 1 ELSE 0 END) AS `IsLast` FROM `Products` P".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select()
                                   .Case(x => x.When("P.`Id` = MAX(P.`Id`)").Then.Value(1)
                                               .Else.Value(0)).As("IsLast")
                                .From("Products", datasetAlias: "P");

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for order by case: builds expected query.</summary>
        [Fact]
        public void Select_OrderByCase_BuildsExpectedQuery()
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
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for variable assignment: builds expected query.</summary>
        [Fact]
        public void Select_VariableAssignment_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT @Id := P.`Id` FROM `Person` P LIMIT 1".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select<Person>()
                                 .ColumnExpression(b => b.AssignVariable("Id", v => v.Column(c => c.Id)))
                               .From()
                               .Limit(1);

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for variable: builds expected query.</summary>
        [Fact]
        public void Select_Variable_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT @Id".GetWithoutWhitespace().ToLower();
            var builder = MySql.Select().Variable("Id");

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for current date with server type: builds expected query.</summary>
        [Fact]
        public void Select_CurrentDateWithServerType_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT NOW(6)".GetWithoutWhitespace().ToLower();

            // Act
            var query = MySql.Select().ColumnExpression(b => b.CurrentDate(DateType.Server)).Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for current date with utc type: builds expected query.</summary>
        [Fact]
        public void Select_CurrentDateWithUtcType_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "SELECT UTC_TIMESTAMP(6)".GetWithoutWhitespace().ToLower();

            // Act
            var query = MySql.Select().ColumnExpression(b => b.CurrentDate(DateType.Utc)).Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Select for modify date interval: builds expected query.</summary>
        [Theory]
        [InlineData(1, DateInterval.Minute, "1", "MINUTE")]
        [InlineData(-1500, DateInterval.Millisecond, "-1500 * 1000", "MICROSECOND")]
        [InlineData(24, DateInterval.Month, "24", "MONTH")]
        [InlineData(1.5, DateInterval.Hour, "1.5", "HOUR")]
        public void Select_ModifyDateInterval_BuildsExpectedQuery(double amount, DateInterval interval, string expectedAmount, string expectedInterval)
        {
            // Arrange
            var expected = $"SELECT DATE_ADD(NOW(6), INTERVAL {expectedAmount} {expectedInterval})".GetWithoutWhitespace().ToLower();

            // Act
            var query = MySql.Select().ColumnExpression(b => b.ModifyDate(b => b.CurrentDate(DateType.Server), amount, interval)).Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Set for constant assignment: builds expected query.</summary>
        [Theory]
        [InlineData("Hello", "'Hello'")]
        [InlineData(2, "2")]
        [InlineData(2.2d, "2.2")]
        public void Set_ConstantAssignment_BuildsExpectedQuery(object constant, string expectedConstant)
        {
            // Arrange
            var expected = $"SET @Variable = {expectedConstant};".GetWithoutWhitespace().ToLower();

            // Act
            var query = MySql.Set().Variable("Variable").To.Value(constant).Build(ExpressionCompileOptions.AppendSeparator);

            // Assert
            Assert.Equal(expected, query?.GetWithoutWhitespace()?.ToLower());
        }

        /// <summary>Verifies Set for sub query: builds expected query.</summary>
        [Fact]
        public void Set_SubQuery_BuildsExpectedQuery()
        {
            // Arrange
            var expected = $"SET @Id = (SELECT `Id` FROM `Queues` ORDER BY `CreatedAt` ASC LIMIT 1 FOR UPDATE);".GetWithoutWhitespace().ToLower();

            // Act
            var query = MySql.Set().Variable("@Id").To.Query(MySql.Select().Column("Id").From("Queues").ForUpdate().OrderBy("CreatedAt", SortOrders.Ascending).Limit(1)).Build(ExpressionCompileOptions.AppendSeparator);

            // Assert
            Assert.Equal(expected, query?.GetWithoutWhitespace()?.ToLower());
        }

        /// <summary>Verifies Update for no options: builds expected query.</summary>
        [Fact]
        public void Update_NoOptions_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "UPDATE `Person` Set `Name` = 'Jens' WHERE `Name` = 'jens'".GetWithoutWhitespace().ToLower();
            var builder = MySql.Update().Table("Person")
                                    .Set.Column("Name").To.Value("Jens")
                                    .Where(x => x.Column("Name").EqualTo.Value("jens"));

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Update for join: builds expected query.</summary>
        [Fact]
        public void Update_Join_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "UPDATE `Person` P INNER JOIN `Residence` R ON P.`ResidenceId` = R.`Id` SET P.`BirthDay` = GETDATE(), R.`HouseNumber` = 78 WHERE R.`Id` = 1998".GetWithoutWhitespace().ToLower();
            var builder = MySql.Update<Person>().Table()
                                    .InnerJoin().Table<Residence>().On(x => x.Column(c => c.ResidenceId).EqualTo.Column<Residence>(c => c.Id))
                                    .Set.Column(x => x.BirthDay).To.Expression("GETDATE()")
                                    .Set.Column<Residence>(x => x.HouseNumber).To.Value(78)
                                    .Where(x => x.Column<Residence>(c => c.Id).EqualTo.Value(1998));

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Update for from object: builds expected query.</summary>
        [Fact]
        public void Update_FromObject_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "UPDATE `Person` P SET P.`Name` = 'Jens', P.`SurName` = 'Sels', P.`BirthDay` = '1998-01-04 00:13:45', P.`ResidenceId` = 90 WHERE P.`Id` = 1".GetWithoutWhitespace().ToLower();
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
                                    .Where(x => 
                                        x.Column(alias, nameof(Person.Id)).EqualTo.Value(1)
                                    );

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Update for from object using parameters: builds expected query.</summary>
        [Fact]
        public void Update_FromObjectUsingParameters_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "UPDATE `Person` P SET P.`Name` = @Name, P.`SurName` = @SurName, P.`BirthDay` = @BirthDay, P.`ResidenceId` = @ResidenceId WHERE P.`Id` = 1".GetWithoutWhitespace().ToLower();
            var builder = MySql.Update<Person>().Table()
                                    .SetFrom<Person>(excludedProperties: nameof(Person.Id))
                                    .Where(x => x.Column(typeof(Person), nameof(Person.Id)).EqualTo.Value(1));

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies Update for conditions: builds expected query.</summary>
        [Fact]
        public void Update_Conditions_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "UPDATE `Person` P SET P.`Name` = 'Jens' WHERE EXISTS (SELECT * FROM `Residence` R WHERE R.`PostalCode` BETWEEN 2500 AND 2599 AND R.`Id` > P.`ResidenceId`)".GetWithoutWhitespace().ToLower();
            var builder = MySql.Update<Person>().Table()
                                    .Set.Column(x => x.Name).To.Value("Jens")
                                    .Where(x => 
                                            x.ExistsIn(
                                                MySql.Select<Residence>().All().From()
                                                    .Where(w => w.Column(c => c.PostalCode).Between(2500, 2599).And
                                                    .Column(c => c.Id).GreaterThan.Column<Person>(x => x.ResidenceId)))
                                    );

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies With for single cte: builds expected query.</summary>
        [Fact]
        public void With_SingleCte_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "WITH `cte` AS ( SELECT * FROM `Person` P ) SELECT * FROM `cte`".GetWithoutWhitespace().ToLower();
            var builder = MySql.With()
                                .Cte("cte").As(MySql.Select<Person>().All().From())
                                .Execute(MySql.Select().All().From("cte"));

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies With for multiple ctes: builds expected query.</summary>
        [Fact]
        public void With_MultipleCtes_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "WITH `personCte` AS ( SELECT P.`Id` FROM `Person` P ), `residenceCte` AS ( SELECT R.`Id` FROM `Residence` R ) SELECT * FROM `personCte` UNION SELECT * FROM `residenceCte`".GetWithoutWhitespace().ToLower();
            var builder = MySql.With()
                                .Cte("personCte").As(MySql.Select<Person>().Column(c => c.Id).From())
                                .Cte("residenceCte").As(MySql.Select<Residence>().Column(c => c.Id).From())
                                .Execute(
                                    MySql.Select().All().From("personCte")
                                    .Union(
                                    MySql.Select().All().From("residenceCte"))
                                    );

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }

        /// <summary>Verifies With for recursive cte: builds expected query.</summary>
        [Fact]
        public void With_RecursiveCte_BuildsExpectedQuery()
        {
            // Arrange
            var expected = "WITH Recursive `cte` AS ( SELECT 1 as `n` UNION ALL SELECT n+1 FROM `cte` WHERE n < 20) SELECT * FROM `cte`".GetWithoutWhitespace().ToLower();
            var builder = MySql.With()
                                .RecursiveCte("cte").As(
                                    MySql.Select().Value(1).As("n")
                                    .UnionAll(
                                    MySql.Select().ColumnExpression("n+1").From("cte").Where(x => x.Expression("n").LesserThan.Value(20))    
                                    ))                                   
                                .Execute(
                                    MySql.Select().All().From("cte")
                                    );

            // Act
            var query = builder.Build();

            // Assert
            Assert.NotNull(query);
            Assert.Equal(expected, query.GetWithoutWhitespace().ToLower());
        }
    }
}
