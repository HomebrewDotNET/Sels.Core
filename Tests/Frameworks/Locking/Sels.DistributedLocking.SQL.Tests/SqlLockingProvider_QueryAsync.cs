using Microsoft.VisualStudio.TestPlatform.ObjectModel.DataCollection;
using Sels.Core.Extensions.Linq;
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
        [Test]
        public async Task CorrectParametersArePassedToLockRepository()
        {
            // Arrange
            var options = TestHelper.GetProviderOptionsMock();
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
            });
            await using var provider = new SqlLockingProvider(repositoryMock.Object, options);

            // Act
            var results = await provider.QueryAsync("Deployment.", 2, 50, x => x.LastLockDate, true);

            // Assert
            repositoryMock.Verify(x => x.SearchAsync(It.IsAny<IRepositoryTransaction>(), "Deployment.", 2, 50, It.Is<PropertyInfo>(x => x.Name == nameof(SqlLock.LastLockDate)), true, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
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
                    ExpiryDate = DateTimeOffset.Now.AddMinutes(5),
                    PendingRequests = 2,
                    LastLockDate = DateTimeOffset.Now,
                    LockedAt = DateTimeOffset.Now,
                }
            };
            var repositoryMock = TestHelper.GetRepositoryMock(x =>
            {
                x.Setup(x => x.SearchAsync(It.IsAny<IRepositoryTransaction>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<PropertyInfo>(), It.IsAny<bool>(), It.IsAny<CancellationToken>())).ReturnsAsync(sqlLocks);
            });
            await using var provider = new SqlLockingProvider(repositoryMock.Object, options);

            // Act
            var results = await provider.QueryAsync();

            // Assert
            Assert.That(results, Is.Not.Null);
            Assert.That(results.Length, Is.EqualTo(sqlLocks.Length));
            for(int i = 0; i < sqlLocks.Length; i++)
            {
                Assert.That(results[i].Resource, Is.EqualTo(sqlLocks[i].Resource));
                Assert.That(results[i].LockedBy, Is.EqualTo(sqlLocks[i].LockedBy));
                Assert.That(results[i].ExpiryDate, Is.EqualTo(sqlLocks[i].ExpiryDate));
                Assert.That(results[i].LockedAt, Is.EqualTo(sqlLocks[i].LockedAt));
                Assert.That(results[i].LastLockDate, Is.EqualTo(sqlLocks[i].LastLockDate));
                Assert.That(results[i].PendingRequests, Is.EqualTo(sqlLocks[i].PendingRequests));
            }
        }
    }
}
