using Castle.Core.Resource;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Sels.Core;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Logging.Advanced;
using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.IntegrationTester.Tests
{
    /// <summary>
    /// Tests the thread safety of each provider.
    /// </summary>
    public class ConcurrencyTester : ITester
    {
        // Constants
        private const string TryLockType = "TryLock";
        private const string LockType = "Lock";
        private const string MixedType = "Mixed";

        // Fields
        private readonly ConcurrencyTesterOptions _options;
        private readonly ILogger? _logger;

        /// <inheritdoc cref="ConcurrencyTester"/>
        /// <param name="options">Contains the options for this instance</param>
        /// <param name="logger">Optional logger for tracing</param>
        public ConcurrencyTester(IOptions<ConcurrencyTesterOptions> options, ILogger<ConcurrencyTester>? logger = null)
        {
            _options = Guard.IsNotNull(options?.Value);
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> RunTests(TestProvider provider, ILockingProvider lockingProvider, CancellationToken token)
        {
            _logger.Log($"Running concurrency tests");
            HashSet<LockResult> results = new HashSet<LockResult>();

            Helper.Console.WriteLine(ConsoleColor.DarkGray, $"Concurrency settings:");
            Console.WriteLine($"Workers: {_options.Workers}");
            Console.WriteLine($"Max runtime: {_options.RunTime}");
            Console.WriteLine($"Max allowed attempts per worker: {_options.MaximumAttempts}");
            Console.WriteLine($"Sleep time after acquiring lock min/max: {_options.MinSleepTime}ms/{_options.MaxSleepTime}ms");
            Console.WriteLine($"Collision deviation: {_options.CollisionDeviation}ms");
            Console.WriteLine($"TryLock to Lock ratio: {_options.TryLockToLockRatio}");
            Console.WriteLine();

            _logger.Log($"Executing concurrency test {TryLockType}");
            var lockResults = await RunTryLockTests(lockingProvider, token);
            PrintResults(provider, TryLockType, lockResults, token);
            results.IntersectWith(lockResults);

            _logger.Log($"Executing concurrency test {LockType}");
            lockResults = await RunLockTests(lockingProvider, token);
            PrintResults(provider, LockType, lockResults, token);
            results.IntersectWith(lockResults);

            _logger.Log($"Executing concurrency test {MixedType}");
            lockResults = await RunMixedTests(lockingProvider, token);
            PrintResults(provider, MixedType, lockResults, token);
            results.IntersectWith(lockResults);

            return !results.Any(x => x.Exception != null);
        }

        private void PrintResults(TestProvider provider, string type, IEnumerable<LockResult> results, CancellationToken token)
        {
            type.ValidateArgumentNotNullOrWhitespace(nameof(type));
            results.ValidateArgumentNotNullOrEmpty(nameof(results));

            Helper.Console.WriteLine(ConsoleColor.DarkGray, $"Concurrency test {type} result for {provider}:");
            ThreadPool.GetMaxThreads(out int maxWorkers, out _);
            Console.WriteLine($"Thread pool current/maximum: {ThreadPool.ThreadCount}/{maxWorkers}");
            Console.WriteLine($"Lock attempts: {results.Count()}");
            Console.WriteLine($"Locks acquired: {results.Where(x => x.Acquired).Count()}");
            Console.WriteLine($"Assertions:");
            var errored = results.Where(x => x.Exception != null).ToArray();
            if (errored.HasValue())
            {
                Helper.Console.WriteLine(ConsoleColor.Red, $"[X] {errored.Length} errors:");
                foreach (var error in errored)
                {
                    Console.WriteLine($"({error.FinishedDate.Value.ToString("dd/MM/yyyy HH:mm:ss.fff")}) {error.WorkerId}: {error.Exception.Message.GetWithoutNewLine()}");
                }
            }
            else
            {
                Helper.Console.WriteLine(ConsoleColor.Green, "[V] No errors");
            }
            var collisions = GetCollisionAmount(results, token);
            if (collisions.HasValue())
            {
                Helper.Console.WriteLine(ConsoleColor.Red, $"[X] {collisions.Length} collisions detected:");
                foreach (var (Left, Right) in collisions)
                {
                    Console.WriteLine($"Collision between {Left.WorkerId} and {Right.WorkerId} at timeframes ({Left.AcquiredDate.Value.ToString("dd/MM/yyyy HH:mm:ss.fff")}<->{Left.FinishedDate.Value.ToString("dd/MM/yyyy HH:mm:ss.fff")}) and ({Right.AcquiredDate.Value.ToString("dd/MM/yyyy HH:mm:ss.fff")}<->{Right.FinishedDate.Value.ToString("dd/MM/yyyy HH:mm:ss.fff")})");
                }
            }
            else
            {
                Helper.Console.WriteLine(ConsoleColor.Green, "[V] No collisions");
            }
            Console.WriteLine();
        }

        private async Task<HashSet<LockResult>> RunTryLockTests(ILockingProvider lockingProvider, CancellationToken token)
        {
            const string resource = nameof(RunTryLockTests);
            CancellationTokenSource runTimeSource = new CancellationTokenSource();
            List<Task<HashSet<LockResult>>> workerTasks = new List<Task<HashSet<LockResult>>>();

            _logger.Log($"Testing the concurrency of TryLockAsync with <{_options.Workers}> workers for <{_options.RunTime}>");

            foreach(var workerId in Enumerable.Range(1, _options.Workers).Select(x => $"Worker {x}"))
            {
                var workerTask = Task.Run(async () =>
                {
                    HashSet<LockResult> results = new HashSet<LockResult>();
                    _logger.Log($"{workerId} starting to test TryLockAsync");

                    while (!runTimeSource.IsCancellationRequested && results.Count < _options.MaximumAttempts)
                    {
                        token.ThrowIfCancellationRequested();

                        var result = await ExecuteTryLock(resource, workerId, lockingProvider, token);

                        results.Add(result);
                    }

                    _logger.Log($"{workerId} finished testing TryLockAsync. Locked for a total of <{results.Count}> times");
                    return results;
                }, token);
                workerTasks.Add(workerTask);
            }


            runTimeSource.CancelAfter(_options.RunTime);
            _logger.Log($"Waiting for <{_options.Workers}> workers to run for <{_options.RunTime}>");
            var taskResults = await Task.WhenAll(workerTasks);
            return taskResults.SelectMany(x => x).ToHashSet();
        }

        private async Task<HashSet<LockResult>> RunLockTests(ILockingProvider lockingProvider, CancellationToken token)
        {
            const string resource = nameof(RunLockTests);
            CancellationTokenSource runTimeSource = new CancellationTokenSource();
            List<Task<HashSet<LockResult>>> workerTasks = new List<Task<HashSet<LockResult>>>();

            _logger.Log($"Testing the concurrency of LockAsync with <{_options.Workers}> workers for <{_options.RunTime}>");

            foreach (var workerId in Enumerable.Range(1, _options.Workers).Select(x => $"Worker {x}"))
            {
                var workerTask = Task.Run(async () =>
                {
                    HashSet<LockResult> results = new HashSet<LockResult>();
                    _logger.Log($"{workerId} starting to test LockAsync");

                    while (!runTimeSource.IsCancellationRequested && results.Count < _options.MaximumAttempts)
                    {
                        token.ThrowIfCancellationRequested();

                        var result = await ExecuteLock(resource, workerId, lockingProvider, token);

                        results.Add(result);
                    }

                    _logger.Log($"{workerId} finished testing LockAsync. Locked for a total of <{results.Count}> times");
                    return results;
                }, token);
                workerTasks.Add(workerTask);
            }


            runTimeSource.CancelAfter(_options.RunTime);
            _logger.Log($"Waiting for <{_options.Workers}> workers to run for <{_options.RunTime}>");
            var taskResults = await Task.WhenAll(workerTasks);
            return taskResults.SelectMany(x => x).ToHashSet();
        }

        private async Task<HashSet<LockResult>> RunMixedTests(ILockingProvider lockingProvider, CancellationToken token)
        {
            const string resource = nameof(RunMixedTests);
            CancellationTokenSource runTimeSource = new CancellationTokenSource();
            List<Task<HashSet<LockResult>>> workerTasks = new List<Task<HashSet<LockResult>>>();

            _logger.Log($"Testing the concurrency of mixed TryLockAsync and LockAsync with <{_options.Workers}> workers for <{_options.RunTime}>");

            foreach (var workerId in Enumerable.Range(1, _options.Workers).Select(x => $"Worker {x}"))
            {
                var workerTask = Task.Run(async () =>
                {
                    HashSet<LockResult> results = new HashSet<LockResult>();
                    _logger.Log($"{workerId} starting to test mixed TryLockAsync and LockAsync");

                    while (!runTimeSource.IsCancellationRequested && results.Count < _options.MaximumAttempts)
                    {
                        token.ThrowIfCancellationRequested();

                        var runTryLock = (Helper.Random.GetRandomDouble(0, 1) <= _options.TryLockToLockRatio);

                        LockResult result = null;
                        if (runTryLock)
                        {
                            result = await ExecuteTryLock(resource, workerId, lockingProvider, token);
                        }
                        else
                        {
                            result = await ExecuteLock(resource, workerId, lockingProvider, token);
                        }

                        results.Add(result);
                    }

                    _logger.Log($"{workerId} finished testing LockAsync. Locked for a total of <{results.Count}> times");
                    return results;
                }, token);
                workerTasks.Add(workerTask);
            }


            runTimeSource.CancelAfter(_options.RunTime);
            _logger.Log($"Waiting for <{_options.Workers}> workers to run for <{_options.RunTime}>");
            var taskResults = await Task.WhenAll(workerTasks);
            return taskResults.SelectMany(x => x).ToHashSet();
        }

        private async Task<LockResult> ExecuteTryLock(string resource, string workerId, ILockingProvider lockingProvider, CancellationToken token)
        {
            _logger.Debug($"{workerId} trying to lock resource <{resource}>");

            var result = new LockResult()
            {
                Type = TryLockType,
                WorkerId = workerId,
                StartedDate = DateTime.Now
            };
            try
            {
                var lockResult = await lockingProvider.TryLockAsync(resource, workerId, token: token);
                if (lockResult.Success)
                {
                    result.AcquiredDate = DateTime.Now;
                    _logger.Log($"{workerId} acquired lock on resource <{resource}>");
                    await using (lockResult.AcquiredLock)
                    {
                        await Helper.Async.Sleep(Helper.Random.GetRandomInt(_options.MinSleepTime, _options.MaxSleepTime), token);
                    }
                    result.FinishedDate = DateTime.Now;
                    _logger.Log($"{workerId} released lock on resource <{resource}>");
                }
                else
                {
                    result.FinishedDate = DateTime.Now;
                    _logger.Debug($"{workerId} could not acquire lock on resource <{resource}>");
                    await Helper.Async.Sleep(Helper.Random.GetRandomInt(_options.MinSleepTime, _options.MaxSleepTime), token);
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex;
                result.FinishedDate = DateTime.Now;
                _logger.Warning($"{workerId} encountered error while trying to lock resource <{resource}>", ex);
            }
            return result;
        }

        private async Task<LockResult> ExecuteLock(string resource, string workerId, ILockingProvider lockingProvider, CancellationToken token)
        {
            _logger.Debug($"{workerId} waiting to lock resource <{resource}>");

            var result = new LockResult()
            {
                Type = LockType,
                WorkerId = workerId,
                StartedDate = DateTime.Now
            };
            try
            {
                var lockResult = await lockingProvider.LockAsync(resource, workerId, token: token);
                result.AcquiredDate = DateTime.Now;
                _logger.Log($"{workerId} acquired lock on resource <{resource}>");
                await using (lockResult)
                {
                    await Helper.Async.Sleep(Helper.Random.GetRandomInt(_options.MinSleepTime, _options.MaxSleepTime), token);
                }
                result.FinishedDate = DateTime.Now;
                _logger.Log($"{workerId} released lock on resource <{resource}>");
            }
            catch (Exception ex)
            {
                result.Exception = ex;
                result.FinishedDate = DateTime.Now;
                _logger.Warning($"{workerId} encountered error while trying to lock resource <{resource}>", ex);
            }

            return result;
        }

        private (LockResult Left, LockResult Right)[] GetCollisionAmount(IEnumerable<LockResult> results, CancellationToken token)
        {
            _logger.Debug($"Detecting collisions");
            var acquiredResults = results.Where(x => x.Acquired);
            var detectedCollisions = new List<(LockResult Left, LockResult Right)>();
            object threadLock = new object();

            Parallel.ForEach(acquiredResults, (r, t) =>
            {
                token.ThrowIfCancellationRequested();
                var collisions = acquiredResults.Where(x => x != r)
                                                .Where(x =>
                                                {
                                                    var acquiredDate = x.AcquiredDate.Value;
                                                    if (acquiredDate > r.AcquiredDate.Value && acquiredDate < r.FinishedDate.Value) {
                                                        var difference = r.FinishedDate.Value - acquiredDate;
                                                        if(difference.TotalMilliseconds <= _options.CollisionDeviation)
                                                        {
                                                            _logger.Warning($"Detected collision for between <{x.WorkerId}> and <{r.WorkerId}> is below the configured deviation of <{_options.CollisionDeviation}ms>. Difference is <{difference.PrintTotalMs()}>. Not counting as collision");
                                                            return false;
                                                        }
                                                        else
                                                        {
                                                            _logger.Warning($"Worker <{x.WorkerId}> acquired lock at <{x.AcquiredDate.Value.ToString("dd/MM/yyyy HH:mm:ss.fff")}> while worker <{r.WorkerId}> held the lock between <{r.AcquiredDate.Value.ToString("dd/MM/yyyy HH:mm:ss.fff")}> and <{r.FinishedDate.Value.ToString("dd/MM/yyyy HH:mm:ss.fff")}>");
                                                            return true;
                                                        }
                                                    }
                                                    return false;
                                                });
                lock (threadLock)
                {
                    detectedCollisions.AddRange(collisions.Where(x => !detectedCollisions.Any(c => c.Left == x || c.Right == x)).Select(x => (r, x)));
                }
            });
            return detectedCollisions.ToArray();
        }

        /// <summary>
        /// Contains the result from trying lock a resource.
        /// </summary>
        private class LockResult
        {
            /// <summary>
            /// The method of locking used.
            /// </summary>
            public string Type { get; set; }
            /// <summary>
            /// The unique id of the worker that attempted the lock.
            /// </summary>
            public string WorkerId { get; set; }
            /// <summary>
            /// If the lock was acquired.
            /// </summary>
            public bool Acquired => AcquiredDate.HasValue;
            /// <summary>
            /// The date the request was started.
            /// </summary>
            public DateTime StartedDate { get; set; }
            /// <summary>
            /// The date when the lock was acquired.
            /// </summary>
            public DateTime? AcquiredDate { get; set; }
            /// <summary>
            /// The date the lock was unlocked if <see cref="AcquiredDate"/> is set, otherwise when the request was finished.
            /// </summary>
            public DateTime? FinishedDate { get; set; }
            /// <summary>
            /// Any exception throw when attempting a lock.
            /// </summary>
            public Exception? Exception { get; set; }
        }
    }
}
