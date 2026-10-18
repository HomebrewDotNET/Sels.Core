using Sels.Core.Entity;

namespace Sels.Core.Legacy.Tests
{
    /// <summary>Contract tests for <see cref="Helper"/>: expression-based object comparison, asynchronous and synchronous waiting on tasks, and log template formatting.</summary>
    [Trait("Category", "Unit")]
    [Collection(LegacyTestCollection.Name)]
    public class HelperTests
    {
        /// <summary>Generated comparison handles equal, skipped, strict, and equivalent properties.</summary>
        [Fact]
        public void GeneratePropertyComparer_ContractCases_ReturnsExpectedResults()
        {
            var compare = Helper.Expressions.Comparing.GeneratePropertyComparer<ComparableModel>(false);
            var looseCompare = Helper.Expressions.Comparing.GeneratePropertyComparer<ComparableModel>(false);
            var strictCompare = Helper.Expressions.Comparing.GeneratePropertyComparer<ComparableModel>(true);
            var skipCompare = Helper.Expressions.Comparing.GeneratePropertyComparer<ComparableModel>(false, false, nameof(ComparableModel.Skipped));
            var equivalentCompare = Helper.Expressions.Comparing.GeneratePropertyComparer<ComparableModel>(false, true);

            var equal = compare(new ComparableModel { Value = 2 }, new ComparableModel { Value = 2 });
            var different = compare(new ComparableModel { Value = 2 }, new ComparableModel { Value = 3 });
            var skipped = skipCompare(new ComparableModel { Value = 2, Skipped = "left" }, new ComparableModel { Value = 2, Skipped = "right" });
            var baseAndDerivedLoose = looseCompare(new ComparableModel { Value = 2 }, new DerivedComparableModel { Value = 2 });
            var baseAndDerivedStrict = strictCompare(new ComparableModel { Value = 2 }, new DerivedComparableModel { Value = 2 });
            var equivalent = equivalentCompare(
                new ComparableModel { Child = new ComparableKey("same") },
                new ComparableModel { Child = new ComparableKey("same") });

            Assert.True(equal);
            Assert.False(different);
            Assert.True(skipped);
            Assert.True(baseAndDerivedLoose);
            Assert.False(baseAndDerivedStrict);
            Assert.True(equivalent);
        }

        /// <summary>Different default and nondefault property values are not equivalent.</summary>
        [Fact]
        public void GeneratePropertyComparer_DefaultAndNondefaultProperty_ReturnsFalse()
        {
            var compare = Helper.Expressions.Comparing.GeneratePropertyComparer<ComparableModel>(false);

            var result = compare(new ComparableModel { Value = 0 }, new ComparableModel { Value = 1 });

            Assert.False(result);
        }

        /// <summary>Generic and runtime-type comparer overloads agree and reject null types.</summary>
        [Fact]
        public void GeneratePropertyComparer_OverloadsAndNullType_PreserveContract()
        {
            var generic = Helper.Expressions.Comparing.GeneratePropertyComparer<ComparableModel>(false);
            var runtime = Helper.Expressions.Comparing.GeneratePropertyComparer(typeof(ComparableModel), false);
            var left = new ComparableModel { Value = 4 };
            var right = new ComparableModel { Value = 4 };

            var genericResult = generic(left, right);
            var runtimeResult = runtime(left, right);
            var nullTypeException = Assert.Throws<ArgumentNullException>(() => Helper.Expressions.Comparing.GeneratePropertyComparer(null!, false));
            var nullSkipException = Assert.Throws<ArgumentNullException>(() => Helper.Expressions.Comparing.GeneratePropertyComparer(typeof(ComparableModel), false, false, null!));

            Assert.Equal(genericResult, runtimeResult);
            Assert.Equal("type", nullTypeException.ParamName);
            Assert.Equal("skipPropertyNames", nullSkipException.ParamName);
        }

        /// <summary>Verifies AsyncWaitOn for task completes in time: returns.</summary>
        [Fact(Timeout = LegacyTestConstants.TestTimeoutMilliseconds)]
        public async Task AsyncWaitOn_TaskCompletesInTime_Returns()
        {
            // Arrange

            // Act
            var delayedTask = Task.Delay(1000);
            await Helper.Async.WaitOn(delayedTask, TimeSpan.FromMinutes(1));

            // Assert
            Assert.True(delayedTask.IsCompleted);
        }

        /// <summary>Verifies AsyncWaitOn for task exceeds max wait time: throws timeout exception.</summary>
        [Fact(Timeout = LegacyTestConstants.TestTimeoutMilliseconds)]
        public async Task AsyncWaitOn_TaskExceedsMaxWaitTime_ThrowsTimeoutException()
        {
            // Arrange
            var taskSource = new TaskCompletionSource();
            Exception exception = null;

            // Act
            try
            {
                await Helper.Async.WaitOn(taskSource.Task, TimeSpan.FromMilliseconds(1));
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<TimeoutException>(exception);
        }

        /// <summary>Verifies AsyncWaitOn for token cancelled: throws task canceled exception.</summary>
        [Fact(Timeout = LegacyTestConstants.TestTimeoutMilliseconds)]
        public async Task AsyncWaitOn_TokenCancelled_ThrowsTaskCanceledException()
        {
            // Arrange
            var taskSource = new TaskCompletionSource();
            var cancellationTokenSource = new CancellationTokenSource();
            Exception exception = null;

            // Act
            try
            {
                cancellationTokenSource.CancelAfter(1000);
                await Helper.Async.WaitOn(taskSource.Task, TimeSpan.FromDays(1), cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<TaskCanceledException>(exception);
        }

        /// <summary>Verifies AsyncWaitOn for generic task completes in time: returns.</summary>
        [Fact(Timeout = LegacyTestConstants.TestTimeoutMilliseconds)]
        public async Task AsyncWaitOn_GenericTaskCompletesInTime_Returns()
        {
            // Arrange
            var taskSource = new TaskCompletionSource<object>();

            // Act
            _ = Task.Run(async () =>
            {
                await Helper.Async.Sleep(1000);
                taskSource.SetResult(new object());
            });
            await Helper.Async.WaitOn(taskSource.Task, TimeSpan.FromMinutes(1));

            // Assert
            Assert.True(taskSource.Task.IsCompleted);
        }

        /// <summary>Verifies AsyncWaitOn for generic task exceeds max wait time: throws timeout exception.</summary>
        [Fact(Timeout = LegacyTestConstants.TestTimeoutMilliseconds)]
        public async Task AsyncWaitOn_GenericTaskExceedsMaxWaitTime_ThrowsTimeoutException()
        {
            // Arrange
            var taskSource = new TaskCompletionSource<object>();
            Exception exception = null;

            // Act
            try
            {
                await Helper.Async.WaitOn(taskSource.Task, TimeSpan.FromMilliseconds(1));
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<TimeoutException>(exception);
        }

        /// <summary>Verifies AsyncWaitOn for generic task token cancelled: throws task canceled exception.</summary>
        [Fact(Timeout = LegacyTestConstants.TestTimeoutMilliseconds)]
        public async Task AsyncWaitOn_GenericTaskTokenCancelled_ThrowsTaskCanceledException()
        {
            // Arrange
            var taskSource = new TaskCompletionSource<object>();
            var cancellationTokenSource = new CancellationTokenSource();
            Exception exception = null;

            // Act
            try
            {
                cancellationTokenSource.CancelAfter(1000);
                await Helper.Async.WaitOn(taskSource.Task, TimeSpan.FromDays(1), cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<TaskCanceledException>(exception);
        }

        /// <summary>Verifies SyncWaitOn for task completes in time: returns.</summary>
        [Fact]
        public void SyncWaitOn_TaskCompletesInTime_Returns()
        {
            // Arrange

            // Act
            var delayedTask = Task.Delay(1000);
            Helper.Sync.WaitOn(delayedTask, TimeSpan.FromMinutes(1));

            // Assert
            Assert.True(delayedTask.IsCompleted);
        }

        /// <summary>Verifies SyncWaitOn for task exceeds max wait time: throws timeout exception.</summary>
        [Fact]
        public void SyncWaitOn_TaskExceedsMaxWaitTime_ThrowsTimeoutException()
        {
            // Arrange
            var taskSource = new TaskCompletionSource();
            Exception exception = null;

            // Act
            try
            {
                Helper.Sync.WaitOn(taskSource.Task, TimeSpan.FromMilliseconds(1));
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<TimeoutException>(exception);
        }

        /// <summary>Verifies SyncWaitOn for token cancelled: throws task canceled exception.</summary>
        [Fact]
        public void SyncWaitOn_TokenCancelled_ThrowsTaskCanceledException()
        {
            // Arrange
            var taskSource = new TaskCompletionSource();
            var cancellationTokenSource = new CancellationTokenSource();
            Exception exception = null;

            // Act
            try
            {
                cancellationTokenSource.CancelAfter(1000);
                Helper.Sync.WaitOn(taskSource.Task, TimeSpan.FromDays(1), cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<TaskCanceledException>(exception);
        }

        /// <summary>Verifies SyncWaitOn for generic task completes in time: returns.</summary>
        [Fact]
        public void SyncWaitOn_GenericTaskCompletesInTime_Returns()
        {
            // Arrange
            var taskSource = new TaskCompletionSource<object>();

            // Act
            _ = Task.Run(async () =>
            {
                await Helper.Async.Sleep(1000);
                taskSource.SetResult(new object());
            });
            Helper.Sync.WaitOn(taskSource.Task, TimeSpan.FromMinutes(1));

            // Assert
            Assert.True(taskSource.Task.IsCompleted);
        }

        /// <summary>Verifies SyncWaitOn for generic task exceeds max wait time: throws timeout exception.</summary>
        [Fact]
        public void SyncWaitOn_GenericTaskExceedsMaxWaitTime_ThrowsTimeoutException()
        {
            // Arrange
            var taskSource = new TaskCompletionSource<object>();
            Exception exception = null;

            // Act
            try
            {
                Helper.Sync.WaitOn(taskSource.Task, TimeSpan.FromMilliseconds(1));
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<TimeoutException>(exception);
        }

        /// <summary>Verifies SyncWaitOn for generic task token cancelled: throws task canceled exception.</summary>
        [Fact]
        public void SyncWaitOn_GenericTaskTokenCancelled_ThrowsTaskCanceledException()
        {
            // Arrange
            var taskSource = new TaskCompletionSource<object>();
            var cancellationTokenSource = new CancellationTokenSource();
            Exception exception = null;

            // Act
            try
            {
                cancellationTokenSource.CancelAfter(1000);
                Helper.Sync.WaitOn(taskSource.Task, TimeSpan.FromDays(1), cancellationTokenSource.Token);
            }
            catch (Exception ex)
            {
                exception = ex;
            }

            // Assert
            Assert.NotNull(exception);
            Assert.IsAssignableFrom<TaskCanceledException>(exception);
        }

        /// <summary>Verifies FormatAsLog for template with parameters: returns formatted string.</summary>
        [Theory]
        [InlineData("Hello from {Name}", new object[] { "Jens" }, "Hello from Jens")]
        [InlineData("{0}", new object[] { 56 }, "56")]
        [InlineData("{{{0},{1},{2}}}", new object[] { 1, 2, 3 }, "{1,2,3}")]
        [InlineData("{A}|{b}|{a}", new object[] { 4, 2 }, "4|2|4")]
        public void FormatAsLog_TemplateWithParameters_ReturnsFormattedString(string template, object[] parameters, string expected)
        {
            // Arrange

            // Act
            var actual = Helper.Strings.FormatAsLog(template, parameters);

            // Assert
            Assert.Equal(expected, actual);
        }

        private class ComparableModel
        {
            public int Value { get; set; }

            public string? Skipped { get; set; }

            public IDeduplicatable? Child { get; set; }
        }

        private sealed class DerivedComparableModel : ComparableModel
        {
        }

        private sealed class ComparableKey : IDeduplicatable
        {
            private readonly string _key;

            public ComparableKey(string key)
            {
                _key = key;
            }

            public bool IsEquivalentTo(IDeduplicatable other) => other is ComparableKey key && key._key == _key;

            public int GetBucket() => 1;
        }
    }
}