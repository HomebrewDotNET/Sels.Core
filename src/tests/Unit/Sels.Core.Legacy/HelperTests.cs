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

        /// <summary>The generated delegate copies every readable and writable property from source to target.</summary>
        [Fact]
        public void GenerateTransferTo_ReadWriteProperties_CopiesValuesToTarget()
        {
            // Arrange
            var transfer = Helper.Expressions.Copying.GenerateTransferTo<TransferModel>();
            var source = new TransferModel { Number = 7, Text = "copied", Child = new TransferChild() };
            var target = new TransferModel();

            // Act
            transfer(source, target);

            // Assert
            Assert.Equal(7, target.Number);
            Assert.Equal("copied", target.Text);
            Assert.Same(source.Child, target.Child);
        }

        /// <summary>The generated delegate leaves the source unchanged and does not touch read-only properties.</summary>
        [Fact]
        public void GenerateTransferTo_ReadOnlyProperty_IsNotCopied()
        {
            // Arrange
            var transfer = Helper.Expressions.Copying.GenerateTransferTo<TransferModel>();
            var source = new TransferModel(1) { Number = 3 };
            var target = new TransferModel(2);

            // Act
            transfer(source, target);

            // Assert
            Assert.Equal(2, target.ReadOnly);
            Assert.Equal(3, source.Number);
        }

        /// <summary>Inherited properties are copied as well.</summary>
        [Fact]
        public void GenerateTransferTo_DerivedType_CopiesInheritedProperties()
        {
            // Arrange
            var transfer = Helper.Expressions.Copying.GenerateTransferTo<DerivedTransferModel>();
            var source = new DerivedTransferModel { Number = 4, Extra = "extra" };
            var target = new DerivedTransferModel();

            // Act
            transfer(source, target);

            // Assert
            Assert.Equal(4, target.Number);
            Assert.Equal("extra", target.Extra);
        }

        /// <summary>Null property values are copied over existing target values.</summary>
        [Fact]
        public void GenerateTransferTo_NullSourceValue_OverwritesTarget()
        {
            // Arrange
            var transfer = Helper.Expressions.Copying.GenerateTransferTo<TransferModel>();
            var source = new TransferModel { Text = null };
            var target = new TransferModel { Text = "existing" };

            // Act
            transfer(source, target);

            // Assert
            Assert.Null(target.Text);
        }

        /// <summary>A type without copyable properties produces a delegate that does nothing.</summary>
        [Fact]
        public void GenerateTransferTo_TypeWithoutProperties_ReturnsNoOpDelegate()
        {
            // Arrange
            var source = new EmptyTransferModel();
            var target = new EmptyTransferModel();

            // Act
            var transfer = Helper.Expressions.Copying.GenerateTransferTo<EmptyTransferModel>();
            transfer(source, target);

            // Assert
            Assert.NotNull(transfer);
        }

        /// <summary>Static properties are not instance values and are skipped when copying between instances.</summary>
        [Fact]
        public void GenerateTransferTo_TypeWithStaticProperty_CopiesInstanceProperties()
        {
            // Arrange
            var source = new StaticPropertyTransferModel { Number = 9 };
            var target = new StaticPropertyTransferModel();

            // Act
            var transfer = Helper.Expressions.Copying.GenerateTransferTo<StaticPropertyTransferModel>();
            transfer(source, target);

            // Assert
            Assert.Equal(9, target.Number);
        }

        private class TransferModel
        {
            public TransferModel()
            {
            }

            public TransferModel(int readOnly)
            {
                ReadOnly = readOnly;
            }

            public int Number { get; set; }

            public string? Text { get; set; }

            public TransferChild? Child { get; set; }

            public int ReadOnly { get; }

            public int this[int index]
            {
                get => index;
                set { }
            }
        }

        private sealed class DerivedTransferModel : TransferModel
        {
            public string? Extra { get; set; }
        }

        private sealed class TransferChild
        {
        }

        private sealed class EmptyTransferModel
        {
        }

        private sealed class StaticPropertyTransferModel
        {
            public static int Shared { get; set; }

            public int Number { get; set; }
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