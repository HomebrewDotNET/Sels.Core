using Microsoft.Extensions.Logging;
using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sels.Core.Extensions.Logging.Advanced;
using NUnit.Framework;
using Sels.DistributedLocking.Abstractions.Extensions;
using static Sels.Core.Delegates.Async;
using Sels.Core.Extensions.Reflection;
using Sels.Core;
using static Sels.Core.Data.MySQL.MySqlHelper.Database;
using Sels.Core.Contracts.Configuration;
using Sels.Core.Extensions;
using Microsoft.Extensions.Options;
using System.Text.RegularExpressions;
using Castle.Core.Resource;
using System.Resources;
using Sels.Core.Cli.ArgumentParsing;

namespace Sels.DistributedLocking.IntegrationTester.Tests
{
    /// <summary>
    /// Runs a simple test on each feature of <see cref="ILockingProvider"/>.
    /// </summary>
    public class AssertionTester : ITester
    {
        // Fields
        private readonly ILogger? _logger;
        private readonly AssertionTesterOptions _options;

        /// <inheritdoc cref="AssertionTester"/>
        /// <param name="options">Extra options for this tester</param>
        /// <param name="logger">Optional logger for tracing</param>
        public AssertionTester(IOptions<AssertionTesterOptions> options, ILogger<AssertionTester>? logger = null)
        {
            _options = options.ValidateArgument(nameof(options)).Value;

            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> RunTests(TestProvider provider, ILockingProvider lockingProvider, CancellationToken token)
        {
            _logger.Log($"Running assertions");
            Dictionary<string, Exception?> _assertionResults = new Dictionary<string, Exception?>();

            foreach (var (name, assertion) in GetAssertions().Where(a => !_options.Filters.HasValue() || _options.Filters.Any(f => Regex.IsMatch(a.Name, f))))
            {
                token.ThrowIfCancellationRequested();
                _logger.Log($"Executing assertion <{name}>");

                try
                {
                    await assertion(lockingProvider, token);

                    _assertionResults.Add(name, null);
                }
                catch (Exception ex)
                {
                    _assertionResults.Add(name, ex);
                }
            }

            
            Helper.Console.WriteLine(ConsoleColor.DarkGray, $"Assertion results for {provider}:");
            foreach (var (name, result) in _assertionResults)
            {
                var failed = result != null;
                Helper.Console.Write(failed ? ConsoleColor.Red : ConsoleColor.Green, $"[{(failed ? "X" : "V")}] {name}");
                if (failed)
                {
                    Helper.Console.Write(ConsoleColor.Red, ": ");
                    Console.WriteLine($"{result.GetType().Name}({result.Message.GetWithoutNewLine()})");
                }
                else Console.WriteLine();
            }
            Console.WriteLine();
            return !_assertionResults.Any(x => x.Value != null);
        }

        private IEnumerable<(string Name, AsyncAction<ILockingProvider, CancellationToken> Assertion)> GetAssertions()
        {
            // Try lock
            yield return (nameof(TryLock_ResourceCanBeLockedAndUnlocked), TryLock_ResourceCanBeLockedAndUnlocked);
            yield return (nameof(TryLock_ResourceShouldStayLocked), TryLock_ResourceShouldStayLocked);
            yield return (nameof(TryLock_OwnerCanLockResourceAgain), TryLock_OwnerCanLockResourceAgain);
            yield return (nameof(TryLock_ResourceCanBeLockedAfterUnlock), TryLock_ResourceCanBeLockedAfterUnlock);
            yield return (nameof(TryLock_ResourceCanBeLockedAfterExpiry), TryLock_ResourceCanBeLockedAfterExpiry);
            yield return (nameof(TryLock_ExpiryDateGetsUpdatedManually), TryLock_ExpiryDateGetsUpdatedManually);
            yield return (nameof(TryLock_ExpiryDateGetsUpdatedAutomaticallyWithKeepAlive), TryLock_ExpiryDateGetsUpdatedAutomaticallyWithKeepAlive);
            yield return (nameof(TryLock_RequestsTakePriorityOverDirectCalls), TryLock_RequestsTakePriorityOverDirectCalls);

            // Lock
            yield return (nameof(Lock_ResourceIsLockedWhenItIsFreeByExpectedRequester), Lock_ResourceIsLockedWhenItIsFreeByExpectedRequester);
            yield return (nameof(Lock_RequestIsPlacedWhenResourceIsLocked), Lock_RequestIsPlacedWhenResourceIsLocked);
            yield return (nameof(Lock_ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked), Lock_ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked);
            yield return (nameof(Lock_LockExpiryDateIsExtendedWhenKeepAliveIsEnabled), Lock_LockExpiryDateIsExtendedWhenKeepAliveIsEnabled);
            yield return (nameof(Lock_LockRequestIsProperlyTimedOut), Lock_LockRequestIsProperlyTimedOut);
            yield return (nameof(Lock_CancelingRequestThrowsOperationCanceledException), Lock_CancelingRequestThrowsOperationCanceledException);
            yield return (nameof(Lock_RequestGetsAssignedWhenLockExpiresWithCorrectSettings), Lock_RequestGetsAssignedWhenLockExpiresWithCorrectSettings);

            // Get
            yield return (nameof(Get_CorrectStateIsReturned), Get_CorrectStateIsReturned);
            yield return (nameof(Get_CorrectAmountOfPendingRequestsIsReturned), Get_CorrectAmountOfPendingRequestsIsReturned);
            yield return (nameof(Get_NonExistantLockReturnsFreeLock), Get_NonExistantLockReturnsFreeLock);

            // Get pending requests
            yield return (nameof(GetPendingRequests_ReturnsCorrectAmountOfPendingRequestsWithCorrectState), GetPendingRequests_ReturnsCorrectAmountOfPendingRequestsWithCorrectState);
            yield return (nameof(GetPendingRequests_ReturnsEmptyArrayWhenThereAreNoPendingRequests), GetPendingRequests_ReturnsEmptyArrayWhenThereAreNoPendingRequests);
            yield return (nameof(GetPendingRequests_ReturnsEmptyArrayWhenNoLocksExistForTheResource), GetPendingRequests_ReturnsEmptyArrayWhenNoLocksExistForTheResource);

            // Force unlock
            yield return (nameof(ForceUnlock_RemovesLock), ForceUnlock_RemovesLock);
            yield return (nameof(ForceUnlock_RemovesPendingRequests), ForceUnlock_RemovesPendingRequests);

            // Query
            yield return (nameof(Query_FilterOnResourceIsAppliedAndCorrectNumberOfResultsAreReturned), Query_FilterOnResourceIsAppliedAndCorrectNumberOfResultsAreReturned);
            yield return (nameof(Query_FilterOnLockedByIsAppliedAndCorrectNumberOfResultsAreReturned), Query_FilterOnLockedByIsAppliedAndCorrectNumberOfResultsAreReturned);
            yield return (nameof(Query_EqualToOnLockedByIsAppliedAndCorrectNumberOfResultsAreReturned), Query_EqualToOnLockedByIsAppliedAndCorrectNumberOfResultsAreReturned);
            yield return (nameof(Query_EqualToOnLockedByWhenSetToNullIsAppliedAndCorrectNumberOfResultsAreReturned), Query_EqualToOnLockedByWhenSetToNullIsAppliedAndCorrectNumberOfResultsAreReturned);
            yield return (nameof(Query_FilterOnPendingRequestsIsAppliedAndCorrectNumberOfResultsAreReturned), Query_FilterOnPendingRequestsIsAppliedAndCorrectNumberOfResultsAreReturned);
            yield return (nameof(Query_CorrectNumberOfResultsAreReturnedWhenFilteringOnOnlyExpired), Query_CorrectNumberOfResultsAreReturnedWhenFilteringOnOnlyExpired);
            yield return (nameof(Query_CorrectNumberOfResultsAreReturnedWhenFilteringOnOnlyNotExpired), Query_CorrectNumberOfResultsAreReturnedWhenFilteringOnOnlyNotExpired);
            yield return (nameof(Query_ReturnsAllLocksWhenNoFilterApplied), Query_ReturnsAllLocksWhenNoFilterApplied);
            yield return (nameof(Query_ReturnsCorrectLocksWhenPaginationIsApplied), Query_ReturnsCorrectLocksWhenPaginationIsApplied);
            yield return (nameof(Query_CorrectSortingIsAppliedWhenSortingOnResource), Query_CorrectSortingIsAppliedWhenSortingOnResource);
            yield return (nameof(Query_CorrectSortingIsAppliedWhenSortingOnLockedBy), Query_CorrectSortingIsAppliedWhenSortingOnLockedBy);
            yield return (nameof(Query_CorrectSortingIsAppliedWhenSortingOnLastLockDate), Query_CorrectSortingIsAppliedWhenSortingOnLastLockDate);
            yield return (nameof(Query_CorrectSortingIsAppliedWhenSortingOnLockDate), Query_CorrectSortingIsAppliedWhenSortingOnLockDate);
            yield return (nameof(Query_CorrectSortingIsAppliedWhenSortingOnExpiryDate), Query_CorrectSortingIsAppliedWhenSortingOnExpiryDate);
        }

        private async Task TryLock_ResourceCanBeLockedAndUnlocked(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Test locking
            var lockResult = await lockingProvider.TryLockAsync(nameof(TryLock_ResourceCanBeLockedAndUnlocked), nameof(AssertionTester), token: token);
            Assert.IsTrue(lockResult.Success, "Expected lock to be placed but wasn't");

            await using (lockResult.AcquiredLock)
            {
                // Test state
                Assert.IsTrue(await lockResult.AcquiredLock.HasLockAsync(token), "Expected to have lock after acquirery but returned false");
            }

            // Test unlock
            var currentState = await lockingProvider.GetAsync(nameof(TryLock_ResourceCanBeLockedAndUnlocked), token);
            Assert.IsTrue(currentState.CanLock("Other Requester"), "Expected lock to be free after unlock but wasn't");
        }

        private async Task TryLock_ResourceShouldStayLocked(ILockingProvider lockingProvider, CancellationToken token)
        {
            // First lock
            var lockResult = await lockingProvider.TryLockAsync(nameof(TryLock_ResourceShouldStayLocked), nameof(AssertionTester), token: token);
            Assert.IsTrue(lockResult.Success, "Expected lock to be placed but wasn't");

            // Second lock
            var secondLockResult = await lockingProvider.TryLockAsync(nameof(TryLock_ResourceShouldStayLocked), "SecondRequester", token: token);
            Assert.IsFalse(secondLockResult.Success, "Expected second lock to fail but was successful");

            // Release lock
            await lockResult.AcquiredLock.DisposeAsync();
        }

        private async Task TryLock_OwnerCanLockResourceAgain(ILockingProvider lockingProvider, CancellationToken token)
        {
            // First lock
            var lockResult = await lockingProvider.TryLockAsync(nameof(TryLock_OwnerCanLockResourceAgain), nameof(AssertionTester), token: token);
            Assert.IsTrue(lockResult.Success, "Expected lock to be placed but wasn't");

            // Second lock
            var secondLockResult = await lockingProvider.TryLockAsync(nameof(TryLock_OwnerCanLockResourceAgain), nameof(AssertionTester), token: token);
            Assert.IsTrue(secondLockResult.Success, "Expected owner to receive lock again");

            // Release lock
            await secondLockResult.AcquiredLock.DisposeAsync();
        }

        private async Task TryLock_ResourceCanBeLockedAfterUnlock(ILockingProvider lockingProvider, CancellationToken token)
        {
            // First lock
            var lockResult = await lockingProvider.TryLockAsync(nameof(TryLock_ResourceCanBeLockedAfterUnlock), $"{nameof(AssertionTester)}.1", token: token);
            Assert.IsTrue(lockResult.Success, "Expected lock to be placed but wasn't");

            // Release lock
            await lockResult.AcquiredLock.DisposeAsync();

            // Second lock
            var secondLockResult = await lockingProvider.TryLockAsync(nameof(TryLock_ResourceCanBeLockedAfterUnlock), $"{nameof(AssertionTester)}.2", token: token);
            Assert.IsTrue(secondLockResult.Success, "Expected lock to be placed after unlock but wasn't");

            // Release lock
            await secondLockResult.AcquiredLock.DisposeAsync();
        }

        private async Task TryLock_ResourceCanBeLockedAfterExpiry(ILockingProvider lockingProvider, CancellationToken token)
        {
            const int ExpirySeconds = 1;

            // First lock
            var lockResult = await lockingProvider.TryLockAsync(nameof(TryLock_ResourceCanBeLockedAfterExpiry), $"{nameof(AssertionTester)}.1", TimeSpan.FromSeconds(ExpirySeconds), token: token);
            var currentExpiry = lockResult.AcquiredLock.ExpiryDate;
            Assert.That(currentExpiry, Is.Not.Null, "Expected expiry date to be set");
            Assert.IsTrue(lockResult.Success, "Expected lock to be placed but wasn't");

            // Wait until original expiry
            await Helper.Async.Sleep((currentExpiry.Value - DateTime.Now).Add(TimeSpan.FromSeconds(1)), token);

            // Second lock
            var secondLockResult = await lockingProvider.TryLockAsync(nameof(TryLock_ResourceCanBeLockedAfterExpiry), $"{nameof(AssertionTester)}.2", token: token);
            Assert.IsTrue(secondLockResult.Success, "Expected lock to be placed after expiry but wasn't");

            // Release lock
            await secondLockResult.AcquiredLock.DisposeAsync();
        }

        private async Task TryLock_ExpiryDateGetsUpdatedManually(ILockingProvider lockingProvider, CancellationToken token)
        {
            // First lock
            var lockResult = await lockingProvider.TryLockAsync(nameof(TryLock_ExpiryDateGetsUpdatedManually), $"{nameof(AssertionTester)}.1", TimeSpan.FromHours(1), token: token);
            Assert.IsTrue(lockResult.Success, "Expected lock to be placed but wasn't");
            var currentExpiry = lockResult.AcquiredLock.ExpiryDate;
            Assert.That(currentExpiry, Is.Not.Null, "Expected expiry date to be set");

            // Update expiry
            await lockResult.AcquiredLock.ExtendAsync(TimeSpan.FromHours(1), token);
            var newExpiry = lockResult.AcquiredLock.ExpiryDate;
            Assert.That(currentExpiry, Is.Not.Null, "Expected new expiry date to be set");

            // Assert
            Assert.That(newExpiry, Is.GreaterThan(currentExpiry), "Expected updated expiry date to be greater than the initial expiry date");

            // Release lock
            await lockResult.AcquiredLock.DisposeAsync();
        }

        private async Task TryLock_ExpiryDateGetsUpdatedAutomaticallyWithKeepAlive(ILockingProvider lockingProvider, CancellationToken token)
        {
            const int ExpirySeconds = 4;

            // First lock
            var lockResult = await lockingProvider.TryLockAsync(nameof(TryLock_ExpiryDateGetsUpdatedAutomaticallyWithKeepAlive), nameof(AssertionTester), TimeSpan.FromSeconds(ExpirySeconds), true, token: token);
            Assert.IsTrue(lockResult.Success, "Expected lock to be placed but wasn't");
            var currentExpiry = lockResult.AcquiredLock.ExpiryDate;
            Assert.That(currentExpiry, Is.Not.Null, "Expected expiry date to be set");

            // Wait for expiry
            await Helper.Async.Sleep(currentExpiry.Value - DateTime.Now, token);

            // Get new expiry
            var newExpiry = lockResult.AcquiredLock.ExpiryDate;
            Assert.That(currentExpiry, Is.Not.Null, "Expected new expiry date to be set");

            // Assert
            Assert.That(newExpiry, Is.GreaterThan(currentExpiry), "Expected updated expiry date to be greater than the initial expiry date");

            // Release lock
            await lockResult.AcquiredLock.DisposeAsync();
        }

        private async Task TryLock_RequestsTakePriorityOverDirectCalls(ILockingProvider lockingProvider, CancellationToken token)
        {
            ILock requestResult = null;
            // First lock
            var lockResult = await lockingProvider.TryLockAsync(nameof(TryLock_RequestsTakePriorityOverDirectCalls), $"{nameof(AssertionTester)}.1", token: token);
            Assert.IsTrue(lockResult.Success, "Expected lock to be placed but wasn't");

            // Log request
            var lockRequest = lockingProvider.LockAsync(nameof(TryLock_RequestsTakePriorityOverDirectCalls), $"{nameof(AssertionTester)}.2", token: token);

            // Wait a bit for the request to flush
            await Helper.Async.Sleep(TimeSpan.FromSeconds(1), token);

            // Release lock
            await lockResult.AcquiredLock.UnlockAsync(token);

            if (lockRequest.IsCompleted) requestResult = await lockRequest;

            // Try lock 
            var secondLockResult = await lockingProvider.TryLockAsync(nameof(TryLock_RequestsTakePriorityOverDirectCalls), $"{nameof(AssertionTester)}.3", token: token);
            Assert.IsFalse(secondLockResult.Success, "Expected direct lock to fail but succeeded");

            // Wait for request
            requestResult = await Helper.Async.WaitOn(lockRequest, TimeSpan.FromSeconds(5), token);
            Assert.That(requestResult, Is.Not.Null, "Lock request returned null");
        }

        private async Task Lock_ResourceIsLockedWhenItIsFreeByExpectedRequester(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Lock
            var @lock = await lockingProvider.LockAsync(nameof(Lock_ResourceIsLockedWhenItIsFreeByExpectedRequester), nameof(AssertionTester), timeout: TimeSpan.FromSeconds(10), token: token);

            Assert.IsNotNull(@lock);
            await using (@lock)
            {
                Assert.That(@lock.Resource, Is.EqualTo(nameof(Lock_ResourceIsLockedWhenItIsFreeByExpectedRequester)));
                Assert.That(@lock.LockedBy, Is.EqualTo(nameof(AssertionTester)));
                Assert.That(@lock.ExpiryDate, Is.Null);
            }
        }

        private async Task Lock_RequestIsPlacedWhenResourceIsLocked(ILockingProvider lockingProvider, CancellationToken token)
        {
            // First place lock
            var initialLock = await lockingProvider.LockAsync(nameof(Lock_RequestIsPlacedWhenResourceIsLocked), $"{nameof(AssertionTester)}.1", timeout: TimeSpan.FromSeconds(10), token: token);

            Assert.IsNotNull(initialLock);
            Assert.That(initialLock.Resource, Is.EqualTo(nameof(Lock_RequestIsPlacedWhenResourceIsLocked)));
            Assert.That(initialLock.LockedBy, Is.EqualTo($"{nameof(AssertionTester)}.1"));
            Assert.That(initialLock.ExpiryDate, Is.Null);

            // Log request
            var lockTask = lockingProvider.LockAsync(nameof(Lock_RequestIsPlacedWhenResourceIsLocked), $"{nameof(AssertionTester)}.2", token: token);
            Assert.IsNotNull(lockTask);

            // Wait for request to be created
            await Helper.Async.Sleep(TimeSpan.FromSeconds(1));

            // Check if pending request exists
            var pendingRequests = await lockingProvider.GetPendingRequestsAsync(nameof(Lock_RequestIsPlacedWhenResourceIsLocked), token: token);
            Assert.IsNotNull(pendingRequests);
            Assert.That(pendingRequests.Length, Is.EqualTo(1));
            var request = pendingRequests[0];
            Assert.IsNotNull(request);
            Assert.That(request.Resource, Is.EqualTo(nameof(Lock_RequestIsPlacedWhenResourceIsLocked)));
            Assert.That(request.Requester, Is.EqualTo($"{nameof(AssertionTester)}.2"));
            Assert.That(request.ExpiryTime, Is.Null);
            Assert.That(request.Timeout, Is.Null);
            Assert.That(request.KeepAlive, Is.False);

            // Release first lock
            await initialLock.DisposeAsync();

            // Wait for second lock to get assigned and unlock
            var @lock = await lockTask;
            await @lock.DisposeAsync();
        }

        private async Task Lock_ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Place locks
            var lockOne = await lockingProvider.LockAsync(nameof(Lock_ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked), $"{nameof(AssertionTester)}.0", token: token);
            _ = lockingProvider.LockAsync(nameof(Lock_ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked), $"{nameof(AssertionTester)}.1", token: token);
            await Helper.Async.Sleep(TimeSpan.FromMilliseconds(50));
            _ = lockingProvider.LockAsync(nameof(Lock_ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked), $"{nameof(AssertionTester)}.2", token: token);
            await Helper.Async.Sleep(TimeSpan.FromMilliseconds(50));
            _ = lockingProvider.LockAsync(nameof(Lock_ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked), $"{nameof(AssertionTester)}.3", token: token);
            await Helper.Async.Sleep(TimeSpan.FromMilliseconds(50));

            // Get pending requests
            var pendingRequests = await lockingProvider.GetPendingRequestsAsync(nameof(Lock_ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked), token: token);

            // Assert
            Assert.IsNotNull(lockOne);
            Assert.That(lockOne.Resource, Is.EqualTo(nameof(Lock_ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked)));
            Assert.That(lockOne.LockedBy, Is.EqualTo($"{nameof(AssertionTester)}.0"));
            Assert.That(lockOne.ExpiryDate, Is.Null);
            Assert.IsNotNull(pendingRequests);
            Assert.That(pendingRequests.Length, Is.EqualTo(3));
            for (int i = 0; i < pendingRequests.Length; i++)
            {
                Assert.IsNotNull(pendingRequests[i]);
                Assert.That(pendingRequests[i].Resource, Is.EqualTo(nameof(Lock_ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked)));
                Assert.That(pendingRequests[i].Requester, Is.EqualTo($"{nameof(AssertionTester)}.{i + 1}"));
                Assert.That(pendingRequests[i].ExpiryTime, Is.Null);
                Assert.That(pendingRequests[i].Timeout, Is.Null);
                Assert.That(pendingRequests[i].KeepAlive, Is.False);
            }
        }

        private async Task Lock_LockExpiryDateIsExtendedWhenKeepAliveIsEnabled(ILockingProvider lockingProvider, CancellationToken token)
        {
            const int expiryTime = 100;

            // Place lock
            await using (var @lock = await lockingProvider.LockAsync(nameof(Lock_LockExpiryDateIsExtendedWhenKeepAliveIsEnabled), nameof(AssertionTester), TimeSpan.FromMilliseconds(expiryTime), true, token: token))
            {
                var initialExpiry = @lock.ExpiryDate;

                // Give lock time to extend itself
                await Helper.Async.Sleep(expiryTime * 5);
                var currentExpiry = @lock.ExpiryDate;

                // Assert
                Assert.That(initialExpiry, Is.Not.EqualTo(currentExpiry));
                Assert.That(currentExpiry, Is.GreaterThan(initialExpiry));
            }
        }

        private async Task Lock_LockRequestIsProperlyTimedOut(ILockingProvider lockingProvider, CancellationToken token)
        {
            Exception exception = null;
            const int WaitTime = 1;

            // Place intial lock
            var initialLock = await lockingProvider.LockAsync(nameof(Lock_LockRequestIsProperlyTimedOut), $"{nameof(AssertionTester)}.1", timeout: TimeSpan.FromSeconds(10), token: token);

            // Place second lock that should timeout
            var lockResultTask = lockingProvider.LockAsync(nameof(Lock_LockRequestIsProperlyTimedOut), $"{nameof(AssertionTester)}.2", timeout: TimeSpan.FromSeconds(WaitTime), token: token);

            try
            {
                // Give time for lock to timeout
                await Helper.Async.WaitOn(lockResultTask, TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception, Is.InstanceOf<LockTimeoutException>());
            var timeoutException = exception as LockTimeoutException;
            Assert.That(timeoutException.Lock, Is.Not.Null);
            Assert.That(timeoutException.Lock.Resource, Is.EqualTo(nameof(Lock_LockRequestIsProperlyTimedOut)));
            Assert.That(timeoutException.Lock.LockedBy, Is.EqualTo($"{nameof(AssertionTester)}.1"));
            Assert.That(timeoutException.Requester, Is.EqualTo($"{nameof(AssertionTester)}.2"));
            Assert.That(timeoutException.Timeout, Is.EqualTo(TimeSpan.FromSeconds(WaitTime)));

            // Release
            await initialLock.DisposeAsync();
        }

        private async Task Lock_CancelingRequestThrowsOperationCanceledException(ILockingProvider lockingProvider, CancellationToken token)
        {
            Exception exception = null;
            var tokenSource = new CancellationTokenSource();

            // Place intial lock
            var initialLock = await lockingProvider.LockAsync(nameof(Lock_CancelingRequestThrowsOperationCanceledException), $"{nameof(AssertionTester)}.1", timeout: TimeSpan.FromSeconds(10), token: token);

            // Place second lock that should cancel
            var lockResultTask = lockingProvider.LockAsync(nameof(Lock_CancelingRequestThrowsOperationCanceledException), $"{nameof(AssertionTester)}.2", token: tokenSource.Token);

            // Cancel request and wait for cancel to be acknowledged
            await Helper.Async.Sleep(TimeSpan.FromSeconds(1));
            tokenSource.Cancel();

            try
            {
                await Helper.Async.WaitOn(lockResultTask, TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception, Is.InstanceOf<OperationCanceledException>());
        }

        private async Task Lock_RequestGetsAssignedWhenLockExpiresWithCorrectSettings(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Place intial lock with expiry
            var initialLock = await lockingProvider.LockAsync(nameof(Lock_RequestGetsAssignedWhenLockExpiresWithCorrectSettings), $"{nameof(AssertionTester)}.1", expiryTime: TimeSpan.FromSeconds(1), token: token);

            // Log second request
            var lockResultTask = lockingProvider.LockAsync(nameof(Lock_RequestGetsAssignedWhenLockExpiresWithCorrectSettings), $"{nameof(AssertionTester)}.2", TimeSpan.FromMinutes(5), true, token: token);

            // Give time for request to be assigned
            var @lock = await Helper.Async.WaitOn(lockResultTask, TimeSpan.FromSeconds(5));

            // Assert
            Assert.That(lockResultTask, Is.Not.Null);
            Assert.That(lockResultTask.IsCompleted, Is.True);
            Assert.IsNotNull(@lock);
            Assert.That(@lock.Resource, Is.EqualTo(nameof(Lock_RequestGetsAssignedWhenLockExpiresWithCorrectSettings)));
            Assert.That(@lock.LockedBy, Is.EqualTo($"{nameof(AssertionTester)}.2"));
            Assert.That(@lock.ExpiryDate, Is.Not.Null);

            // Release
            await @lock.DisposeAsync();
        }

        private async Task Get_CorrectStateIsReturned(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Lock
            var lockResult = await lockingProvider.TryLockAsync(nameof(Get_CorrectStateIsReturned), nameof(AssertionTester), TimeSpan.FromMinutes(5), token: token);

            // Get
            var result = await lockingProvider.GetAsync(nameof(Get_CorrectStateIsReturned), token: token);

            // Assert
            Assert.That(lockResult, Is.Not.Null);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Resource, Is.EqualTo(lockResult.AcquiredLock.Resource));
            Assert.That(result.LockedBy, Is.EqualTo(lockResult.AcquiredLock.LockedBy));
            Assert.That(result.LockedAt, Is.EqualTo(lockResult.AcquiredLock.LockedAt));
            Assert.That(result.ExpiryDate, Is.EqualTo(lockResult.AcquiredLock.ExpiryDate));
            Assert.That(result.LastLockDate, Is.EqualTo(lockResult.AcquiredLock.LastLockDate));
            Assert.That(result.PendingRequests, Is.EqualTo(lockResult.AcquiredLock.PendingRequests));
        }

        private async Task Get_CorrectAmountOfPendingRequestsIsReturned(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Lock
            var lockResult = await lockingProvider.TryLockAsync(nameof(Get_CorrectAmountOfPendingRequestsIsReturned), nameof(AssertionTester), TimeSpan.FromMinutes(5), token: token);
            _ = lockingProvider.LockAsync(nameof(Get_CorrectAmountOfPendingRequestsIsReturned), $"{nameof(AssertionTester)}.1", token: token);
            await Helper.Async.Sleep(TimeSpan.FromMilliseconds(50));
            _ = lockingProvider.LockAsync(nameof(Get_CorrectAmountOfPendingRequestsIsReturned), $"{nameof(AssertionTester)}.2", token: token);
            await Helper.Async.Sleep(TimeSpan.FromMilliseconds(50));
            _ = lockingProvider.LockAsync(nameof(Get_CorrectAmountOfPendingRequestsIsReturned), $"{nameof(AssertionTester)}.3", token: token);
            await Helper.Async.Sleep(TimeSpan.FromMilliseconds(50));

            // Get
            var result = await lockingProvider.GetAsync(nameof(Get_CorrectAmountOfPendingRequestsIsReturned), token: token);

            // Assert
            Assert.That(lockResult, Is.Not.Null);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.PendingRequests, Is.EqualTo(3));
        }

        private async Task Get_NonExistantLockReturnsFreeLock(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Get
            var result = await lockingProvider.GetAsync(nameof(Get_NonExistantLockReturnsFreeLock), token: token);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Resource, Is.EqualTo(nameof(Get_NonExistantLockReturnsFreeLock)));
            Assert.That(result.LockedBy, Is.Null);
            Assert.That(result.LockedAt, Is.Null);
            Assert.That(result.ExpiryDate, Is.Null);
            Assert.That(result.LastLockDate, Is.Null);
            Assert.That(result.PendingRequests, Is.EqualTo(0));
        }

        private async Task GetPendingRequests_ReturnsCorrectAmountOfPendingRequestsWithCorrectState(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Place locks
            var lockOne = await lockingProvider.LockAsync(nameof(GetPendingRequests_ReturnsCorrectAmountOfPendingRequestsWithCorrectState), $"{nameof(AssertionTester)}.0", token: token);
            _ = lockingProvider.LockAsync(nameof(GetPendingRequests_ReturnsCorrectAmountOfPendingRequestsWithCorrectState), $"{nameof(AssertionTester)}.1", token: token);
            await Helper.Async.Sleep(TimeSpan.FromMilliseconds(50));
            _ = lockingProvider.LockAsync(nameof(GetPendingRequests_ReturnsCorrectAmountOfPendingRequestsWithCorrectState), $"{nameof(AssertionTester)}.2", token: token);
            await Helper.Async.Sleep(TimeSpan.FromMilliseconds(50));
            _ = lockingProvider.LockAsync(nameof(GetPendingRequests_ReturnsCorrectAmountOfPendingRequestsWithCorrectState), $"{nameof(AssertionTester)}.3", token: token);
            await Helper.Async.Sleep(TimeSpan.FromMilliseconds(50));

            // Get requests
            var pendingRequests = await lockingProvider.GetPendingRequestsAsync(nameof(GetPendingRequests_ReturnsCorrectAmountOfPendingRequestsWithCorrectState), token: token);

            // Assert
            Assert.IsNotNull(pendingRequests);
            Assert.That(pendingRequests.Length, Is.EqualTo(3));
            for (int i = 0; i < pendingRequests.Length; i++)
            {
                Assert.IsNotNull(pendingRequests[i]);
                Assert.That(pendingRequests[i].Resource, Is.EqualTo(nameof(GetPendingRequests_ReturnsCorrectAmountOfPendingRequestsWithCorrectState)));
                Assert.That(pendingRequests[i].Requester, Is.EqualTo($"{nameof(AssertionTester)}.{i + 1}"));
                Assert.That(pendingRequests[i].ExpiryTime, Is.Null);
                Assert.That(pendingRequests[i].Timeout, Is.Null);
                Assert.That(pendingRequests[i].KeepAlive, Is.False);
            }
        }

        private async Task GetPendingRequests_ReturnsEmptyArrayWhenThereAreNoPendingRequests(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Lock
            _ = await lockingProvider.LockAsync(nameof(GetPendingRequests_ReturnsEmptyArrayWhenThereAreNoPendingRequests), nameof(AssertionTester), token: token);

            // Get
            var pendingRequests = await lockingProvider.GetPendingRequestsAsync(nameof(GetPendingRequests_ReturnsEmptyArrayWhenThereAreNoPendingRequests), token: token);

            // Assert
            Assert.IsNotNull(pendingRequests);
            Assert.That(pendingRequests.Length, Is.EqualTo(0));
        }

        private async Task GetPendingRequests_ReturnsEmptyArrayWhenNoLocksExistForTheResource(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Get
            var pendingRequests = await lockingProvider.GetPendingRequestsAsync(nameof(GetPendingRequests_ReturnsEmptyArrayWhenNoLocksExistForTheResource), token: token);

            // Assert
            Assert.IsNotNull(pendingRequests);
            Assert.That(pendingRequests.Length, Is.EqualTo(0));
        }

        private async Task ForceUnlock_RemovesLock(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Lock
            var initialLock = await lockingProvider.LockAsync(nameof(ForceUnlock_RemovesLock), nameof(AssertionTester), token: token);

            // Force unlock
            await lockingProvider.ForceUnlockAsync(nameof(ForceUnlock_RemovesLock), false, token: token);

            // Get 
            var result = await lockingProvider.GetAsync(nameof(ForceUnlock_RemovesLock), token: token);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Resource, Is.EqualTo(nameof(ForceUnlock_RemovesLock)));
            Assert.That(result.LockedBy, Is.Null);
            Assert.That(result.LockedAt, Is.Null);
            Assert.That(result.ExpiryDate, Is.Null);
        }

        private async Task ForceUnlock_RemovesPendingRequests(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Lock
            _ = lockingProvider.LockAsync(nameof(ForceUnlock_RemovesPendingRequests), $"{nameof(AssertionTester)}.0", token: token);
            _ = lockingProvider.LockAsync(nameof(ForceUnlock_RemovesPendingRequests), $"{nameof(AssertionTester)}.1", token: token);
            _ = lockingProvider.LockAsync(nameof(ForceUnlock_RemovesPendingRequests), $"{nameof(AssertionTester)}.2", token: token);
            _ = lockingProvider.LockAsync(nameof(ForceUnlock_RemovesPendingRequests), $"{nameof(AssertionTester)}.3", token: token);

            // Wait for flush
            await Helper.Async.Sleep(TimeSpan.FromSeconds(1));

            // Force unlock
            await lockingProvider.ForceUnlockAsync(nameof(ForceUnlock_RemovesPendingRequests), true, token: token);

            // Get 
            var result = await lockingProvider.GetAsync(nameof(ForceUnlock_RemovesPendingRequests), token: token);
            var pendingRequests = await lockingProvider.GetPendingRequestsAsync(nameof(ForceUnlock_RemovesPendingRequests), token: token);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Resource, Is.EqualTo(nameof(ForceUnlock_RemovesPendingRequests)));
            Assert.That(result.LockedBy, Is.Null);
            Assert.That(result.LockedAt, Is.Null);
            Assert.That(result.ExpiryDate, Is.Null);
            Assert.That(pendingRequests, Is.Empty);
        }

        private async Task Query_FilterOnResourceIsAppliedAndCorrectNumberOfResultsAreReturned(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Create entries
            var resources = new string[] { "Resource", "System.Monitor", "System.Heartbeat", "RecurringJob.1-1" }
                            .Select(x => $"{nameof(Query_FilterOnResourceIsAppliedAndCorrectNumberOfResultsAreReturned)}.{x}")
                            .ToArray();
            foreach (var resource in resources)
            {
                await lockingProvider.LockAsync(resource, nameof(AssertionTester), token: token);
            }

            // Query
            var result = await lockingProvider.QueryAsync(x => x.WithFilterOnResource($"{nameof(Query_FilterOnResourceIsAppliedAndCorrectNumberOfResultsAreReturned)}.System"), token: token);

            // Assert
            Assert.That(result.Results, Is.Not.Null);
            Assert.That(result.Results.Length, Is.EqualTo(2));
            foreach (var lockResult in result.Results)
            {
                Assert.That(lockResult, Is.Not.Null);
                Assert.Contains(lockResult.Resource, resources);
            }
        }

        private async Task Query_FilterOnLockedByIsAppliedAndCorrectNumberOfResultsAreReturned(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Create entries
            var requesters = new string[] { "Thread.1", "Thread.2", "BackgroundJob.1", "BackgroundJob.2", "RecurringJob.1" };

            foreach (var (requester, i) in requesters.Select((x, i) => (x, i)))
            {
                await lockingProvider.LockAsync($"{nameof(Query_FilterOnLockedByIsAppliedAndCorrectNumberOfResultsAreReturned)}.{i}", requester, token: token);
            }

            // Query
            var result = await lockingProvider.QueryAsync(x => x.WithFilterOnResource(nameof(Query_FilterOnLockedByIsAppliedAndCorrectNumberOfResultsAreReturned))
                                                                .WithFilterOnLockedBy("BackgroundJob.2"), token: token);

            // Assert
            Assert.That(result.Results, Is.Not.Null);
            Assert.That(result.Results.Length, Is.EqualTo(1));
            foreach (var lockResult in result.Results)
            {
                Assert.That(lockResult, Is.Not.Null);
                Assert.Contains(lockResult.LockedBy, requesters);
            }
        }

        private async Task Query_EqualToOnLockedByIsAppliedAndCorrectNumberOfResultsAreReturned(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Create entries
            var requesters = new string?[] { "Thread.1", "Thread.1", null, "BackgroundJob.2", "Thread.1" };

            foreach (var (requester, i) in requesters.Select((x, i) => (x, i)))
            {
                if(requester == null)
                {
                    await using(await lockingProvider.LockAsync($"{nameof(Query_EqualToOnLockedByIsAppliedAndCorrectNumberOfResultsAreReturned)}.{i}", Guid.NewGuid().ToString(), token: token))
                    {

                    }
                }
                else
                {
                    await lockingProvider.LockAsync($"{nameof(Query_EqualToOnLockedByIsAppliedAndCorrectNumberOfResultsAreReturned)}.{i}", requester, token: token);
                }
            }

            // Query
            var result = await lockingProvider.QueryAsync(x => x.WithFilterOnResource(nameof(Query_EqualToOnLockedByIsAppliedAndCorrectNumberOfResultsAreReturned))
                                                                .WithLockedByEqualTo("Thread.1"), token: token);

            // Assert
            Assert.That(result.Results, Is.Not.Null);
            Assert.That(result.Results.Length, Is.EqualTo(3));
            foreach (var lockResult in result.Results)
            {
                Assert.That(lockResult, Is.Not.Null);
                Assert.Contains(lockResult.LockedBy, requesters);
            }
        }

        private async Task Query_EqualToOnLockedByWhenSetToNullIsAppliedAndCorrectNumberOfResultsAreReturned(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Create entries
            var requesters = new string?[] { "Thread.1", "Thread.1", null, "BackgroundJob.2", "Thread.1", null };

            foreach (var (requester, i) in requesters.Select((x, i) => (x, i)))
            {
                if (requester == null)
                {
                    await using (await lockingProvider.LockAsync($"{nameof(Query_EqualToOnLockedByWhenSetToNullIsAppliedAndCorrectNumberOfResultsAreReturned)}.{i}", Guid.NewGuid().ToString(), token: token))
                    {

                    }
                }
                else
                {
                    await lockingProvider.LockAsync($"{nameof(Query_EqualToOnLockedByWhenSetToNullIsAppliedAndCorrectNumberOfResultsAreReturned)}.{i}", requester, token: token);
                }
            }

            // Query
            var result = await lockingProvider.QueryAsync(x => x.WithFilterOnResource(nameof(Query_EqualToOnLockedByWhenSetToNullIsAppliedAndCorrectNumberOfResultsAreReturned))
                                                                .WithLockedByEqualTo(null), token: token);

            // Assert
            Assert.That(result.Results, Is.Not.Null);
            Assert.That(result.Results.Length, Is.EqualTo(2));
            foreach (var lockResult in result.Results)
            {
                Assert.That(lockResult, Is.Not.Null);
                Assert.Contains(lockResult.LockedBy, requesters);
            }
        }

        private async Task Query_FilterOnPendingRequestsIsAppliedAndCorrectNumberOfResultsAreReturned(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Create entries
            for (int i = 0; i < 10; i++)
            {
                await lockingProvider.LockAsync($"{nameof(Query_FilterOnPendingRequestsIsAppliedAndCorrectNumberOfResultsAreReturned)}.{i}", Guid.NewGuid().ToString(), token: token);
                for (int y = 0; y < i; y++)
                {
                    _ = lockingProvider.LockAsync($"{nameof(Query_FilterOnPendingRequestsIsAppliedAndCorrectNumberOfResultsAreReturned)}.{y}", $"{nameof(AssertionTester)},{nameof(y)}", token: token);
                }
            }
            await Helper.Async.Sleep(TimeSpan.FromSeconds(1));

            // Query
            var result = await lockingProvider.QueryAsync(x => x.WithFilterOnResource(nameof(Query_FilterOnPendingRequestsIsAppliedAndCorrectNumberOfResultsAreReturned))
                                                                .WithPendingRequestsLargerThan(5), token: token);

            // Assert
            Assert.That(result.Results, Is.Not.Null);
            Assert.That(result.Results.Length, Is.EqualTo(4));
        }

        private async Task Query_CorrectNumberOfResultsAreReturnedWhenFilteringOnOnlyExpired(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Create entries
            for (int i = 0; i < 100; i++)
            {
                await lockingProvider.LockAsync($"{nameof(Query_CorrectNumberOfResultsAreReturnedWhenFilteringOnOnlyExpired)}.{i}", Guid.NewGuid().ToString(), i < 10 ? TimeSpan.Zero : TimeSpan.FromHours(24));
            }

            // Query
            var result = await lockingProvider.QueryAsync(x => x.WithFilterOnResource(nameof(Query_CorrectNumberOfResultsAreReturnedWhenFilteringOnOnlyExpired))
                                                                .WithOnlyExpired(), token: token);

            // Assert
            Assert.That(result.Results, Is.Not.Null);
            Assert.That(result.Results.Length, Is.EqualTo(10));
        }

        private async Task Query_CorrectNumberOfResultsAreReturnedWhenFilteringOnOnlyNotExpired(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Create entries
            for (int i = 0; i < 100; i++)
            {
                await lockingProvider.LockAsync($"{nameof(Query_CorrectNumberOfResultsAreReturnedWhenFilteringOnOnlyNotExpired)}.{i}", Guid.NewGuid().ToString(), i < 10 ? TimeSpan.Zero : TimeSpan.FromHours(24), token: token);
            }

            // Query
            var result = await lockingProvider.QueryAsync(x => x.WithFilterOnResource(nameof(Query_CorrectNumberOfResultsAreReturnedWhenFilteringOnOnlyNotExpired))
                                                                .WithOnlyNotExpired(), token: token);

            // Assert
            Assert.That(result.Results, Is.Not.Null);
            Assert.That(result.Results.Length, Is.EqualTo(90));
        }

        private async Task Query_ReturnsAllLocksWhenNoFilterApplied(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Act
            var result = await lockingProvider.QueryAsync(x => { }, token: token);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Results.Length, Is.GreaterThanOrEqualTo(GetAssertions().Count()));
        }

        private async Task Query_ReturnsCorrectLocksWhenPaginationIsApplied(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Create entries
            foreach (var @lock in new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" }.Select(x => $"{nameof(Query_ReturnsCorrectLocksWhenPaginationIsApplied)}.{x}"))
            {
                await lockingProvider.TryLockAsync(@lock, nameof(AssertionTester), token: token);
            }

            // Act
            var result = await lockingProvider.QueryAsync(x => x.WithFilterOnResource(nameof(Query_ReturnsCorrectLocksWhenPaginationIsApplied))
                                                                .WithPagination(2, 5), token: token);

            // Assert
            Assert.That(result, Is.Not.Null);
            CollectionAssert.AreEqual(new string[] { "6", "7", "8", "9" }.Select(x => $"{nameof(Query_ReturnsCorrectLocksWhenPaginationIsApplied)}.{x}"), result.Results.Select(x => x.Resource));
        }

        private async Task Query_CorrectSortingIsAppliedWhenSortingOnResource(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Create entries
            foreach (var @lock in new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" }.Select(x => $"{nameof(Query_CorrectSortingIsAppliedWhenSortingOnResource)}.{x}"))
            {
                await lockingProvider.TryLockAsync(@lock, nameof(AssertionTester), token: token);
            }

            // Act
            var result = await lockingProvider.QueryAsync(x => x.WithFilterOnResource(nameof(Query_CorrectSortingIsAppliedWhenSortingOnResource))
                                                                .OrderByResource(true), token: token);

            // Assert
            Assert.That(result, Is.Not.Null);
            CollectionAssert.AreEqual(new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" }
                                        .Reverse()
                                        .Select(x => $"{nameof(Query_CorrectSortingIsAppliedWhenSortingOnResource)}.{x}")
                                      , result.Results.Select(x => x.Resource));
        }

        private async Task Query_CorrectSortingIsAppliedWhenSortingOnLockedBy(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Create entries
            foreach (var requester in new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" })
            {
                await lockingProvider.TryLockAsync($"{nameof(Query_CorrectSortingIsAppliedWhenSortingOnLockedBy)}.{Guid.NewGuid()}", requester, token: token);
            }

            // Act
            var result = await lockingProvider.QueryAsync(x => x.WithFilterOnResource(nameof(Query_CorrectSortingIsAppliedWhenSortingOnLockedBy))
                                                                .OrderByLockedBy(true), token: token);

            // Assert
            Assert.That(result, Is.Not.Null);
            CollectionAssert.AreEqual(new string[] { "1", "2", "3", "4", "5", "6", "7", "8", "9" }
                                        .Reverse()
                                      , result.Results.Select(x => x.LockedBy));
        }

        private async Task Query_CorrectSortingIsAppliedWhenSortingOnLastLockDate(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Create entries
            foreach (var i in Enumerable.Range(0, 100))
            {
                await using (await lockingProvider.LockAsync($"{nameof(Query_CorrectSortingIsAppliedWhenSortingOnLastLockDate)}.{i}", nameof(AssertionTester), token: token))
                {

                }
            }

            // Act
            var result = await lockingProvider.QueryAsync(x => x.WithFilterOnResource(nameof(Query_CorrectSortingIsAppliedWhenSortingOnLastLockDate))
                                                                .OrderByLastLockDate(true), token: token);

            // Assert
            Assert.That(result, Is.Not.Null);
            CollectionAssert.AreEqual(Enumerable.Range(0, 100)
                                        .Reverse()
                                        .Select(x => $"{nameof(Query_CorrectSortingIsAppliedWhenSortingOnLastLockDate)}.{x}")
                                      , result.Results.Select(x => x.Resource));
        }

        private async Task Query_CorrectSortingIsAppliedWhenSortingOnLockDate(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Create entries
            foreach (var i in Enumerable.Range(0, 100))
            {
                _ = await lockingProvider.TryLockAsync($"{nameof(Query_CorrectSortingIsAppliedWhenSortingOnLockDate)}.{i}", nameof(AssertionTester), token: token);
            }

            // Act
            var result = await lockingProvider.QueryAsync(x => x.WithFilterOnResource(nameof(Query_CorrectSortingIsAppliedWhenSortingOnLockDate))
                                                                .OrderByLockedAt(true), token: token);

            // Assert
            Assert.That(result, Is.Not.Null);
            CollectionAssert.AreEqual(Enumerable.Range(0, 100)
                                        .Reverse()
                                        .Select(x => $"{nameof(Query_CorrectSortingIsAppliedWhenSortingOnLockDate)}.{x}")
                                      , result.Results.Select(x => x.Resource));
        }

        private async Task Query_CorrectSortingIsAppliedWhenSortingOnExpiryDate(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Create entries
            foreach (var i in Enumerable.Range(0, 100))
            {
                _ = await lockingProvider.TryLockAsync($"{nameof(Query_CorrectSortingIsAppliedWhenSortingOnExpiryDate)}.{i}", nameof(AssertionTester), TimeSpan.FromMilliseconds(10), token: token);
            }

            // Act
            var result = await lockingProvider.QueryAsync(x => x.WithFilterOnResource(nameof(Query_CorrectSortingIsAppliedWhenSortingOnExpiryDate))
                                                                .OrderByExpiryDate(true), token: token);

            // Assert
            Assert.That(result, Is.Not.Null);
            CollectionAssert.AreEqual(Enumerable.Range(0, 100)
                                        .Reverse()
                                        .Select(x => $"{nameof(Query_CorrectSortingIsAppliedWhenSortingOnExpiryDate)}.{x}")
                                      , result.Results.Select(x => x.Resource));
        }
    }
}
