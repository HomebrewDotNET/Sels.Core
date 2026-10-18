using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Sels.Core.Entity.Components;

namespace Sels.Core.Legacy.Tests.Entity.Components
{
    /// <summary>Contract tests for observable deduplication.</summary>
    [Trait("Category", "Unit")]
    public class ObservableDeduplicatorTests
    {
        /// <summary>Add emits an Add event containing the inserted item.</summary>
        [Fact]
        public void Add_ItemAdded_EmitsAddEventWithItem()
        {
            var sut = new ObservableDeduplicator<ObservableItem>();
            NotifyCollectionChangedEventArgs? observed = null;
            sut.CollectionChanged += (_, args) => observed = args;
            var item = new ObservableItem(1);

            sut.Add(item);

            Assert.NotNull(observed);
            Assert.Equal(NotifyCollectionChangedAction.Add, observed!.Action);
            Assert.Same(item, observed.NewItems![0]);
        }

        /// <summary>Set updates an existing item after its selected key changes.</summary>
        [Fact]
        public void Set_ExistingItemChangesKey_UpdatesBucket()
        {
            var sut = new ObservableDeduplicator<ObservableItem>(by: item => item.Key);
            var item = new ObservableItem(1);
            sut.Add(item);
            item.Key = 2;

            var bucket = sut.Set(item);

            Assert.Equal(2, bucket);
            Assert.True(sut.HasDuplicate(new ObservableItem(2)));
        }

        /// <summary>Removing an item detaches its property-change subscription.</summary>
        [Fact]
        public void Remove_ItemThenPropertyChanges_DoesNotForwardNotification()
        {
            var sut = new ObservableDeduplicator<ObservableItem>();
            var notifications = 0;
            var item = new ObservableItem(1);
            sut.Add(item);
            sut.PropertyChanged += (_, _) => notifications++;

            sut.Remove(item);
            item.RaisePropertyChanged();

            Assert.Equal(0, notifications);
        }

        /// <summary>Unsubscribe stops observing subsequent source collection changes.</summary>
        [Fact]
        public void Unsubscribe_SourceChanges_DoesNotTrackNewItems()
        {
            var source = new ObservableCollection<ObservableItem>();
            var sut = new ObservableDeduplicator<ObservableItem>();
            var item = new ObservableItem(1);
            sut.Subscribe(source);

            sut.Unsubscribe(source);
            source.Add(item);

            Assert.False(sut.HasDuplicate(item));
        }

        private sealed class ObservableItem : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler? PropertyChanged;

            public int Key { get; set; }

            public ObservableItem(int key)
            {
                Key = key;
            }

            public void RaisePropertyChanged() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Key)));
        }
    }
}