using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Linq;
using Sels.DistributedLocking.Abstractions.Models;
using Sels.DistributedLocking.SQL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.Memory.Test
{
    public class SqlLockingProvider_QueryAsync
    {
        [Test, Timeout(10000)]
        public async Task CorrectParametersArePassedToLockRepository()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
            });
            await using var provider = new SqlLockingProvider(repositoryMock.Object, options);

            // Act
            var results = await provider.QueryAsync(x => x.WithFilterOnResource("Deployment")
                                                          .WithPagination(2, 50)
                                                          .OrderByLastLockDate(true));

            // Assert
            repositoryMock.Verify(x => x.SearchAsync(It.IsAny<IRepositoryTransaction>(),
                                                     It.Is<SqlQuerySearchCriteria>(x => x.Filters.Any(f => f.Column.Equals("Resource") && f.Filter.Equals("Deployment") && !f.IsFullMatch)
                                                                                        && x.Pagination.HasValue && x.Pagination.Value.Page == 2 && x.Pagination.Value.PageSize == 50
                                                                                        && x.SortColumns.Any(s => s.Column.Equals("LastLockDate") && s.SortDescending)), 
                                                     It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test, Timeout(10000)]
        public async Task ReturnsCorrectLocks()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            var sqlLocks = new SqlLock[]
            {
                new SqlLock()
                {
                    Resource = "Resource.1",
                    LockedBy = "Requester.1"
                },
                 new SqlLock()
                {
                    Resource = "Resource.2",
                    LockedBy = "Requester.2",
                    ExpiryDate = DateTime.Now.AddMinutes(5),
                    PendingRequests = 2,
                    LastLockDate = DateTime.Now,
                    LockedAt = DateTime.Now,
                }
            };
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
                x.Setup(x => x.SearchAsync(It.IsAny<IRepositoryTransaction>(), It.IsAny<SqlQuerySearchCriteria>(), It.IsAny<CancellationToken>())).ReturnsAsync((sqlLocks, sqlLocks.Length));
            });
            await using var provider = new SqlLockingProvider(repositoryMock.Object, options);

            // Act
            var result = await provider.QueryAsync(x => { });

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Results.Length, Is.EqualTo(sqlLocks.Length));
            for(int i = 0; i < sqlLocks.Length; i++)
            {
                Assert.That(result.Results[i].Resource, Is.EqualTo(sqlLocks[i].Resource));
                Assert.That(result.Results[i].LockedBy, Is.EqualTo(sqlLocks[i].LockedBy));
                Assert.That(result.Results[i].ExpiryDate, Is.EqualTo(sqlLocks[i].ExpiryDate));
                Assert.That(result.Results[i].LockedAt, Is.EqualTo(sqlLocks[i].LockedAt));
                Assert.That(result.Results[i].LastLockDate, Is.EqualTo(sqlLocks[i].LastLockDate));
                Assert.That(result.Results[i].PendingRequests, Is.EqualTo(sqlLocks[i].PendingRequests));
            }
        }
    }
}
