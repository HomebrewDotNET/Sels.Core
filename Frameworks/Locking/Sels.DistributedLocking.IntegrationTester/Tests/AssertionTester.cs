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
        public async Task RunTests(TestProvider provider, ILockingProvider lockingProvider, CancellationToken token)
        {
            _logger.Log($"Running assertions");
            Dictionary<string, Exception?> _assertionResults = new Dictionary<string, Exception?>();

            foreach (var (name, assertion) in GetAssertions().Where(a => !_options.Filters.HasValue() || _options.Filters.All(f => Regex.IsMatch(a.Name, f))))
            {
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

            var resultBuilder = new StringBuilder();
            resultBuilder.AppendLine($"Assertion results for {provider}:");
            foreach (var (name, result) in _assertionResults)
            {
                resultBuilder.Append('[').Append(result != null ? "X" : "V").Append(']').AppendSpace().Append(name);
                if (result != null) resultBuilder.Append(':').AppendSpace().Append(result.GetType().Name).Append('(').Append(result.Message.GetWithoutNewLine()).AppendLine(")");
                else resultBuilder.AppendLine();
            }
            Console.WriteLine(resultBuilder);
        }

        private IEnumerable<(string Name, AsyncAction<ILockingProvider, CancellationToken> Assertion)> GetAssertions()
        {
            yield return (nameof(ResourceCanBeLockedAndUnlocked), ResourceCanBeLockedAndUnlocked);
            yield return (nameof(ResourceShouldStayLocked), ResourceShouldStayLocked);
            yield return (nameof(OwnerCanLockResourceAgain), OwnerCanLockResourceAgain);
            yield return (nameof(ResourceCanBeLockedAfterUnlock), ResourceCanBeLockedAfterUnlock);
            yield return (nameof(ResourceCanBeLockedAfterExpiry), ResourceCanBeLockedAfterExpiry);
            yield return (nameof(ExpiryDateGetsUpdatedManually), ExpiryDateGetsUpdatedManually);
            yield return (nameof(ExpiryDateGetsUpdatedAutomaticallyWithKeepAlive), ExpiryDateGetsUpdatedAutomaticallyWithKeepAlive);
            yield return (nameof(RequestsTakePriorityOverDirectCalls), RequestsTakePriorityOverDirectCalls);
            yield return (nameof(ResourceIsLockedWhenItIsFreeByExpectedRequester), ResourceIsLockedWhenItIsFreeByExpectedRequester);
            yield return (nameof(RequestIsPlacedWhenResourceIsLocked), RequestIsPlacedWhenResourceIsLocked);
            yield return (nameof(ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked), ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked);
            yield return (nameof(LockExpiryDateIsExtendedWhenKeepAliveIsEnabled), LockExpiryDateIsExtendedWhenKeepAliveIsEnabled);
            yield return (nameof(LockRequestIsProperlyTimedOut), LockRequestIsProperlyTimedOut);
            yield return (nameof(CancelingRequestThrowsOperationCanceledException), CancelingRequestThrowsOperationCanceledException);
            yield return (nameof(RequestGetsAssignedWhenLockExpiresWithCorrectSettings), RequestGetsAssignedWhenLockExpiresWithCorrectSettings);
        }

        private async Task ResourceCanBeLockedAndUnlocked(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Test locking
            var lockResult = await lockingProvider.TryLockAsync(nameof(ResourceCanBeLockedAndUnlocked), nameof(AssertionTester), token: token);
            Assert.IsTrue(lockResult.Success, "Expected lock to be placed but wasn't");

            await using (lockResult.AcquiredLock)
            {
                // Test state
                Assert.IsTrue(await lockResult.AcquiredLock.HasLockAsync(token), "Expected to have lock after acquirery but returned false");
            }

            // Test unlock
            var currentState = await lockingProvider.GetAsync(nameof(ResourceCanBeLockedAndUnlocked), token);
            Assert.IsTrue(currentState.CanLock("Other Requester"), "Expected lock to be free after unlock but wasn't");
        }

        private async Task ResourceShouldStayLocked(ILockingProvider lockingProvider, CancellationToken token)
        {
            // First lock
            var lockResult = await lockingProvider.TryLockAsync(nameof(ResourceShouldStayLocked), nameof(AssertionTester), token: token);
            Assert.IsTrue(lockResult.Success, "Expected lock to be placed but wasn't");

            // Second lock
            var secondLockResult = await lockingProvider.TryLockAsync(nameof(ResourceShouldStayLocked), "SecondRequester", token: token);
            Assert.IsFalse(secondLockResult.Success, "Expected second lock to fail but was successful");

            // Release lock
            await lockResult.AcquiredLock.DisposeAsync();
        }

        private async Task OwnerCanLockResourceAgain(ILockingProvider lockingProvider, CancellationToken token)
        {
            // First lock
            var lockResult = await lockingProvider.TryLockAsync(nameof(OwnerCanLockResourceAgain), nameof(AssertionTester), token: token);
            Assert.IsTrue(lockResult.Success, "Expected lock to be placed but wasn't");

            // Second lock
            var secondLockResult = await lockingProvider.TryLockAsync(nameof(OwnerCanLockResourceAgain), nameof(AssertionTester), token: token);
            Assert.IsTrue(secondLockResult.Success, "Expected owner to receive lock again");

            // Release lock
            await secondLockResult.AcquiredLock.DisposeAsync();
        }

        private async Task ResourceCanBeLockedAfterUnlock(ILockingProvider lockingProvider, CancellationToken token)
        {
            // First lock
            var lockResult = await lockingProvider.TryLockAsync(nameof(ResourceCanBeLockedAfterUnlock), $"{nameof(AssertionTester)}.1", token: token);
            Assert.IsTrue(lockResult.Success, "Expected lock to be placed but wasn't");

            // Release lock
            await lockResult.AcquiredLock.DisposeAsync();

            // Second lock
            var secondLockResult = await lockingProvider.TryLockAsync(nameof(ResourceCanBeLockedAfterUnlock), $"{nameof(AssertionTester)}.2", token: token);
            Assert.IsTrue(secondLockResult.Success, "Expected lock to be placed after unlock but wasn't");

            // Release lock
            await secondLockResult.AcquiredLock.DisposeAsync();
        }

        private async Task ResourceCanBeLockedAfterExpiry(ILockingProvider lockingProvider, CancellationToken token)
        {
            const int ExpirySeconds = 1;

            // First lock
            var lockResult = await lockingProvider.TryLockAsync(nameof(ResourceCanBeLockedAfterExpiry), $"{nameof(AssertionTester)}.1", TimeSpan.FromSeconds(ExpirySeconds), token: token);
            var currentExpiry = lockResult.AcquiredLock.ExpiryDate;
            Assert.That(currentExpiry, Is.Not.Null, "Expected expiry date to be set");
            Assert.IsTrue(lockResult.Success, "Expected lock to be placed but wasn't");

            // Wait until original expiry
            await Helper.Async.Sleep((currentExpiry.Value - DateTime.Now).Add(TimeSpan.FromSeconds(1)), token);

            // Second lock
            var secondLockResult = await lockingProvider.TryLockAsync(nameof(ResourceCanBeLockedAfterExpiry), $"{nameof(AssertionTester)}.2", token: token);
            Assert.IsTrue(secondLockResult.Success, "Expected lock to be placed after expiry but wasn't");

            // Release lock
            await secondLockResult.AcquiredLock.DisposeAsync();
        }

        private async Task ExpiryDateGetsUpdatedManually(ILockingProvider lockingProvider, CancellationToken token)
        {
            // First lock
            var lockResult = await lockingProvider.TryLockAsync(nameof(ExpiryDateGetsUpdatedManually), $"{nameof(AssertionTester)}.1", TimeSpan.FromHours(1), token: token);
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

        private async Task ExpiryDateGetsUpdatedAutomaticallyWithKeepAlive(ILockingProvider lockingProvider, CancellationToken token)
        {
            const int ExpirySeconds = 4;

            // First lock
            var lockResult = await lockingProvider.TryLockAsync(nameof(ExpiryDateGetsUpdatedAutomaticallyWithKeepAlive), nameof(AssertionTester), TimeSpan.FromSeconds(ExpirySeconds), true, token: token);
            Assert.IsTrue(lockResult.Success, "Expected lock to be placed but wasn't");
            var currentExpiry = lockResult.AcquiredLock.ExpiryDate;
            Assert.That(currentExpiry, Is.Not.Null, "Expected expiry date to be set");

            // Wait for expiry
            await Helper.Async.Sleep(currentExpiry.Value-DateTime.Now, token);

            // Get new expiry
            var newExpiry = lockResult.AcquiredLock.ExpiryDate;
            Assert.That(currentExpiry, Is.Not.Null, "Expected new expiry date to be set");

            // Assert
            Assert.That(newExpiry, Is.GreaterThan(currentExpiry), "Expected updated expiry date to be greater than the initial expiry date");

            // Release lock
            await lockResult.AcquiredLock.DisposeAsync();
        }

        private async Task RequestsTakePriorityOverDirectCalls(ILockingProvider lockingProvider, CancellationToken token)
        {
            ILock requestResult = null;
            // First lock
            var lockResult = await lockingProvider.TryLockAsync(nameof(RequestsTakePriorityOverDirectCalls), $"{nameof(AssertionTester)}.1", token: token);
            Assert.IsTrue(lockResult.Success, "Expected lock to be placed but wasn't");

            // Log request
            var lockRequest = lockingProvider.LockAsync(nameof(RequestsTakePriorityOverDirectCalls), $"{nameof(AssertionTester)}.2", token: token);

            // Wait a bit for the request to flush
            await Helper.Async.Sleep(TimeSpan.FromSeconds(1), token);

            // Release lock
            await lockResult.AcquiredLock.UnlockAsync(token);

            if (lockRequest.IsCompleted) requestResult = await lockRequest;

            // Try lock 
            var secondLockResult = await lockingProvider.TryLockAsync(nameof(RequestsTakePriorityOverDirectCalls), $"{nameof(AssertionTester)}.3", token: token);
            Assert.IsFalse(secondLockResult.Success, "Expected direct lock to fail but succeeded");

            // Wait for request
            requestResult = await Helper.Async.WaitOn(lockRequest, TimeSpan.FromMinutes(1), token);
            Assert.That(requestResult, Is.Not.Null, "Lock request returned null");
        }

        private async Task ResourceIsLockedWhenItIsFreeByExpectedRequester(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Lock
            var @lock = await lockingProvider.LockAsync(nameof(ResourceIsLockedWhenItIsFreeByExpectedRequester), nameof(AssertionTester), timeout: TimeSpan.FromSeconds(10));

            Assert.IsNotNull(@lock);
            await using (@lock)
            {
                Assert.That(@lock.Resource, Is.EqualTo(nameof(ResourceIsLockedWhenItIsFreeByExpectedRequester)));
                Assert.That(@lock.LockedBy, Is.EqualTo(nameof(AssertionTester)));
                Assert.That(@lock.ExpiryDate, Is.Null);
            }
        }

        private async Task RequestIsPlacedWhenResourceIsLocked(ILockingProvider lockingProvider, CancellationToken token)
        {
            // First place lock
            var initialLock = await lockingProvider.LockAsync(nameof(RequestIsPlacedWhenResourceIsLocked), $"{nameof(AssertionTester)}.1", timeout: TimeSpan.FromSeconds(10));

            Assert.IsNotNull(initialLock);
            Assert.That(initialLock.Resource, Is.EqualTo(nameof(RequestIsPlacedWhenResourceIsLocked)));
            Assert.That(initialLock.LockedBy, Is.EqualTo($"{nameof(AssertionTester)}.1"));
            Assert.That(initialLock.ExpiryDate, Is.Null);

            // Log request
            var lockTask = lockingProvider.LockAsync(nameof(RequestIsPlacedWhenResourceIsLocked), $"{nameof(AssertionTester)}.2");
            Assert.IsNotNull(lockTask);

            // Check if pending request exists
            var pendingRequests = await lockingProvider.GetPendingRequestsAsync(nameof(RequestIsPlacedWhenResourceIsLocked));
            Assert.IsNotNull(pendingRequests);
            Assert.That(pendingRequests.Length, Is.EqualTo(1));
            var request = pendingRequests[0];
            Assert.IsNotNull(request);
            Assert.That(request.Resource, Is.EqualTo(nameof(RequestIsPlacedWhenResourceIsLocked)));
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

        private async Task ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Place locks
            var lockOne = await lockingProvider.LockAsync(nameof(ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked), $"{nameof(AssertionTester)}.0");
            _ = lockingProvider.LockAsync(nameof(ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked), $"{nameof(AssertionTester)}.1");
            await Helper.Async.Sleep(TimeSpan.FromMilliseconds(50));
            _ = lockingProvider.LockAsync(nameof(ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked), $"{nameof(AssertionTester)}.2");
            await Helper.Async.Sleep(TimeSpan.FromMilliseconds(50));
            _ = lockingProvider.LockAsync(nameof(ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked), $"{nameof(AssertionTester)}.3");
            await Helper.Async.Sleep(TimeSpan.FromMilliseconds(50));

            // Get pending requests
            var pendingRequests = await lockingProvider.GetPendingRequestsAsync(nameof(ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked));

            // Assert
            Assert.IsNotNull(lockOne);
            Assert.That(lockOne.Resource, Is.EqualTo(nameof(ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked)));
            Assert.That(lockOne.LockedBy, Is.EqualTo($"{nameof(AssertionTester)}.0"));
            Assert.That(lockOne.ExpiryDate, Is.Null);
            Assert.IsNotNull(pendingRequests);
            Assert.That(pendingRequests.Length, Is.EqualTo(3));
            for (int i = 0; i < pendingRequests.Length; i++)
            {
                Assert.IsNotNull(pendingRequests[i]);
                Assert.That(pendingRequests[i].Resource, Is.EqualTo(nameof(ExpectedNumberOfRequestsAreCreatedWhenResourceIsLocked)));
                Assert.That(pendingRequests[i].Requester, Is.EqualTo($"{nameof(AssertionTester)}.{i + 1}"));
                Assert.That(pendingRequests[i].ExpiryTime, Is.Null);
                Assert.That(pendingRequests[i].Timeout, Is.Null);
                Assert.That(pendingRequests[i].KeepAlive, Is.False);
            }
        }

        private async Task LockExpiryDateIsExtendedWhenKeepAliveIsEnabled(ILockingProvider lockingProvider, CancellationToken token)
        {
            const int expiryTime = 100;

            // Place lock
            await using (var @lock = await lockingProvider.LockAsync(nameof(LockExpiryDateIsExtendedWhenKeepAliveIsEnabled), nameof(AssertionTester), TimeSpan.FromMilliseconds(expiryTime), true))
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

        private async Task LockRequestIsProperlyTimedOut(ILockingProvider lockingProvider, CancellationToken token)
        {
            Exception exception = null;
            const int WaitTime = 1;

            // Place intial lock
            var initialLock = await lockingProvider.LockAsync(nameof(LockRequestIsProperlyTimedOut), $"{nameof(AssertionTester)}.1", timeout: TimeSpan.FromSeconds(10));

            // Place second lock that should timeout
            var lockResultTask = lockingProvider.LockAsync(nameof(LockRequestIsProperlyTimedOut), $"{nameof(AssertionTester)}.2", timeout: TimeSpan.FromSeconds(WaitTime));

            // Give time for lock to timeout
            await Helper.Async.Sleep(TimeSpan.FromSeconds(WaitTime * 2));
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
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception, Is.InstanceOf<LockTimeoutException>());
            var timeoutException = exception as LockTimeoutException;
            Assert.That(timeoutException.Lock, Is.Not.Null);
            Assert.That(timeoutException.Lock.Resource, Is.EqualTo(nameof(LockRequestIsProperlyTimedOut)));
            Assert.That(timeoutException.Lock.LockedBy, Is.EqualTo($"{nameof(AssertionTester)}.1"));
            Assert.That(timeoutException.Requester, Is.EqualTo($"{nameof(AssertionTester)}.2"));
            Assert.That(timeoutException.Timeout, Is.EqualTo(TimeSpan.FromSeconds(WaitTime)));

            // Release
            await initialLock.DisposeAsync();
        }

        private async Task CancelingRequestThrowsOperationCanceledException(ILockingProvider lockingProvider, CancellationToken token)
        {
            Exception exception = null;
            var tokenSource = new CancellationTokenSource();

            // Place intial lock
            var initialLock = await lockingProvider.LockAsync(nameof(CancelingRequestThrowsOperationCanceledException), $"{nameof(AssertionTester)}.1", timeout: TimeSpan.FromSeconds(10));

            // Place second lock that should cancel
            var lockResultTask = lockingProvider.LockAsync(nameof(CancelingRequestThrowsOperationCanceledException), $"{nameof(AssertionTester)}.2", token: tokenSource.Token);

            // Cancel request and wait for cancel to be acknowledged
            await Helper.Async.Sleep(TimeSpan.FromSeconds(1));
            tokenSource.Cancel();
            await Helper.Async.Sleep(TimeSpan.FromSeconds(1));
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
            Assert.That(exception, Is.Not.Null);
            Assert.That(exception, Is.InstanceOf<OperationCanceledException>());
        }

        private async Task RequestGetsAssignedWhenLockExpiresWithCorrectSettings(ILockingProvider lockingProvider, CancellationToken token)
        {
            // Place intial lock with expiry
            var initialLock = await lockingProvider.LockAsync(nameof(RequestGetsAssignedWhenLockExpiresWithCorrectSettings), $"{nameof(AssertionTester)}.1", expiryTime: TimeSpan.FromSeconds(1));

            // Log second request
            var lockResultTask = lockingProvider.LockAsync(nameof(RequestGetsAssignedWhenLockExpiresWithCorrectSettings), $"{nameof(AssertionTester)}.2", TimeSpan.FromMinutes(5), true);

            // Give time for request to be assigned
            await Helper.Async.Sleep(TimeSpan.FromSeconds(2));

            // Assert
            Assert.That(lockResultTask, Is.Not.Null);
            Assert.That(lockResultTask.IsCompleted, Is.True);
            var @lock = await lockResultTask;
            Assert.IsNotNull(@lock);
            Assert.That(@lock.Resource, Is.EqualTo(nameof(RequestGetsAssignedWhenLockExpiresWithCorrectSettings)));
            Assert.That(@lock.LockedBy, Is.EqualTo($"{nameof(AssertionTester)}.2"));
            Assert.That(@lock.ExpiryDate, Is.Not.Null);

            // Release
            await @lock.DisposeAsync();
        }
    }
}
