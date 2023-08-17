using Castle.Core.Resource;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using Sels.Core;
using Sels.Core.Extensions;
using Sels.Core.Extensions.Calculation;
using Sels.Core.Extensions.Conversion;
using Sels.Core.Extensions.Logging;
using Sels.Core.Models;
using Sels.DistributedLocking.Abstractions.Models;
using Sels.DistributedLocking.Provider;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sels.DistributedLocking.IntegrationTester.Tests
{
    /// <summary>
    /// Tests the performance of each tester.
    /// </summary>
    public class BenchmarkTester : ITester
    {
        // Fields
        private readonly BenchmarkTesterOptions _options;
        private readonly ILogger? _logger;

        /// <inheritdoc cref="ConcurrencyTester"/>
        /// <param name="options">Contains the options for this instance</param>
        /// <param name="logger">Optional logger for tracing</param>
        public BenchmarkTester(IOptions<BenchmarkTesterOptions> options, ILogger<BenchmarkTester>? logger = null)
        {
            _options = Guard.IsNotNull(options?.Value);
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<bool> RunTests(TestProvider provider, ILockingProvider lockingProvider, CancellationToken token)
        {
            _logger.Log($"Preparing to run benchmark for provider <{provider}>");

            // Seed storage
            await SeedStorage(lockingProvider, token);

            Helper.Console.WriteLine(ConsoleColor.DarkGray, $"Benchmark settings:");
            Console.WriteLine($"Workers: {_options.Workers}");
            Console.WriteLine($"Storage size: {_options.StorageSize}");
            Console.WriteLine($"Expiry ratio: {_options.ExpiryRatio}");
            Console.WriteLine($"Expired ratio: {_options.ExpiredRatio}");
            Console.WriteLine($"Unlocked ratio: {_options.UnlockedRatio}");
            Console.WriteLine($"Max runtime: {_options.RunTime}");
            Console.WriteLine($"Max allowed attempts per worker: {_options.MaximumAttempts}");
            Console.WriteLine($"Lock resource pool size: {_options.ResourcePoolSize}");
            Console.WriteLine($"Query result set size: {_options.QueryResultSetSize}");
            Console.WriteLine($"TryLock to Lock ratio: {_options.TryLockToLockRatio}");
            Console.WriteLine();

            bool anyFailed = false;
            _logger.Log($"Running benchmark tests with {_options.Workers} workers for provider <{provider}>");
            var failed = await RunBenchmarks(_options.Workers, provider, lockingProvider, token);
            if (!anyFailed) anyFailed = failed;

            _logger.Log($"Running benchmark tests with single worker for provider <{provider}>");
            failed = await RunBenchmarks(1, provider, lockingProvider, token);
            if (!anyFailed) anyFailed = failed;

            return !anyFailed;
        }

        private async Task<bool> RunBenchmarks(int workers, TestProvider provider, ILockingProvider lockingProvider, CancellationToken token)
        {
            Helper.Console.WriteLine(ConsoleColor.DarkGray, $"Benchmarked with {workers} worker(s):");
            Console.WriteLine();

            HashSet<LockBenchmarkResult> lockBenchmarkResults = null;
            bool failed = false;

            // Try lock no pool
            const string TryLockNoPool = "TryLockAsync - No pool";
            Ref<TimeSpan> duration;
            using (Helper.Time.CaptureDuration(out duration))
            {
                lockBenchmarkResults = await BenchmarkTryLock(workers, null, lockingProvider, token);
                if (!failed) failed = lockBenchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(TryLockNoPool, provider, duration.Value, lockBenchmarkResults);
            token.ThrowIfCancellationRequested();

            // Try lock pool
            string TryLockPool = $"TryLockAsync - Pool of {_options.ResourcePoolSize}";
            using (Helper.Time.CaptureDuration(out duration))
            {
                lockBenchmarkResults = await BenchmarkTryLock(workers, _options.ResourcePoolSize, lockingProvider, token);
                if (!failed) failed = lockBenchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(TryLockPool, provider, duration.Value, lockBenchmarkResults);
            token.ThrowIfCancellationRequested();

            // Lock no pool
            const string LockNoPool = "LockAsync - No pool";
            using (Helper.Time.CaptureDuration(out duration))
            {
                lockBenchmarkResults = await BenchmarkLock(workers, null, lockingProvider, token);
                if (!failed) failed = lockBenchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(LockNoPool, provider, duration.Value, lockBenchmarkResults);
            token.ThrowIfCancellationRequested();

            // Lock pool
            string LockPool = $"LockAsync - Pool of {_options.ResourcePoolSize}";
            using (Helper.Time.CaptureDuration(out duration))
            {
                lockBenchmarkResults = await BenchmarkLock(workers, _options.ResourcePoolSize, lockingProvider, token);
                if (!failed) failed = lockBenchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(LockPool, provider, duration.Value, lockBenchmarkResults);
            token.ThrowIfCancellationRequested();

            // Mixed lock no pool
            const string MixedLockNoPool = "TryLockAsync/LockAsync - No pool";
            using (Helper.Time.CaptureDuration(out duration))
            {
                lockBenchmarkResults = await BenchmarkMixedLock(workers, null, lockingProvider, token);
                if (!failed) failed = lockBenchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(MixedLockNoPool, provider, duration.Value, lockBenchmarkResults);
            token.ThrowIfCancellationRequested();

            // Mixed lock pool
            string MixedLockPool = $"TryLockAsync/LockAsync - Pool of {_options.ResourcePoolSize}";
            using (Helper.Time.CaptureDuration(out duration))
            {
                lockBenchmarkResults = await BenchmarkMixedLock(workers, _options.ResourcePoolSize, lockingProvider, token);
                if (!failed) failed = lockBenchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(MixedLockPool, provider, duration.Value, lockBenchmarkResults);
            token.ThrowIfCancellationRequested();

            // Get
            HashSet<BenchmarkResult> benchmarkResults = null;
            const string Get = "GetAsync";
            using (Helper.Time.CaptureDuration(out duration))
            {
                benchmarkResults = await BenchmarkGet(workers, _options.ResourcePoolSize, lockingProvider, token);
                if (!failed) failed = benchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(Get, provider, duration.Value, benchmarkResults);
            token.ThrowIfCancellationRequested();

            // GetPending
            const string GetPending = "GetPendingRequestsAsync";
            using (Helper.Time.CaptureDuration(out duration))
            {
                benchmarkResults = await BenchmarkGetPendingRequests(workers, _options.ResourcePoolSize, lockingProvider, token);
                if (!failed) failed = benchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(GetPending, provider, duration.Value, benchmarkResults);
            token.ThrowIfCancellationRequested();

            // ForceUnlock
            const string ForceUnlock = "ForceUnlockAsync";
            using (Helper.Time.CaptureDuration(out duration))
            {
                benchmarkResults = await BenchmarkForceUnlock(workers, _options.ResourcePoolSize, lockingProvider, token);
                if (!failed) failed = benchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(ForceUnlock, provider, duration.Value, benchmarkResults);
            token.ThrowIfCancellationRequested();

            // Query filter on resource
            const string QueryFilterResource = "QueryAsync - Filter on resource";
            using (Helper.Time.CaptureDuration(out duration))
            {
                benchmarkResults = await BenchmarkQuery(workers, lockingProvider, token, x => x.WithFilterOnResource(nameof(BenchmarkQuery)), (false, null, false));
                if (!failed) failed = benchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(QueryFilterResource, provider, duration.Value, benchmarkResults);
            token.ThrowIfCancellationRequested();

            // Query filter on locked by
            const string QueryFilterLockedBy = "QueryAsync - Filter on locked by";
            using (Helper.Time.CaptureDuration(out duration))
            {
                benchmarkResults = await BenchmarkQuery(workers, lockingProvider, token, x => x.WithFilterOnLockedBy("Tester"), (false, null, false));
                if (!failed) failed = benchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(QueryFilterLockedBy, provider, duration.Value, benchmarkResults);
            token.ThrowIfCancellationRequested();

            // Query filter on equal to locked by
            const string QueryFilterEqualToLockedBy = "QueryAsync - Filter on equal to locked by";
            using (Helper.Time.CaptureDuration(out duration))
            {
                benchmarkResults = await BenchmarkQuery(workers, lockingProvider, token, x => x.WithLockedByEqualTo(nameof(BenchmarkTester)), (false, null, false));
                if (!failed) failed = benchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(QueryFilterEqualToLockedBy, provider, duration.Value, benchmarkResults);
            token.ThrowIfCancellationRequested();

            // Query with pending requests
            const string QueryWithPendingRequests = "QueryAsync - With pending requests";
            using (Helper.Time.CaptureDuration(out duration))
            {
                benchmarkResults = await BenchmarkQuery(workers, lockingProvider, token, x => x.WithPendingRequestsLargerThan(0), (false, null, true));
                if (!failed) failed = benchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(QueryWithPendingRequests, provider, duration.Value, benchmarkResults);
            token.ThrowIfCancellationRequested();

            // Query only expired
            const string QueryOnlyExpired = "QueryAsync - Only expired";
            using (Helper.Time.CaptureDuration(out duration))
            {
                benchmarkResults = await BenchmarkQuery(workers, lockingProvider, token, x => x.WithOnlyExpired(), (false, true, false));
                if (!failed) failed = benchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(QueryOnlyExpired, provider, duration.Value, benchmarkResults);
            token.ThrowIfCancellationRequested();

            // Query only not expired
            const string QueryOnlyNotExpired = "QueryAsync - Only not expired";
            using (Helper.Time.CaptureDuration(out duration))
            {
                benchmarkResults = await BenchmarkQuery(workers, lockingProvider, token, x => x.WithOnlyNotExpired(), (false, false, false));
                if (!failed) failed = benchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(QueryOnlyNotExpired, provider, duration.Value, benchmarkResults);
            token.ThrowIfCancellationRequested();

            // Query only locked
            const string QueryOnlyLocked = "QueryAsync - Only locked";
            using (Helper.Time.CaptureDuration(out duration))
            {
                benchmarkResults = await BenchmarkQuery(workers, lockingProvider, token, x => x.WithOnlyLocked(), (false, null, false));
                if (!failed) failed = benchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(QueryOnlyLocked, provider, duration.Value, benchmarkResults);
            token.ThrowIfCancellationRequested();

            // Query only noy locked
            const string QueryOnlyNotLocked = "QueryAsync - Only not locked";
            using (Helper.Time.CaptureDuration(out duration))
            {
                benchmarkResults = await BenchmarkQuery(workers, lockingProvider, token, x => x.WithOnlyNotLocked(), (true, null, false));
                if (!failed) failed = benchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(QueryOnlyNotLocked, provider, duration.Value, benchmarkResults);
            token.ThrowIfCancellationRequested();

            // Query order by resource
            const string QueryOrderByResource = "QueryAsync - Order by resource";
            using (Helper.Time.CaptureDuration(out duration))
            {
                benchmarkResults = await BenchmarkQuery(workers, lockingProvider, token, x => x.OrderByResource(), (false, null, false));
                if (!failed) failed = benchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(QueryOrderByResource, provider, duration.Value, benchmarkResults);
            token.ThrowIfCancellationRequested();

            // Query order by resource desc
            const string QueryOrderByResourceDesc = "QueryAsync - Order by resource desc";
            using (Helper.Time.CaptureDuration(out duration))
            {
                benchmarkResults = await BenchmarkQuery(workers, lockingProvider, token, x => x.OrderByResource(true), (false, null, false));
                if (!failed) failed = benchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(QueryOrderByResourceDesc, provider, duration.Value, benchmarkResults);
            token.ThrowIfCancellationRequested();

            // Query order by locked by
            const string QueryOrderByLockedBy = "QueryAsync - Order by locked by";
            using (Helper.Time.CaptureDuration(out duration))
            {
                benchmarkResults = await BenchmarkQuery(workers, lockingProvider, token, x => x.OrderByLockedBy(), (false, null, false));
                if (!failed) failed = benchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(QueryOrderByLockedBy, provider, duration.Value, benchmarkResults);
            token.ThrowIfCancellationRequested();

            // Query order by locked by desc
            const string QueryOrderByLockedByDesc = "QueryAsync - Order by locked by desc";
            using (Helper.Time.CaptureDuration(out duration))
            {
                benchmarkResults = await BenchmarkQuery(workers, lockingProvider, token, x => x.OrderByLockedBy(true), (false, null, false));
                if (!failed) failed = benchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(QueryOrderByLockedByDesc, provider, duration.Value, benchmarkResults);
            token.ThrowIfCancellationRequested();

            // Query order by last lock date
            const string QueryOrderByLastLockDate = "QueryAsync - Order by last lock date";
            using (Helper.Time.CaptureDuration(out duration))
            {
                benchmarkResults = await BenchmarkQuery(workers, lockingProvider, token, x => x.OrderByLastLockDate(), (false, null, false));
                if (!failed) failed = benchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(QueryOrderByLastLockDate, provider, duration.Value, benchmarkResults);
            token.ThrowIfCancellationRequested();

            // Query order by last lock date desc
            const string QueryOrderByLastLockDateDesc = "QueryAsync - Order by last lock date desc";
            using (Helper.Time.CaptureDuration(out duration))
            {
                benchmarkResults = await BenchmarkQuery(workers, lockingProvider, token, x => x.OrderByLastLockDate(true), (false, null, false));
                if (!failed) failed = benchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(QueryOrderByLastLockDateDesc, provider, duration.Value, benchmarkResults);
            token.ThrowIfCancellationRequested();

            // Query order by locked at
            const string QueryOrderByLockedAt = "QueryAsync - Order by locked at";
            using (Helper.Time.CaptureDuration(out duration))
            {
                benchmarkResults = await BenchmarkQuery(workers, lockingProvider, token, x => x.OrderByLockedAt(), (false, null, false));
                if (!failed) failed = benchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(QueryOrderByLockedAt, provider, duration.Value, benchmarkResults);
            token.ThrowIfCancellationRequested();

            // Query order by locked at desc
            const string QueryOrderByLockedAtDesc = "QueryAsync - Order by locked at desc";
            using (Helper.Time.CaptureDuration(out duration))
            {
                benchmarkResults = await BenchmarkQuery(workers, lockingProvider, token, x => x.OrderByLockedAt(true), (false, null, false));
                if (!failed) failed = benchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(QueryOrderByLockedAtDesc, provider, duration.Value, benchmarkResults);
            token.ThrowIfCancellationRequested();

            // Query order by expiry date
            const string QueryOrderByExpiryDate = "QueryAsync - Order by expiry date";
            using (Helper.Time.CaptureDuration(out duration))
            {
                benchmarkResults = await BenchmarkQuery(workers, lockingProvider, token, x => x.OrderByExpiryDate(), (false, null, false));
                if (!failed) failed = benchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(QueryOrderByExpiryDate, provider, duration.Value, benchmarkResults);
            token.ThrowIfCancellationRequested();

            // Query order by locked at desc
            const string QueryOrderByExpiryDateDesc = "QueryAsync - Order by expiry date desc";
            using (Helper.Time.CaptureDuration(out duration))
            {
                benchmarkResults = await BenchmarkQuery(workers, lockingProvider, token, x => x.OrderByExpiryDate(true), (false, null, false));
                if (!failed) failed = benchmarkResults.Any(x => x.Exception != null);
            }
            PrintResults(QueryOrderByExpiryDateDesc, provider, duration.Value, benchmarkResults);
            token.ThrowIfCancellationRequested();

            return failed;
        }

        private async Task<HashSet<LockBenchmarkResult>> BenchmarkTryLock(int workerAmount, int? poolSize, ILockingProvider lockingProvider, CancellationToken token)
        {
            CancellationTokenSource runTimeSource = new CancellationTokenSource();
            List<Task<HashSet<LockBenchmarkResult>>> workerTasks = new List<Task<HashSet<LockBenchmarkResult>>>();
            var resourcePool = poolSize.HasValue ? Enumerable.Range(1, poolSize.Value).Select(x => $"{nameof(BenchmarkTryLock)}.Pool.{x}").ToArray() : null;

            // Run benchmark
            _logger.Log($"Benchmarking TryLockAsync using {(poolSize.HasValue ? $"resource pool of <{poolSize.Value}>" : "no resource pool")}");
            foreach (var workerNumber in Enumerable.Range(1, workerAmount))
            {
                var workerTask = Task.Run(async () =>
                {
                    var workerId = $"Worker {workerNumber}";
                    HashSet<LockBenchmarkResult> results = new HashSet<LockBenchmarkResult>();
                    _logger.Log($"{workerId} starting to benchmark TryLockAsync");

                    while (!runTimeSource.IsCancellationRequested && results.Count < _options.MaximumAttempts)
                    {
                        token.ThrowIfCancellationRequested();

                        var resource = resourcePool != null ? resourcePool.GetRandomItem() : $"{nameof(BenchmarkTryLock)}.{workerNumber}.{results.Count}";
                        var result = await ExecuteTryLock(resource, workerId, lockingProvider, token);

                        results.Add(result);
                    }

                    _logger.Log($"{workerId} finished benchmarking TryLockAsync. Locked for a total of <{results.Count}> times");
                    return results;
                }, token);
                workerTasks.Add(workerTask);
            }

            // Wait for workers to finish
            runTimeSource.CancelAfter(_options.RunTime);
            _logger.Log($"Waiting for <{_options.Workers}> workers to run for <{_options.RunTime}>");
            var taskResults = await Task.WhenAll(workerTasks);
            return taskResults.SelectMany(x => x).ToHashSet();
        }

        private async Task<HashSet<LockBenchmarkResult>> BenchmarkLock(int workerAmount, int? poolSize, ILockingProvider lockingProvider, CancellationToken token)
        {
            CancellationTokenSource runTimeSource = new CancellationTokenSource();
            List<Task<HashSet<LockBenchmarkResult>>> workerTasks = new List<Task<HashSet<LockBenchmarkResult>>>();
            var resourcePool = poolSize.HasValue ? Enumerable.Range(1, poolSize.Value).Select(x => $"{nameof(BenchmarkLock)}.Pool.{x}").ToArray() : null;

            // Run benchmark
            _logger.Log($"Benchmarking LockAsync using {(poolSize.HasValue ? $"resource pool of <{poolSize.Value}>" : "no resource pool")}");
            foreach (var workerNumber in Enumerable.Range(1, workerAmount))
            {
                var workerTask = Task.Run(async () =>
                {
                    var workerId = $"Worker {workerNumber}";
                    HashSet<LockBenchmarkResult> results = new HashSet<LockBenchmarkResult>();
                    _logger.Log($"{workerId} starting to benchmark LockAsync");

                    while (!runTimeSource.IsCancellationRequested && results.Count < _options.MaximumAttempts)
                    {
                        token.ThrowIfCancellationRequested();

                        var resource = resourcePool != null ? resourcePool.GetRandomItem() : $"{nameof(BenchmarkLock)}.{workerNumber}.{results.Count}";
                        var result = await ExecuteLock(resource, workerId, lockingProvider, token);

                        results.Add(result);
                    }

                    _logger.Log($"{workerId} finished benchmarking LockAsync. Locked for a total of <{results.Count}> times");
                    return results;
                }, token);
                workerTasks.Add(workerTask);
            }

            // Wait for workers to finish
            runTimeSource.CancelAfter(_options.RunTime);
            _logger.Log($"Waiting for <{_options.Workers}> workers to run for <{_options.RunTime}>");
            var taskResults = await Task.WhenAll(workerTasks);
            return taskResults.SelectMany(x => x).ToHashSet();
        }

        private async Task<HashSet<LockBenchmarkResult>> BenchmarkMixedLock(int workerAmount, int? poolSize, ILockingProvider lockingProvider, CancellationToken token)
        {
            CancellationTokenSource runTimeSource = new CancellationTokenSource();
            List<Task<HashSet<LockBenchmarkResult>>> workerTasks = new List<Task<HashSet<LockBenchmarkResult>>>();
            var resourcePool = poolSize.HasValue ? Enumerable.Range(1, poolSize.Value).Select(x => $"{nameof(BenchmarkMixedLock)}.Pool.{x}").ToArray() : null;

            // Run benchmark
            _logger.Log($"Benchmarking mix of TryLockAsync/LockAsync using {(poolSize.HasValue ? $"resource pool of <{poolSize.Value}>" : "no resource pool")}");
            foreach (var workerNumber in Enumerable.Range(1, workerAmount))
            {
                var workerTask = Task.Run(async () =>
                {
                    var workerId = $"Worker {workerNumber}";
                    HashSet<LockBenchmarkResult> results = new HashSet<LockBenchmarkResult>();
                    _logger.Log($"{workerId} starting to benchmark mix of TryLockAsync/LockAsync");

                    while (!runTimeSource.IsCancellationRequested && results.Count < _options.MaximumAttempts)
                    {
                        token.ThrowIfCancellationRequested();

                        var resource = resourcePool != null ? resourcePool.GetRandomItem() : $"{nameof(BenchmarkMixedLock)}.{workerNumber}.{results.Count}";
                        var runTryLock = (Helper.Random.GetRandomDouble(0, 1) <= _options.TryLockToLockRatio);

                        LockBenchmarkResult result = null;
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

                    _logger.Log($"{workerId} finished benchmarking mix of TryLockAsync/LockAsync. Locked for a total of <{results.Count}> times");
                    return results;
                }, token);
                workerTasks.Add(workerTask);
            }

            // Wait for workers to finish
            runTimeSource.CancelAfter(_options.RunTime);
            _logger.Log($"Waiting for <{_options.Workers}> workers to run for <{_options.RunTime}>");
            var taskResults = await Task.WhenAll(workerTasks);
            return taskResults.SelectMany(x => x).ToHashSet();
        }

        private async Task<HashSet<BenchmarkResult>> BenchmarkGet(int workerAmount, int poolSize, ILockingProvider lockingProvider, CancellationToken token)
        {
            CancellationTokenSource runTimeSource = new CancellationTokenSource();
            List<Task<HashSet<BenchmarkResult>>> workerTasks = new List<Task<HashSet<BenchmarkResult>>>();

            // Get pool of locks
            var queryResult = await lockingProvider.QueryAsync(x => x.OrderByLastLockDate().WithPagination(1, poolSize), token);
            var resources = queryResult.Results.Select(x => x.Resource).ToList();
            if (resources.Count < poolSize)
            {
                await Task.WhenAll(Enumerable.Range(1, poolSize - resources.Count).DivideIntoHashSet(_options.Workers).Select(x => Task.Run(async () =>
                {
                    _logger.Log($"Resources to benchmark is under the pool size of <{poolSize}>. Creating additional locks");
                    foreach (var number in x)
                    {
                        var resource = $"{nameof(BenchmarkGet)}.{number}";
                        _ = await lockingProvider.TryLockAsync(resource, nameof(BenchmarkTester), token: token);
                        lock (resources)
                        {
                            resources.Add(resource);
                        }
                    }
                }, token)));
            }

            // Run benchmark
            _logger.Log($"Benchmarking GetAsync");
            foreach (var workerNumber in Enumerable.Range(1, workerAmount))
            {
                var workerTask = Task.Run(async () =>
                {
                    var workerId = $"Worker {workerNumber}";
                    HashSet<BenchmarkResult> results = new HashSet<BenchmarkResult>();
                    _logger.Log($"{workerId} starting to benchmark GetAsync");

                    while (!runTimeSource.IsCancellationRequested && results.Count < _options.MaximumAttempts)
                    {
                        token.ThrowIfCancellationRequested();

                        var resource = resources.GetRandomItem();

                        var result = new BenchmarkResult()
                        {
                            StartedDate = DateTime.Now
                        };

                        try
                        {
                            // Get lock
                            Ref<TimeSpan> duration;
                            using (Helper.Time.CaptureDuration(out duration))
                            {
                                _ = await lockingProvider.GetAsync(resource, token);
                            }
                            result.FinishedDate = DateTime.Now;
                            result.Duration = duration.Value;
                        }
                        catch(Exception ex)
                        {
                            result.Exception = ex;
                            result.FinishedDate = DateTime.Now;
                            _logger.Log($"{workerId} encountered error while trying to fetch resource <{resource}>", ex);
                        }

                        results.Add(result);
                    }

                    _logger.Log($"{workerId} finished benchmarking GetAsync. Fetched for a total of <{results.Count}> times");
                    return results;
                }, token);
                workerTasks.Add(workerTask);
            }

            // Wait for workers to finish
            runTimeSource.CancelAfter(_options.RunTime);
            _logger.Log($"Waiting for <{_options.Workers}> workers to run for <{_options.RunTime}>");
            var taskResults = await Task.WhenAll(workerTasks);
            return taskResults.SelectMany(x => x).ToHashSet();
        }

        private async Task<HashSet<BenchmarkResult>> BenchmarkGetPendingRequests(int workerAmount, int poolSize, ILockingProvider lockingProvider, CancellationToken token)
        {
            CancellationTokenSource runTimeSource = new CancellationTokenSource();
            List<Task<HashSet<BenchmarkResult>>> workerTasks = new List<Task<HashSet<BenchmarkResult>>>();

            // Get pool of locks
            var queryResult = await lockingProvider.QueryAsync(x => x.OrderByLastLockDate().WithPendingRequestsLargerThan(0).WithPagination(1, poolSize), token);
            var resources = queryResult.Results.Select(x => x.Resource).ToList();
            if (resources.Count < poolSize)
            {
                await Task.WhenAll(Enumerable.Range(1, poolSize - resources.Count).DivideIntoHashSet(_options.Workers).Select(x => Task.Run(async () =>
                {
                    _logger.Log($"Resources to benchmark is under the pool size of <{poolSize}>. Creating additional locks");
                    foreach (var number in x)
                    {
                        var resource = $"{nameof(BenchmarkGetPendingRequests)}.{number}";
                        await lockingProvider.TryLockAsync(resource, nameof(BenchmarkTester), token: token);

                        // Create requests
                        foreach (var i in Enumerable.Range(1, Helper.Random.GetRandomInt(1, poolSize)))
                        {
                            _ = await lockingProvider.LockAsync(resource, $"{nameof(BenchmarkTester)}.{i}", token: runTimeSource.Token);
                        }

                        lock (resources)
                        {
                            resources.Add(resource);
                        }
                    }
                }, token)));
            }

            // Run benchmark
            _logger.Log($"Benchmarking GetPendingRequestsAsync");
            foreach (var workerNumber in Enumerable.Range(1, workerAmount))
            {
                var workerTask = Task.Run(async () =>
                {
                    var workerId = $"Worker {workerNumber}";
                    HashSet<BenchmarkResult> results = new HashSet<BenchmarkResult>();
                    _logger.Log($"{workerId} starting to benchmark GetPendingRequestsAsync");

                    while (!runTimeSource.IsCancellationRequested && results.Count < _options.MaximumAttempts)
                    {
                        token.ThrowIfCancellationRequested();

                        var resource = resources.GetRandomItem();

                        var result = new BenchmarkResult() {
                            StartedDate = DateTime.Now
                        };

                        try
                        {
                            // Get requests
                            Ref<TimeSpan> duration;
                            using (Helper.Time.CaptureDuration(out duration))
                            {
                                _ = await lockingProvider.GetPendingRequestsAsync(resource, token);
                            }
                            result.FinishedDate = DateTime.Now;
                            result.Duration = duration.Value;
                        }
                        catch (Exception ex)
                        {
                            result.Exception = ex;
                            result.FinishedDate = DateTime.Now;
                            _logger.Log($"{workerId} encountered error while trying to fetch resource <{resource}>", ex);
                        }

                        results.Add(result);
                    }

                    _logger.Log($"{workerId} finished benchmarking GetPendingRequestsAsync. Fetched for a total of <{results.Count}> times");
                    return results;
                }, token);
                workerTasks.Add(workerTask);
            }

            // Wait for workers to finish
            runTimeSource.CancelAfter(_options.RunTime);
            _logger.Log($"Waiting for <{_options.Workers}> workers to run for <{_options.RunTime}>");
            var taskResults = await Task.WhenAll(workerTasks);
            return taskResults.SelectMany(x => x).ToHashSet();
        }

        private async Task<HashSet<BenchmarkResult>> BenchmarkForceUnlock(int workerAmount, int poolSize, ILockingProvider lockingProvider, CancellationToken token)
        {
            CancellationTokenSource runTimeSource = new CancellationTokenSource();
            List<Task<HashSet<BenchmarkResult>>> workerTasks = new List<Task<HashSet<BenchmarkResult>>>();

            // Get pool of locks
            var queryResult = await lockingProvider.QueryAsync(x => x.OrderByLockedAt(true).WithPagination(1, workerAmount* poolSize), token);
            var resources = queryResult.Results.Select(x => x.Resource).ToList();
            if (resources.Count < poolSize)
            {
                await Task.WhenAll(Enumerable.Range(1, poolSize - resources.Count).DivideIntoHashSet(_options.Workers).Select(x => Task.Run(async () =>
                {
                    _logger.Log($"Resources to benchmark is under the pool size of <{poolSize}>. Creating additional locks");
                    foreach (var number in x)
                    {
                        var resource = $"{nameof(BenchmarkForceUnlock)}.{resources.Count}";
                        _ = await lockingProvider.TryLockAsync(resource, nameof(BenchmarkTester), token: token);

                        lock (resources)
                        {
                            resources.Add(resource);
                        }
                    }
                }, token)));
            }

            // Run benchmark
            _logger.Log($"Benchmarking ForceUnlockAsync");
            foreach (var workerNumber in Enumerable.Range(1, workerAmount))
            {
                var workerTask = Task.Run(async () =>
                {
                    var workerId = $"Worker {workerNumber}";
                    HashSet<BenchmarkResult> results = new HashSet<BenchmarkResult>();
                    _logger.Log($"{workerId} starting to benchmark ForceUnlockAsync");

                    while (!runTimeSource.IsCancellationRequested && results.Count < _options.MaximumAttempts)
                    {
                        token.ThrowIfCancellationRequested();

                        var resource = resources.GetRandomItem();
                        var result = new BenchmarkResult()
                        {
                            StartedDate = DateTime.Now
                        };

                        try
                        {
                            // Force unlock
                            Ref<TimeSpan> duration;
                            using (Helper.Time.CaptureDuration(out duration))
                            {
                                await lockingProvider.ForceUnlockAsync(resource, true, token);
                            }
                            result.FinishedDate = DateTime.Now;
                            result.Duration = duration.Value;
                        }
                        catch (Exception ex)
                        {
                            result.Exception = ex;
                            result.FinishedDate = DateTime.Now;
                            _logger.Log($"{workerId} encountered error while trying to force unlock resource <{resource}>", ex);
                        }

                        results.Add(result);
                    }

                    _logger.Log($"{workerId} finished benchmarking ForceUnlockAsync. Force unlocked for a total of <{results.Count}> times");
                    return results;
                }, token);
                workerTasks.Add(workerTask);
            }

            // Wait for workers to finish
            runTimeSource.CancelAfter(_options.RunTime);
            _logger.Log($"Waiting for <{_options.Workers}> workers to run for <{_options.RunTime}>");
            var taskResults = await Task.WhenAll(workerTasks);
            return taskResults.SelectMany(x => x).ToHashSet();
        }

        private async Task<HashSet<BenchmarkResult>> BenchmarkQuery(int workerAmount, ILockingProvider lockingProvider, CancellationToken token, Func<ILockQueryCriteria, ILockQueryCriteria> criteria, (bool Unlock, bool? Expire, bool CreateRequest) seedSettings)
        {
            CancellationTokenSource runTimeSource = new CancellationTokenSource();
            List<Task<HashSet<BenchmarkResult>>> workerTasks = new List<Task<HashSet<BenchmarkResult>>>();

            // Get pool of locks
            var guid = Guid.NewGuid();
            var querySize = _options.QueryResultSetSize;
            var queryResult = await lockingProvider.QueryAsync(x => criteria(x).WithPagination(1, querySize), token);
            if(queryResult.Results.Length < querySize)
            {
                await Task.WhenAll(Enumerable.Range(1, querySize - queryResult.Results.Length).DivideIntoHashSet(_options.Workers).Select(x => Task.Run(async () =>
                {
                    _logger.Log($"Resources to benchmark is under the pool size of <{querySize}>. Creating additional locks");
                    foreach (var number in x)
                    {
                        var resource = $"{nameof(BenchmarkQuery)}.{guid}.{number}";
                        var lockResult = await lockingProvider.TryLockAsync(resource, nameof(BenchmarkTester), seedSettings.Expire.HasValue ? seedSettings.Expire.Value ? TimeSpan.Zero : TimeSpan.FromDays(365) : null, token: token);

                        if (seedSettings.CreateRequest)
                        {
                            _ = await lockingProvider.LockAsync(resource, $"{nameof(BenchmarkTester)}.{number}", token: runTimeSource.Token);
                        }

                        if (lockResult.Success && seedSettings.Unlock)
                        {
                            await lockResult.AcquiredLock.DisposeAsync();
                        }
                    }
                }, token)));
            }

            // Run benchmark
            _logger.Log($"Benchmarking QueryAsync");
            foreach (var workerNumber in Enumerable.Range(1, workerAmount))
            {
                var workerTask = Task.Run(async () =>
                {
                    var workerId = $"Worker {workerNumber}";
                    HashSet<BenchmarkResult> results = new HashSet<BenchmarkResult>();
                    _logger.Log($"{workerId} starting to benchmark QueryAsync");

                    while (!runTimeSource.IsCancellationRequested && results.Count < _options.MaximumAttempts)
                    {
                        token.ThrowIfCancellationRequested();

                        var result = new BenchmarkResult()
                        {
                            StartedDate = DateTime.Now
                        };

                        try
                        {
                            // Get lock
                            Ref<TimeSpan> duration;
                            using (Helper.Time.CaptureDuration(out duration))
                            {
                                _ = await lockingProvider.QueryAsync(x => criteria(x).WithPagination(1, querySize), token);
                            }
                            result.FinishedDate = DateTime.Now;
                            result.Duration = duration.Value;
                        }
                        catch (Exception ex)
                        {
                            result.Exception = ex;
                            result.FinishedDate = DateTime.Now;
                            _logger.Log($"{workerId} encountered error while querying locks", ex);
                        }

                        results.Add(result);
                    }

                    _logger.Log($"{workerId} finished benchmarking QueryAsync. Queried for a total of <{results.Count}> times");
                    return results;
                }, token);
                workerTasks.Add(workerTask);
            }

            // Wait for workers to finish
            runTimeSource.CancelAfter(_options.RunTime);
            _logger.Log($"Waiting for <{_options.Workers}> workers to run for <{_options.RunTime}>");
            var taskResults = await Task.WhenAll(workerTasks);
            return taskResults.SelectMany(x => x).ToHashSet();
        }

        private async Task<LockBenchmarkResult> ExecuteTryLock(string resource, string workerId, ILockingProvider lockingProvider, CancellationToken token)
        {
            _logger.Debug($"{workerId} trying to lock resource <{resource}>");

            var result = new LockBenchmarkResult()
            {
                StartedDate = DateTime.Now
            };
            try
            {
                Ref<TimeSpan> duration;
                ILockResult lockResult;

                using(Helper.Time.CaptureDuration(out duration))
                {
                    lockResult = await lockingProvider.TryLockAsync(resource, workerId, token: token);
                }
                result.FinishedDate = DateTime.Now;
                result.Duration = duration.Value;
                if (lockResult.Success)
                {
                    result.Acquired = true;
                    await using (lockResult.AcquiredLock)
                    {
                        _logger.Log($"{workerId} acquired lock on resource <{resource}>");

                        using (Helper.Time.CaptureDuration(out duration))
                        {
                            await lockResult.AcquiredLock.UnlockAsync(token);
                        }
                        result.UnlockDuration = duration.Value;
                        _logger.Log($"{workerId} released lock on resource <{resource}>");
                    }
                }
                else
                {
                    _logger.Debug($"{workerId} could not acquire lock on resource <{resource}>");
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex;
                result.FinishedDate = DateTime.Now;
                _logger.Log($"{workerId} encountered error while trying to lock resource <{resource}>", ex);
            }
            return result;
        }

        private async Task<LockBenchmarkResult> ExecuteLock(string resource, string workerId, ILockingProvider lockingProvider, CancellationToken token)
        {
            _logger.Debug($"{workerId} trying to lock resource <{resource}>");

            var result = new LockBenchmarkResult()
            {
                StartedDate = DateTime.Now
            };
            try
            {
                Ref<TimeSpan> duration;
                ILock lockResult;
                using (Helper.Time.CaptureDuration(out duration))
                {
                    lockResult = await lockingProvider.LockAndWaitAsync(resource, workerId, token: token);
                }
                result.FinishedDate = DateTime.Now;
                result.Duration = duration.Value;
                result.Acquired = true;

                await using (lockResult)
                {
                    _logger.Log($"{workerId} acquired lock on resource <{resource}>");

                    using (Helper.Time.CaptureDuration(out duration))
                    {
                        await lockResult.UnlockAsync(token);
                    }
                    result.UnlockDuration = duration.Value;
                    _logger.Log($"{workerId} released lock on resource <{resource}>");
                }
            }
            catch (Exception ex)
            {
                result.Exception = ex;
                result.FinishedDate = DateTime.Now;
                _logger.Log($"{workerId} encountered error while trying to lock resource <{resource}>", ex);
            }
            return result;
        }

        private void PrintResults(string benchmarkName, TestProvider provider, TimeSpan duration, HashSet<LockBenchmarkResult> results)
        {
            benchmarkName.ValidateArgumentNotNullOrWhitespace(nameof(benchmarkName));
            results.ValidateArgumentNotNullOrEmpty(nameof(results));

            var acquiredLocks = results.Where(x => x.Acquired).ToHashSet();
            var failedLocks = results.Where(x => x.Exception != null).ToHashSet();

            Helper.Console.WriteLine(ConsoleColor.DarkGray, $"Benchmark results of <{benchmarkName}> for provider {provider}:");
            Console.WriteLine($"Total runtime: {duration}");
            ThreadPool.GetMaxThreads(out int maxWorkers, out _);
            Console.WriteLine($"Thread pool current/maximum: {ThreadPool.ThreadCount}/{maxWorkers}");
            Console.WriteLine($"Lock attempts: {results.Count}");
            Console.WriteLine($"Locks acquired: {acquiredLocks.Count}");
            Console.WriteLine($"Lock attempts failed: {failedLocks.Count}");
            var min = results.HasValue() ? results.Min(x => x.Duration).TotalMilliseconds : 0;
            var avg = results.HasValue() ? results.Average(x => x.Duration.TotalMilliseconds) : 0; 
            var med = results.HasValue() ? results.Median(x => x.Duration.TotalMilliseconds) : 0;
            var max = results.HasValue() ? results.Max(x => x.Duration).TotalMilliseconds : 0;
            Console.WriteLine($"Lock duration min/med/avg/max: {min.RoundTo(2, MidpointRounding.ToPositiveInfinity)}ms/{med.RoundTo(2, MidpointRounding.ToPositiveInfinity)}ms/{avg.RoundTo(2, MidpointRounding.ToPositiveInfinity)}ms/{max.RoundTo(2, MidpointRounding.ToPositiveInfinity)}ms");
            min = acquiredLocks.HasValue() ? acquiredLocks.Min(x => x.UnlockDuration.Value).TotalMilliseconds : 0;
            avg = acquiredLocks.HasValue() ? acquiredLocks.Average(x => x.UnlockDuration.Value.TotalMilliseconds) : 0;
            med = acquiredLocks.HasValue() ? acquiredLocks.Median(x => x.Duration.TotalMilliseconds) : 0;
            max = acquiredLocks.HasValue() ? acquiredLocks.Max(x => x.UnlockDuration.Value).TotalMilliseconds : 0;
            Console.WriteLine($"Unlock duration min/med/avg/max: {min.RoundTo(2, MidpointRounding.ToPositiveInfinity)}ms/{med.RoundTo(2, MidpointRounding.ToPositiveInfinity)}ms/{avg.RoundTo(2, MidpointRounding.ToPositiveInfinity)}ms/{max.RoundTo(2, MidpointRounding.ToPositiveInfinity)}ms");
            var average = results.HasValue() ? results.GroupAsDictionary(x => x.FinishedDate.ToString("dd/MM/yyyy-HH:mm:ss")).Average(x => x.Value.Count) : 0;
            Console.WriteLine($"Average lock attempts per second: {average.RoundTo(2, MidpointRounding.ToPositiveInfinity)}");
            average = acquiredLocks.HasValue() ? acquiredLocks.GroupAsDictionary(x => x.FinishedDate.ToString("dd/MM/yyyy-HH:mm:ss")).Average(x => x.Value.Count) : 0;
            Console.WriteLine($"Average locks acquired per second: {average.RoundTo(2, MidpointRounding.ToPositiveInfinity)}");
            average = failedLocks.HasValue() ? failedLocks.GroupAsDictionary(x => x.FinishedDate.ToString("dd/MM/yyyy-HH:mm:ss")).Average(x => x.Value.Count) : 0;
            Console.WriteLine($"Average failed lock attempts per second: {average.RoundTo(2, MidpointRounding.ToPositiveInfinity)}");
            Console.WriteLine();
        }

        private void PrintResults(string benchmarkName, TestProvider provider, TimeSpan duration, HashSet<BenchmarkResult> results)
        {
            benchmarkName.ValidateArgumentNotNullOrWhitespace(nameof(benchmarkName));
            results.ValidateArgumentNotNullOrEmpty(nameof(results));

            var successfulActions = results.Where(x => x.Exception == null).ToHashSet();
            var failedActions = results.Where(x => x.Exception != null).ToHashSet();

            Helper.Console.WriteLine(ConsoleColor.DarkGray, $"Benchmark results of <{benchmarkName}> for provider {provider}:");
            Console.WriteLine($"Total runtime: {duration}");
            ThreadPool.GetMaxThreads(out int maxWorkers, out _);
            Console.WriteLine($"Thread pool current/maximum: {ThreadPool.ThreadCount}/{maxWorkers}");
            Console.WriteLine($"Actions performed: {results.Count}");
            Console.WriteLine($"Actions performed successfully: {successfulActions.Count}");
            Console.WriteLine($"Actions failed: {failedActions.Count}");
            var min = results.HasValue() ? results.Min(x => x.Duration).TotalMilliseconds : 0;
            var avg = results.HasValue() ? results.Average(x => x.Duration.TotalMilliseconds) : 0;
            var med = results.HasValue() ? results.Median(x => x.Duration.TotalMilliseconds) : 0;
            var max = results.HasValue() ? results.Max(x => x.Duration).TotalMilliseconds : 0;
            Console.WriteLine($"Action duration min/med/avg/max: {min.RoundTo(2, MidpointRounding.ToPositiveInfinity)}ms/{med.RoundTo(2, MidpointRounding.ToPositiveInfinity)}ms/{avg.RoundTo(2, MidpointRounding.ToPositiveInfinity)}ms/{max.RoundTo(2, MidpointRounding.ToPositiveInfinity)}ms");
            var average = results.HasValue() ? results.GroupAsDictionary(x => x.FinishedDate.ToString("dd/MM/yyyy-HH:mm:ss")).Average(x => x.Value.Count) : 0;
            Console.WriteLine($"Average actions per second: {average.RoundTo(2, MidpointRounding.ToPositiveInfinity)}");
            average = successfulActions.HasValue() ? successfulActions.GroupAsDictionary(x => x.FinishedDate.ToString("dd/MM/yyyy-HH:mm:ss")).Average(x => x.Value.Count) : 0;
            Console.WriteLine($"Average successful actions per second: {average.RoundTo(2, MidpointRounding.ToPositiveInfinity)}");
            average = failedActions.HasValue() ? failedActions.GroupAsDictionary(x => x.FinishedDate.ToString("dd/MM/yyyy-HH:mm:ss")).Average(x => x.Value.Count) : 0;
            Console.WriteLine($"Average failed actions per second: {average.RoundTo(2, MidpointRounding.ToPositiveInfinity)}");
            Console.WriteLine();
        }

        private async Task SeedStorage(ILockingProvider lockingProvider, CancellationToken token)
        {
            var seedSize = _options.StorageSize;
            _logger.Log($"Persisting <{seedSize}> locks before starting benchmark");

            await Task.WhenAll(Enumerable.Range(1, seedSize).DivideIntoHashSet(_options.Workers).Select(x => Task.Run(async () =>
            {
                foreach (var number in x)
                {
                    token.ThrowIfCancellationRequested();
                    var resource = $"PersistedLock.{number}";
                    _logger.Debug($"Persisting lock <{resource}>");
                    var hasExpiry = Helper.Random.GetRandomDouble(0, 1) <= _options.ExpiryRatio;
                    var lockResult = await lockingProvider.TryLockAsync(resource, nameof(BenchmarkTester), hasExpiry ? Helper.Random.GetRandomDouble(0, 1) <= _options.ExpiredRatio ? TimeSpan.Zero : TimeSpan.FromDays(365) : null, token: token);
                    if (lockResult.Success && !lockResult.AcquiredLock.ExpiryDate.HasValue && (Helper.Random.GetRandomDouble(0, 1) <= _options.UnlockedRatio))
                    {
                        await lockResult.AcquiredLock.DisposeAsync();
                    }
                }
            }, token)));
        }

        /// <summary>
        /// The result from executing an action.
        /// </summary>
        private class BenchmarkResult
        {
            /// <summary>
            /// The time the benchmark action was started.
            /// </summary>
            public DateTime StartedDate { get; set; }
            /// <summary>
            /// How long the action took to execute.
            /// </summary>
            public TimeSpan Duration { get; set; }
            /// <summary>
            /// When the action was finished.
            /// </summary>
            public DateTime FinishedDate { get; set; }
            /// <summary>
            /// The exception if the action could not execute properly.
            /// </summary>
            public Exception? Exception { get; set; }
        }

        /// <summary>
        /// The result from executing a lock action.
        /// </summary>
        private class LockBenchmarkResult : BenchmarkResult
        {
            /// <summary>
            /// If the lock was acquired.
            /// </summary>
            public bool Acquired { get; set; }
            /// <summary>
            /// How long the unlock took. Only set when <see cref="Acquired"/> is true.
            /// </summary>
            public TimeSpan? UnlockDuration { get; set; }
        }
    }
}
