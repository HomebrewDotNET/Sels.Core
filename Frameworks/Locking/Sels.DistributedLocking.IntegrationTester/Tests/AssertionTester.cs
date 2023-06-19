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

namespace Sels.DistributedLocking.IntegrationTester.Tests
{
    /// <summary>
    /// Runs a simple test on each feature of <see cref="ILockingProvider"/>.
    /// </summary>
    public class AssertionTester : ITester
    {
        // Fields
        private readonly ILogger? _logger;

        /// <inheritdoc cref="AssertionTester"/>
        /// <param name="logger">Optional logger for tracing</param>
        public AssertionTester(ILogger<AssertionTester>? logger = null)
        {
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task RunTests(TestProvider provider, ILockingProvider lockingProvider, CancellationToken token)
        {
            _logger.Log($"Running assertions");
            Dictionary<string, Exception?> _assertionResults = new Dictionary<string, Exception?>();

            foreach (var (name, assertion) in GetAssertions())
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

        private IEnumerable<(string, AsyncAction<ILockingProvider, CancellationToken>)> GetAssertions()
        {
            yield return (nameof(ResourceCanBeLockedAndUnlocked), ResourceCanBeLockedAndUnlocked);
            yield return (nameof(ResourceShouldStayLocked), ResourceShouldStayLocked);
            yield return (nameof(OwnerCanLockResourceAgain), OwnerCanLockResourceAgain);
            yield return (nameof(ResourceCanBeLockedAfterUnlock), ResourceCanBeLockedAfterUnlock);
            yield return (nameof(ResourceCanBeLockedAfterExpiry), ResourceCanBeLockedAfterExpiry);
            yield return (nameof(ExpiryDateGetsUpdatedManually), ExpiryDateGetsUpdatedManually);
            yield return (nameof(ExpiryDateGetsUpdatedAutomaticallyWithKeepAlive), ExpiryDateGetsUpdatedAutomaticallyWithKeepAlive);
            yield return (nameof(RequestsTakePriorityOverDirectCalls), RequestsTakePriorityOverDirectCalls);
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
    }
}
