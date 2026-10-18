using Sels.Core;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Linq;
using Sels.DistributedLocking.Abstractions.Models;
using Sels.DistributedLocking.Provider;
using System.Reflection;
using System.Text;

namespace Sels.DistributedLocking.SQL.Tests
{
    /// <summary>Contract tests for <see cref="SqlLockingProvider"/>.</summary>
    [Trait("Category", "Unit")]
    [Collection(DistributedLockingSqlTestCollection.Name)]
    public class SqlLockingProviderTests
    {
        /// <summary>Verifies GetAsync for locked resource: returns lock state.</summary>
        [Fact(Timeout = DistributedLockingSqlTestConstants.TestTimeoutMilliseconds)]
        public async Task GetAsync_LockedResource_ReturnsLockState()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            var date = DateTime.Now.AddMinutes(5);
            var now = DateTime.Now;
            var resource = "Resource";
            var requester = "Me";
            var sqlLock = new SqlLock()
            {
                Resource = resource,
                LockedBy = requester,
                ExpiryDate = date,
                LastLockDate = now,
                LockedAt = now,
                PendingRequests = 0
            };
            var repository = TestHelper.CreateRepository(x =>
            {
                x.TryLockAsync(Arg.Any<IRepositoryTransaction>(), resource, requester, Arg.Any<DateTime?>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(sqlLock));
                x.GetLockByResourceAsync(Arg.Any<IRepositoryTransaction>(), resource, true, Arg.Any<CancellationToken>()).Returns(Task.FromResult(sqlLock));
            });
            
            await using var provider = new SqlLockingProvider(TestHelper.CreateNotifier(), TestHelper.CreateSubscriber(), TestHelper.CreateTaskManager(), repository, options);

            // Act
            var lockResult = await provider.TryLockAsync(resource, requester, TimeSpan.FromMinutes(5));
            var result = await provider.GetAsync(resource);

            // Assert
            Assert.NotNull(lockResult);
            Assert.NotNull(result);
            Assert.Equal(lockResult.AcquiredLock.Resource, result.Resource);
            Assert.Equal(lockResult.AcquiredLock.LockedBy, result.LockedBy);
            Assert.Equal(lockResult.AcquiredLock.LockedAt, result.LockedAt);
            Assert.Equal(lockResult.AcquiredLock.ExpiryDate, result.ExpiryDate);
            Assert.Equal(lockResult.AcquiredLock.LastLockDate, result.LastLockDate);
            Assert.Equal(lockResult.AcquiredLock.PendingRequests, result.PendingRequests);
        }

        /// <summary>Verifies GetAsync for unknown resource: returns free lock.</summary>
        [Fact(Timeout = DistributedLockingSqlTestConstants.TestTimeoutMilliseconds)]
        public async Task GetAsync_UnknownResource_ReturnsFreeLock()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            var repository = TestHelper.CreateRepository(x =>
            {
                x.GetLockByResourceAsync(Arg.Any<IRepositoryTransaction>(), "Resource", true, default).Returns(Task.FromResult<SqlLock?>(null));
            });
            await using var provider = new SqlLockingProvider(TestHelper.CreateNotifier(), TestHelper.CreateSubscriber(), TestHelper.CreateTaskManager(), repository, options);

            // Act
            var result = await provider.GetAsync("Resource");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Resource", result.Resource);
            Assert.Null(result.LockedBy);
            Assert.Null(result.LockedAt);
            Assert.Null(result.ExpiryDate);
            Assert.Null(result.LastLockDate);
            Assert.Equal(0, result.PendingRequests);
        }

        /// <summary>Verifies GetPendingRequestsAsync for pending requests: returns all requests.</summary>
        [Fact(Timeout = DistributedLockingSqlTestConstants.TestTimeoutMilliseconds)]
        public async Task GetPendingRequestsAsync_PendingRequests_ReturnsAllRequests()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            var resource = "Resource";
            var sqlRequests = new SqlLockRequest[]
            {
                new SqlLockRequest()
                {
                    Id = 1,
                    Resource = resource,
                    Requester = "Requester.1",
                    ExpiryTime = TimeSpan.FromMinutes(5).TotalSeconds,
                    KeepAlive = false,
                    Timeout = null,
                    CreatedAt = DateTime.Now.AddMinutes(-2)
                },
                new SqlLockRequest()
                {
                    Id = 2,
                    Resource = resource,
                    Requester = "Requester.2",
                    ExpiryTime = null,
                    KeepAlive = true,
                    Timeout = null,
                    CreatedAt = DateTime.Now.AddMinutes(-1)
                },
                new SqlLockRequest()
                {
                    Id = 3,
                    Resource = resource,
                    Requester = "Requester.3",
                    ExpiryTime = null,
                    KeepAlive = false,
                    Timeout = DateTime.Now.AddMilliseconds(1000),
                    CreatedAt = DateTime.Now
                }
            };
            var repository = TestHelper.CreateRepository(x =>
            {
                x.GetAllLockRequestsByResourceAsync(Arg.Any<IRepositoryTransaction>(), resource, Arg.Any<CancellationToken>()).Returns(Task.FromResult(sqlRequests));
            });

            await using var provider = new SqlLockingProvider(TestHelper.CreateNotifier(), TestHelper.CreateSubscriber(), TestHelper.CreateTaskManager(), repository, options);

            // Act
            var pendingRequests = await provider.GetPendingRequestsAsync(resource);

            // Assert
            Assert.NotNull(pendingRequests);
            Assert.Equal(sqlRequests.Length, pendingRequests.Length);
            for (int i = 0; i < pendingRequests.Length; i++)
            {
                Assert.NotNull(pendingRequests[i]);
                Assert.Equal(sqlRequests[i].Resource, pendingRequests[i].Resource);
                Assert.Equal(sqlRequests[i].Requester, pendingRequests[i].Requester);
                Assert.Equal(sqlRequests[i].ExpiryTime.HasValue ? TimeSpan.FromSeconds(sqlRequests[i].ExpiryTime.Value) : null, pendingRequests[i].ExpiryTime);
                Assert.Equal(sqlRequests[i].Timeout, pendingRequests[i].Timeout);
                Assert.Equal(sqlRequests[i].KeepAlive, pendingRequests[i].KeepAlive);
                Assert.Equal(sqlRequests[i].CreatedAt, pendingRequests[i].CreatedAt);
            }
        }

        /// <summary>Verifies GetPendingRequestsAsync for no pending requests: returns empty array.</summary>
        [Fact(Timeout = DistributedLockingSqlTestConstants.TestTimeoutMilliseconds)]
        public async Task GetPendingRequestsAsync_NoPendingRequests_ReturnsEmptyArray()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            var resource = "Resource";
            var repository = TestHelper.CreateRepository(x =>
            {
                x.GetAllLockRequestsByResourceAsync(Arg.Any<IRepositoryTransaction>(), resource, Arg.Any<CancellationToken>()).Returns(Task.FromResult(Array.Empty<SqlLockRequest>()));
            });

            await using var provider = new SqlLockingProvider(TestHelper.CreateNotifier(), TestHelper.CreateSubscriber(), TestHelper.CreateTaskManager(), repository, options);

            // Act
            var pendingRequests = await provider.GetPendingRequestsAsync(resource);

            // Assert
            Assert.NotNull(pendingRequests);
            Assert.Equal(0, pendingRequests.Length);
        }

        /// <summary>Verifies GetPendingRequestsAsync for unknown resource: returns empty array.</summary>
        [Fact(Timeout = DistributedLockingSqlTestConstants.TestTimeoutMilliseconds)]
        public async Task GetPendingRequestsAsync_UnknownResource_ReturnsEmptyArray()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            var repository = TestHelper.CreateRepository(x =>
            {
            });
            await using var provider = new SqlLockingProvider(TestHelper.CreateNotifier(), TestHelper.CreateSubscriber(), TestHelper.CreateTaskManager(), repository, options);

            // Act
            var pendingRequests = await provider.GetPendingRequestsAsync("NonExistant");

            // Assert
            Assert.NotNull(pendingRequests);
            Assert.Equal(0, pendingRequests.Length);
        }

        /// <summary>Verifies LockAsync for free resource: locks for requester.</summary>
        [Theory(Timeout = DistributedLockingSqlTestConstants.TestTimeoutMilliseconds)]
        [InlineData("Jens")]
        [InlineData("Thread.1")]
        [InlineData("Database.Deployer")]
        [InlineData("BackgroundJob.56")]
        [InlineData("sels.homebrewit.csautomatron")]
        public async Task LockAsync_FreeResource_LocksForRequester(string requester)
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            var resource = "Resource";
            var sqlLock = new SqlLock()
            {
                Resource = resource,
                LockedBy = requester,
                LockedAt = DateTime.Now,
                LastLockDate = DateTime.Now
            };
            var repository = TestHelper.CreateRepository(x =>
            {
                x.TryLockAsync(Arg.Any<IRepositoryTransaction>(), resource, requester, Arg.Any<DateTime?>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(sqlLock));
            });
            await using var provider = new SqlLockingProvider(TestHelper.CreateNotifier(), TestHelper.CreateSubscriber(), TestHelper.CreateTaskManager(), repository, options);
            var lockingProvider = provider.CastTo<ILockingProvider>();

            // Act
            var @lock = await lockingProvider.LockAndWaitAsync(resource, requester);

            // Assert
            Assert.NotNull(@lock);
            Assert.Equal(resource, @lock.Resource);
            Assert.Equal(requester, @lock.LockedBy);
            Assert.Null(@lock.ExpiryDate);
        }

        /// <summary>Verifies LockAsync for locked resource: places request.</summary>
        [Theory(Timeout = DistributedLockingSqlTestConstants.TestTimeoutMilliseconds)]
        [InlineData("Jens")]
        [InlineData("Thread.1")]
        [InlineData("Database.Deployer")]
        [InlineData("BackgroundJob.56")]
        [InlineData("sels.homebrewit.csautomatron")]
        public async Task LockAsync_LockedResource_PlacesRequest(string requester)
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            var resource = "Resource";
            var sqlLock = new SqlLock()
            {
                Resource = resource,
                LockedBy = "Requester",
                LockedAt = DateTime.Now,
                LastLockDate = DateTime.Now
            };
            var sqlRequest = new SqlLockRequest()
            {
                Id = 1,
                Resource = resource,
                Requester = requester,
                CreatedAt = DateTime.Now,
                Timeout = DateTime.Now
            };
            var repository = TestHelper.CreateRepository(x =>
            {
                x.TryLockAsync(Arg.Any<IRepositoryTransaction>(), resource, requester, Arg.Any<DateTime?>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(sqlLock));
                x.CreateRequestAsync(Arg.Any<IRepositoryTransaction>(), Arg.Any<SqlLockRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(sqlRequest));
            });
            await using var provider = new SqlLockingProvider(TestHelper.CreateNotifier(), TestHelper.CreateSubscriber(), TestHelper.CreateTaskManager(), repository, options);
            var lockingProvider = provider.CastTo<ILockingProvider>();

            // Act
            var lockTask = lockingProvider.LockAndWaitAsync(resource, requester);

            // Assert
            Assert.NotNull(lockTask);
            Assert.False(lockTask.IsCompleted);
            _ = repository.Received(1).CreateRequestAsync(Arg.Any<IRepositoryTransaction>(), Arg.Is<SqlLockRequest>(x => x.Resource.Equals(resource) && x.Requester.Equals(requester)), Arg.Any<CancellationToken>());
        }

        /// <summary>Verifies LockAsync for request timeout elapsed: times out request.</summary>
        [Fact(Timeout = DistributedLockingSqlTestConstants.TestTimeoutMilliseconds)]
        public async Task LockAsync_RequestTimeoutElapsed_TimesOutRequest()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            var resource = "Resource";
            var holder = "Holder";
            var requester = "Requester";
            var sqlLock = new SqlLock()
            {
                Resource = resource,
                LockedBy = holder,
                LockedAt = DateTime.Now,
                LastLockDate = DateTime.Now,
                ExpiryDate = null,
            };
            var sqlRequest = new SqlLockRequest()
            {
                Id = 1,
                Resource = resource,
                Requester = requester,
                CreatedAt = DateTime.Now,
                Timeout = DateTime.Now
            };
            var repository = TestHelper.CreateRepository(x =>
            {
                x.TryLockAsync(Arg.Any<IRepositoryTransaction>(), resource, requester, Arg.Any<DateTime?>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(sqlLock));
                x.GetLockByResourceAsync(Arg.Any<IRepositoryTransaction>(), resource, false, Arg.Any<CancellationToken>()).Returns(Task.FromResult(sqlLock));
                x.CreateRequestAsync(Arg.Any<IRepositoryTransaction>(), Arg.Any<SqlLockRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(sqlRequest));
                x.DeleteAllRequestsById(Arg.Any<IRepositoryTransaction>(), Arg.Any<long[]>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            });
            await using var provider = new SqlLockingProvider(TestHelper.CreateNotifier(), TestHelper.CreateSubscriber(), TestHelper.CreateTaskManager(), repository, options);
            var lockingProvider = provider.CastTo<ILockingProvider>();
            Exception exception = null;

            // Act
            var lockResultTask = lockingProvider.LockAndWaitAsync(resource, requester, timeout: TimeSpan.FromMilliseconds(0));
            try
            {
                await lockResultTask;
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<LockTimeoutException>(exception);
            var timeoutException = exception as LockTimeoutException;
            Assert.NotNull(timeoutException);
            Assert.Equal(sqlLock.Resource, timeoutException.Resource);
            Assert.Equal(sqlRequest.Requester, timeoutException.Requester);
            Assert.Equal(TimeSpan.FromMilliseconds(0), timeoutException.Timeout);
        }

        /// <summary>Verifies LockAsync for token cancelled: throws operation canceled exception.</summary>
        [Fact(Timeout = DistributedLockingSqlTestConstants.TestTimeoutMilliseconds)]
        public async Task LockAsync_TokenCancelled_ThrowsOperationCanceledException()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            var resource = "Resource";
            var holder = "Holder";
            var requester = "Requester";
            var sqlLock = new SqlLock()
            {
                Resource = resource,
                LockedBy = holder,
                LockedAt = DateTime.Now,
                LastLockDate = DateTime.Now,
                ExpiryDate = null,
            };
            var sqlRequest = new SqlLockRequest()
            {
                Id = 1,
                Resource = resource,
                Requester = requester,
                CreatedAt = DateTime.Now,
                Timeout = null
            };
            var repository = TestHelper.CreateRepository(x =>
            {
                x.TryLockAsync(Arg.Any<IRepositoryTransaction>(), resource, requester, Arg.Any<DateTime?>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(sqlLock));
                x.GetLockByResourceAsync(Arg.Any<IRepositoryTransaction>(), resource, false, Arg.Any<CancellationToken>()).Returns(Task.FromResult(sqlLock));
                x.CreateRequestAsync(Arg.Any<IRepositoryTransaction>(), Arg.Any<SqlLockRequest>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(sqlRequest));
                x.DeleteAllRequestsById(Arg.Any<IRepositoryTransaction>(), Arg.Any<long[]>(), Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);
            });
            await using var provider = new SqlLockingProvider(TestHelper.CreateNotifier(), TestHelper.CreateSubscriber(), TestHelper.CreateTaskManager(), repository, options, null);
            var lockingProvider = provider.CastTo<ILockingProvider>();
            Exception exception = null;
            var tokenSource = new CancellationTokenSource();

            // Act
            var lockResultTask = lockingProvider.LockAndWaitAsync(sqlRequest.Resource, sqlRequest.Requester, token: tokenSource.Token);
            tokenSource.Cancel();
            await Helper.Async.Sleep(100 * 2);
            if (lockResultTask.IsCompleted)
            {
                try
                {
                    await lockResultTask;
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            }

            // Assert
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<OperationCanceledException>(exception);
        }

        /// <summary>Verifies QueryAsync for filter pagination and sort: passes criteria to repository.</summary>
        [Fact(Timeout = DistributedLockingSqlTestConstants.TestTimeoutMilliseconds)]
        public async Task QueryAsync_FilterPaginationAndSort_PassesCriteriaToRepository()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            var repository = TestHelper.CreateRepository(x =>
            {
                x.SearchAsync(Arg.Any<IRepositoryTransaction>(), Arg.Any<SqlQuerySearchCriteria>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult((Array.Empty<SqlLock>(), 0)));
            });
            await using var provider = new SqlLockingProvider(TestHelper.CreateNotifier(), TestHelper.CreateSubscriber(), TestHelper.CreateTaskManager(), repository, options);

            // Act
            var results = await provider.QueryAsync(x => x.WithFilterOnResource("Deployment")
                                                          .WithPagination(2, 50)
                                                          .OrderByLastLockDate(true));

            // Assert
            _ = repository.Received(1).SearchAsync(Arg.Any<IRepositoryTransaction>(),
                                                     Arg.Is<SqlQuerySearchCriteria>(x => x.Filters.Any(f => f.Column.Equals("Resource") && f.Filter.Equals("Deployment") && !f.IsFullMatch)
                                                                                        && x.Pagination.HasValue && x.Pagination.Value.Page == 2 && x.Pagination.Value.PageSize == 50
                                                                                        && x.SortColumns.Any(s => s.Column.Equals("LastLockDate") && s.SortDescending)), 
                                                     Arg.Any<CancellationToken>());
        }

        /// <summary>Verifies QueryAsync for repository results: returns mapped locks.</summary>
        [Fact(Timeout = DistributedLockingSqlTestConstants.TestTimeoutMilliseconds)]
        public async Task QueryAsync_RepositoryResults_ReturnsMappedLocks()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
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
            var repository = TestHelper.CreateRepository(x =>
            {
                x.SearchAsync(Arg.Any<IRepositoryTransaction>(), Arg.Any<SqlQuerySearchCriteria>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult((sqlLocks, sqlLocks.Length)));
            });
            await using var provider = new SqlLockingProvider(TestHelper.CreateNotifier(), TestHelper.CreateSubscriber(), TestHelper.CreateTaskManager(), repository, options);

            // Act
            var result = await provider.QueryAsync(x => { });

            // Assert
            Assert.NotNull(result);
            Assert.Equal(sqlLocks.Length, result.Results.Length);
            for(int i = 0; i < sqlLocks.Length; i++)
            {
                Assert.Equal(sqlLocks[i].Resource, result.Results[i].Resource);
                Assert.Equal(sqlLocks[i].LockedBy, result.Results[i].LockedBy);
                Assert.Equal(sqlLocks[i].ExpiryDate, result.Results[i].ExpiryDate);
                Assert.Equal(sqlLocks[i].LockedAt, result.Results[i].LockedAt);
                Assert.Equal(sqlLocks[i].LastLockDate, result.Results[i].LastLockDate);
                Assert.Equal(sqlLocks[i].PendingRequests, result.Results[i].PendingRequests);
            }
        }

        /// <summary>Verifies TryLockAsync for free resource: locks for requester.</summary>
        [Theory(Timeout = DistributedLockingSqlTestConstants.TestTimeoutMilliseconds)]
        [InlineData("Jens")]
        [InlineData("Thread.1")]
        [InlineData("Database.Deployer")]
        [InlineData("BackgroundJob.56")]
        [InlineData("sels.homebrewit.csautomatron")]
        public async Task TryLockAsync_FreeResource_LocksForRequester(string requester)
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            var resource = "Resource";
            var sqlLock = new SqlLock()
            {
                Resource = resource,
                LockedBy = requester,
                LockedAt = DateTime.Now,
                LastLockDate = DateTime.Now
            };
            var repository = TestHelper.CreateRepository(x =>
            {
                x.TryLockAsync(Arg.Any<IRepositoryTransaction>(), resource, requester, Arg.Any<DateTime?>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(sqlLock));
            });
            await using var provider = new SqlLockingProvider(TestHelper.CreateNotifier(), TestHelper.CreateSubscriber(), TestHelper.CreateTaskManager(), repository, options);

            // Act
            var lockResult = await provider.TryLockAsync(resource, requester);

            // Assert
            Assert.True(lockResult.Success);
            Assert.NotNull(requester);
            Assert.NotNull(lockResult.AcquiredLock);
            Assert.Equal(resource, lockResult.AcquiredLock.Resource);
            Assert.Equal(requester, lockResult.AcquiredLock.LockedBy);
            Assert.Null(lockResult.AcquiredLock.ExpiryDate);
        }

        /// <summary>Verifies TryLockAsync for resource locked by other: returns failure.</summary>
        [Fact(Timeout = DistributedLockingSqlTestConstants.TestTimeoutMilliseconds)]
        public async Task TryLockAsync_ResourceLockedByOther_ReturnsFailure()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            var resource = "Resource";
            var holder = "Holder";
            var requester = "Requester";
            var sqlLock = new SqlLock()
            {
                Resource = resource,
                LockedBy = holder,
                LockedAt = DateTime.Now,
                LastLockDate = DateTime.Now
            };
            var repository = TestHelper.CreateRepository(x =>
            {
                x.TryLockAsync(Arg.Any<IRepositoryTransaction>(), resource, Arg.Any<string>(), Arg.Any<DateTime?>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(sqlLock));
            });
            await using var provider = new SqlLockingProvider(TestHelper.CreateNotifier(), TestHelper.CreateSubscriber(), TestHelper.CreateTaskManager(), repository, options);

            // Act
            var lockResult = await provider.TryLockAsync(resource, holder);
            var lockResultTwo = await provider.TryLockAsync(resource, requester);

            // Assert
            Assert.True(lockResult.Success);
            Assert.NotNull(lockResult.AcquiredLock);
            Assert.False(lockResultTwo.Success);
            Assert.Null(lockResultTwo.AcquiredLock);
        }

        /// <summary>Verifies TryLockAsync for requester already holds lock: returns success.</summary>
        [Fact(Timeout = DistributedLockingSqlTestConstants.TestTimeoutMilliseconds)]
        public async Task TryLockAsync_RequesterAlreadyHoldsLock_ReturnsSuccess()
        {
            // Arrange
            var options = TestHelper.CreateProviderOptions();
            var resource = "Resource";
            var requester = "Requester";
            var sqlLock = new SqlLock()
            {
                Resource = resource,
                LockedBy = requester,
                LockedAt = DateTime.Now,
                LastLockDate = DateTime.Now
            };
            var repository = TestHelper.CreateRepository(x =>
            {
                x.TryLockAsync(Arg.Any<IRepositoryTransaction>(), resource, requester, Arg.Any<DateTime?>(), Arg.Any<CancellationToken>()).Returns(Task.FromResult(sqlLock));
            });
            await using var provider = new SqlLockingProvider(TestHelper.CreateNotifier(), TestHelper.CreateSubscriber(), TestHelper.CreateTaskManager(), repository, options);

            // Act
            var lockResult = await provider.TryLockAsync(resource, requester);
            var lockResultTwo = await provider.TryLockAsync(resource, requester);

            // Assert
            Assert.True(lockResult.Success);
            Assert.NotNull(lockResult.AcquiredLock);
            Assert.True(lockResultTwo.Success);
            Assert.NotNull(lockResultTwo.AcquiredLock);
        }
    }
}
