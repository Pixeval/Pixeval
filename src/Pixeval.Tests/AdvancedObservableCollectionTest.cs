using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Pixeval.Collections;

namespace Pixeval.Tests;

[TestClass]
public sealed class AdvancedObservableCollectionTest
{
    private class SampleClass : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public virtual int Val
        {
            get;
            set
            {
                field = value;
                OnPropertyChanged();
            }
        }

        public int GetPropertyChangedEventHandlerSubscriberLength()
        {
            return PropertyChanged is null ? 0 : PropertyChanged.GetInvocationList().Length;
        }

        public SampleClass(int val) => Val = val;

        private void OnPropertyChanged([CallerMemberName] string name = "") => PropertyChanged?.Invoke(this, new(name));
    }

    private class DerivedClass(int val) : SampleClass(val)
    {
        public override int Val { get => 101; set { } }
    }

    [TestMethod]
    public void Test_SourceNcc_CollectionChanged_Add()
    {
        // Create ref list with all test items:
        var refList = new List<SampleClass>();
        for (var e = 0; e < 100; e++)
        {
            refList.Add(new SampleClass(e));
        }

        var col = new ObservableCollection<SampleClass>();
        var aoc = new AdvancedObservableCollection<SampleClass>(col, true);

        Assert.HasCount(0, col);
        Assert.HasCount(0, aoc);

        foreach (var item in refList)
        {
            col.Add(item);
        }

        Assert.AreNotEqual(0, col.Count);
        Assert.AreNotEqual(0, aoc.Count);

        Assert.HasCount(refList.Count, col);
        Assert.HasCount(refList.Count, aoc);

        // Make sure each item added to source is in the expected place in the view:
        for (var i = 0; i < refList.Count; i++)
        {
            var sourceItem = col[i];
            var collectionViewItem = aoc[i];

            Assert.AreEqual(sourceItem.Val, collectionViewItem.Val);
        }

        // Check if subscribed to all items:
        foreach (var item in refList)
        {
            Assert.AreEqual(1, item.GetPropertyChangedEventHandlerSubscriberLength());
        }

        // Add a filter and item to filter
        var itemToFilter = new SampleClass(1000);
        aoc.Filter = x => !Filter(x);
        col.Add(itemToFilter);

        var filteredItemInAoc = aoc.FirstOrDefault(Filter);
        Assert.IsNull(filteredItemInAoc, $"Filtered item unexpectedly included in collection view: {filteredItemInAoc?.Val}.");

        col.Remove(itemToFilter);

        // With filter set, create and add an item that isn't filtered
        var itemToNotFilter = new SampleClass(itemToFilter.Val + 1);
        col.Add(itemToNotFilter);

        var addedItemInAoc = aoc.FirstOrDefault(x => x.Val == itemToNotFilter.Val);

        Assert.IsNotNull(addedItemInAoc, $"Unfiltered item unexpectedly filtered out of collection view: {addedItemInAoc?.Val}.");

        // Ensure added (not filtered) item is added to the last position in view.
        var indexOfNotFilteredItemInAoc = aoc.IndexOf(addedItemInAoc);
        Assert.AreEqual(aoc.Count - 1, indexOfNotFilteredItemInAoc, "Unfiltered item added to source collection not last in view.");
        return;
        bool Filter(SampleClass x) => x.Val == itemToFilter.Val;
    }

    [TestMethod]
    public void Test_SourceNcc_CollectionChanged_Remove()
    {
        // Create ref list with all test items:
        var refList = new List<SampleClass>();
        for (var e = 0; e < 100; e++)
        {
            refList.Add(new(e));
        }

        var col = new ObservableCollection<SampleClass>();
        var aoc = new AdvancedObservableCollection<SampleClass>(col, true);

        // Add all items to collection:
        foreach (var item in refList)
        {
            col.Add(item);
        }

        while (col.Count > 0)
        {
            col.RemoveAt(0);
        }

        // Check if unsubscribed from all items:
        foreach (var item in refList)
        {
            Assert.AreEqual(0, item.GetPropertyChangedEventHandlerSubscriberLength());
        }
    }

    [TestMethod]
    public void Test_DerivedTypesInList()
    {
        // Create ref list with elements of different types:
        var refList = new List<SampleClass>();
        for (var e = 0; e < 100; e++)
        {
            if (e % 2 == 1)
            {
                refList.Add(new SampleClass(e));
            }
            else
            {
                refList.Add(new DerivedClass(e));
            }
        }
        var col = new ObservableCollection<SampleClass>();

        // Add all items to collection:
        foreach (var item in refList)
        {
            col.Add(item);
        }

        // Sort elements using a property that is overriden in the derived class
        var aoc = new AdvancedObservableCollection<SampleClass>(col, true);
        aoc.SortDescriptions.Add(ISortDescription<SampleClass>.Create(t => t.Val, true));
    }
}
