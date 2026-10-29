using System.ComponentModel;
using Sels.Core.Entity.Templates;

namespace Sels.Core.Legacy.Tests.Entity.Templates
{
    /// <summary>Contract tests for metadata-backed observable properties.</summary>
    [Trait("Category", "Unit")]
    public class BaseObservableTests
    {
        /// <summary>Unset properties return default values and factories are retained after first access.</summary>
        [Fact]
        public void GetAndGetOrSet_UnsetAndCachedValues_ReturnExpectedValues()
        {
            var sut = new ObservableProbe();
            var factoryCalls = 0;

            var unset = sut.Name;
            var first = sut.GetOrSetValue(() => ++factoryCalls);
            var second = sut.GetOrSetValue(() => ++factoryCalls);

            Assert.Null(unset);
            Assert.Equal(1, first);
            Assert.Equal(first, second);
            Assert.Equal(1, factoryCalls);
        }

        /// <summary>Set raises only for changed values and reports the property name.</summary>
        [Fact]
        public void Set_EqualThenChangedValue_OnlyRaisesForChange()
        {
            var sut = new ObservableProbe();
            var names = new List<string?>();
            sut.PropertyChanged += (_, args) => names.Add(args.PropertyName);

            sut.Name = "same";
            sut.Name = "same";
            sut.Name = "changed";

            Assert.Equal(new[] { nameof(ObservableProbe.Name), nameof(ObservableProbe.Name) }, names);
            Assert.Equal("changed", sut.Name);
        }

        /// <summary>Replacing an observable child detaches the old child and forwards new events.</summary>
        [Fact]
        public void Set_ObservableChildReplaced_TracksOnlyCurrentChild()
        {
            var sut = new ObservableProbe();
            var child = new ObservableChild();
            var nextChild = new ObservableChild();
            var names = new List<string?>();
            sut.PropertyChanged += (_, args) => names.Add(args.PropertyName);
            sut.Child = child;
            names.Clear();

            sut.Child = nextChild;
            names.Clear();
            child.Raise();
            nextChild.Raise();

            Assert.Equal(new[] { nameof(ObservableChild.Value) }, names);
        }

        /// <summary>Assigning null unsets a previously stored property.</summary>
        [Fact]
        public void Set_NullValue_UnsetsProperty()
        {
            var sut = new ObservableProbe { Name = "value" };

            sut.Name = null;

            Assert.Null(sut.Name);
            Assert.False(sut.IsNameSet);
        }

        /// <summary>Reset removes all stored property values.</summary>
        [Fact]
        public void Reset_WithStoredValues_UnsetsProperties()
        {
            // Arrange
            var sut = new ObservableProbe { Name = "value", Child = new ObservableChild() };

            // Act
            sut.ResetState();

            // Assert
            Assert.Null(sut.Name);
            Assert.False(sut.IsNameSet);
            Assert.Null(sut.Child);
        }

        /// <summary>Reset removes the handlers that were subscribed before the reset.</summary>
        [Fact]
        public void Reset_WithSubscribedHandlers_RemovesHandlers()
        {
            // Arrange
            var sut = new ObservableProbe();
            var names = new List<string?>();
            sut.PropertyChanged += (_, args) => names.Add(args.PropertyName);

            // Act
            sut.ResetState();
            sut.Name = "value";

            // Assert
            Assert.Empty(names);
        }

        /// <summary>Reset detaches from observable children so their events are no longer forwarded.</summary>
        [Fact]
        public void Reset_WithObservableChild_StopsForwardingChildEvents()
        {
            // Arrange
            var sut = new ObservableProbe();
            var child = new ObservableChild();
            sut.Child = child;
            sut.ResetState();
            var names = new List<string?>();
            sut.PropertyChanged += (_, args) => names.Add(args.PropertyName);

            // Act
            child.Raise();

            // Assert
            Assert.Empty(names);
        }

        /// <summary>Reset on a fresh instance leaves it usable.</summary>
        [Fact]
        public void Reset_FreshInstance_RemainsUsable()
        {
            // Arrange
            var sut = new ObservableProbe();

            // Act
            sut.ResetState();
            sut.Name = "value";

            // Assert
            Assert.Equal("value", sut.Name);
        }

        private sealed class ObservableProbe : BaseObservable
        {
            public void ResetState() => Reset();

            public string? Name
            {
                get => Get<string?>(nameof(Name));
                set => Set(nameof(Name), value);
            }

            public ObservableChild? Child
            {
                get => Get<ObservableChild?>(nameof(Child));
                set => Set<ObservableChild?, ObservableChild?>(nameof(Child), value, value);
            }

            public bool IsNameSet => IsSet<string?>(nameof(Name));

            public int GetOrSetValue(Func<int> factory) => GetOrSet(nameof(GetOrSetValue), factory);
        }

        private sealed class ObservableChild : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            public int Value { get; set; }

            public void Raise() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Value)));
        }
    }
}