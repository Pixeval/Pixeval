// Copyright (c) Pixeval.
// Licensed under the GPL-3.0 License.

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pixeval.Collections;

namespace Pixeval.Tests;

[TestClass]
public sealed class CompositeObservableCollectionTest
{
    [TestMethod]
    public void Test_CompositeObservableCollection_FlattensSourcesAndTracksChanges()
    {
        var pinned = new ObservableCollection<string> { "pinned-1", "pinned-2" };
        var history = new ObservableCollection<string> { "history-1", "history-2" };
        using var composite = new CompositeObservableCollection<object>(pinned, history);

        Assert.AreSequenceEqual((object[]) ["pinned-1", "pinned-2", "history-1", "history-2"], composite.ToArray());

        NotifyCollectionChangedEventArgs? change = null;
        composite.CollectionChanged += (_, args) => change = args;

        pinned.Insert(1, "pinned-inserted");
        Assert.AreSequenceEqual((object[]) ["pinned-1", "pinned-inserted", "pinned-2", "history-1", "history-2"], composite.ToArray());
        Assert.AreEqual(NotifyCollectionChangedAction.Add, change!.Action);
        Assert.AreEqual(1, change.NewStartingIndex);

        history.RemoveAt(0);
        Assert.AreSequenceEqual((object[]) ["pinned-1", "pinned-inserted", "pinned-2", "history-2"], composite.ToArray());
        Assert.AreEqual(3, change!.OldStartingIndex);

        pinned[0] = "pinned-replaced";
        Assert.AreSequenceEqual((object[]) ["pinned-replaced", "pinned-inserted", "pinned-2", "history-2"], composite.ToArray());
        Assert.AreEqual(NotifyCollectionChangedAction.Replace, change!.Action);
        Assert.AreEqual(0, change.NewStartingIndex);

        pinned.Move(2, 0);
        Assert.AreSequenceEqual((object[]) ["pinned-2", "pinned-replaced", "pinned-inserted", "history-2"], composite.ToArray());
        Assert.AreEqual(NotifyCollectionChangedAction.Move, change!.Action);
        Assert.AreEqual(0, change.NewStartingIndex);
        Assert.AreEqual(2, change.OldStartingIndex);
    }

    [TestMethod]
    public void Test_CompositeObservableCollection_ResetRebuildsOnlyTheChangedSource()
    {
        var pinned = new ObservableCollection<string> { "pinned" };
        var history = new ObservableCollection<string> { "history-1", "history-2" };
        using var composite = new CompositeObservableCollection<object>(pinned, history);
        var changes = new List<NotifyCollectionChangedEventArgs>();
        composite.CollectionChanged += (_, args) => changes.Add(args);

        history.Clear();

        Assert.AreSequenceEqual((object[]) ["pinned"], composite.ToArray());
        Assert.AreEqual(NotifyCollectionChangedAction.Reset, changes.Single().Action);
    }

    [TestMethod]
    public void Test_CompositeObservableCollection_CanAddAndRemoveSources()
    {
        var first = new ObservableCollection<string> { "first" };
        var second = new ObservableCollection<string> { "second" };
        var third = new ObservableCollection<string> { "third" };
        using var composite = new CompositeObservableCollection<object>(first, second);

        composite.AddSource(third);
        Assert.AreSequenceEqual((object[]) ["first", "second", "third"], composite.ToArray());

        Assert.IsTrue(composite.RemoveSource(second));
        Assert.AreSequenceEqual((object[]) ["first", "third"], composite.ToArray());
        Assert.IsFalse(composite.RemoveSource(second));

        second.Add("detached");
        Assert.AreSequenceEqual((object[]) ["first", "third"], composite.ToArray());
    }

    [TestMethod]
    public void Test_CompositeObservableCollection_AcceptsNonObservableSources()
    {
        var first = new[] { "first" };
        var second = new List<string> { "second" };
        using var composite = new CompositeObservableCollection<string>(first);

        composite.AddSource(second);

        Assert.AreSequenceEqual(["first", "second"], composite.ToArray());
        Assert.IsTrue(composite.RemoveSource(second));
        Assert.AreSequenceEqual(["first"], composite.ToArray());
    }

    [TestMethod]
    public void Test_CompositeObservableCollection_DisposeDetachesSources()
    {
        var source = new ObservableCollection<string> { "before" };
        var composite = new CompositeObservableCollection<object>(source);

        composite.Dispose();
        source.Add("after");

        Assert.AreSequenceEqual((object[]) ["before"], composite.ToArray());
    }
}
