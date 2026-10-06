using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AnimatedControls.Avalonia;
using Avalonia;
using Avalonia.Headless;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Pixeval.Tests;

[TestClass]
[DoNotParallelize]
public sealed class UpdatableAnimatedBitmapTest
{
    [TestMethod]
    public async Task LatePreviewsCannotOverwriteAnotherUpdateOrACompletedSource()
    {
        await using var session = HeadlessUnitTestSession.StartNew(typeof(ImageViewerRenderingTest.ViewerTestApplication));
        await session.Dispatch(async () =>
        {
            using var source = new UpdatableAnimatedBitmap();
            var firstResult = new TaskCompletionSource<IAnimatedBitmap?>(TaskCreationOptions.RunContinuationsAsynchronously);
            var secondResult = new TaskCompletionSource<IAnimatedBitmap?>(TaskCreationOptions.RunContinuationsAsynchronously);
            CancellationToken firstToken = default;
            var secondStarted = new TaskCompletionSource<CancellationToken>(TaskCreationOptions.RunContinuationsAsynchronously);
            var first = source.UpdateAsync(token =>
            {
                firstToken = token;
                return firstResult.Task;
            });
            var second = source.UpdateAsync(token =>
            {
                secondStarted.SetResult(token);
                return secondResult.Task;
            });
            var secondToken = await secondStarted.Task.WaitAsync(TimeSpan.FromSeconds(10));
            var preview = new WriteableBitmap(new(20, 10), new(96, 96));
            source.UpdatePreview(preview, secondToken);
            source.UpdatePreview(new WriteableBitmap(new(10, 5), new(96, 96)), firstToken);
            Assert.AreSame(preview, source.Frames[0]);

            secondResult.SetResult(new TestBitmap(new(200, 100)));
            await second;
            var changes = 0;
            source.Changed += (_, _) => changes++;
            source.UpdatePreview(new WriteableBitmap(new(20, 10), new(96, 96)), secondToken);
            Assert.AreEqual(0, changes);
            Assert.AreEqual(new Size(200, 100), source.Size);
            firstResult.SetResult(null);
            await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () => await first);
        }, CancellationToken.None);
    }

    [TestMethod]
    public async Task ReplacementStaysHiddenUntilInitialized()
    {
        using var source = new UpdatableAnimatedBitmap();
        var fallback = new TestBitmap(new(100, 100));
        source.SetFallback(fallback);
        var started = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var replacement = new TestBitmap(new(1000, 1000), () =>
        {
            started.SetResult();
            release.Task.GetAwaiter().GetResult();
        });
        var update = source.UpdateAsync(_ => Task.FromResult<IAnimatedBitmap?>(replacement));
        await started.Task.WaitAsync(TimeSpan.FromSeconds(10));
        Assert.AreEqual(fallback.Size, source.Size);
        release.SetResult();
        Assert.AreSame(replacement, await update);
        Assert.AreEqual(replacement.Size, source.Size);
    }

    [TestMethod]
    public async Task LatestUpdateWinsAndDisposesAbandonedCandidate()
    {
        using var source = new UpdatableAnimatedBitmap();
        var pending = new TaskCompletionSource<IAnimatedBitmap?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var old = new TestBitmap(new(100, 100));
        CancellationToken oldToken = default;
        var first = source.UpdateAsync(token =>
        {
            oldToken = token;
            return pending.Task;
        });
        var latest = new TestBitmap(new(200, 200));
        Assert.AreSame(latest, await source.UpdateAsync(_ => Task.FromResult<IAnimatedBitmap?>(latest)));
        Assert.IsTrue(oldToken.IsCancellationRequested);
        pending.SetResult(old);
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () => await first);
        Assert.AreEqual(1, old.DisposeCount);
        Assert.AreEqual(latest.Size, source.Size);
    }

    [TestMethod]
    public async Task FailedInitializationPreservesCompletedImage()
    {
        using var source = new UpdatableAnimatedBitmap();
        var current = new TestBitmap(new(100, 100));
        await source.UpdateAsync(_ => Task.FromResult<IAnimatedBitmap?>(current));
        var failed = new TestBitmap(new(200, 200), () => { }, fail: true);
        var failures = 0;
        source.Failed += (_, _) => failures++;
        Assert.IsNull(await source.UpdateAsync(_ => Task.FromResult<IAnimatedBitmap?>(failed)));
        Assert.AreEqual(current.Size, source.Size);
        Assert.IsFalse(source.IsFailed);
        Assert.AreEqual(1, failures);
        Assert.AreEqual(1, failed.DisposeCount);
    }

    [TestMethod]
    public async Task DisposalCancelsPendingUpdateAndDisposesLateCandidate()
    {
        var source = new UpdatableAnimatedBitmap();
        var pending = new TaskCompletionSource<IAnimatedBitmap?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var update = source.UpdateAsync(_ => pending.Task);
        source.Dispose();
        var candidate = new TestBitmap(new(100, 100));
        pending.SetResult(candidate);
        await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () => await update);
        Assert.AreEqual(1, candidate.DisposeCount);
        Assert.IsFalse(source.IsInitialized);
    }

    [TestMethod]
    public async Task BorrowedSourcesSurviveReplacementAndDisposal()
    {
        var source = new UpdatableAnimatedBitmap(disposeSources: false);
        using var fallback = new TestBitmap(new(10, 10));
        using var original = new TestBitmap(new(100, 100));
        using var replacement = new TestBitmap(new(200, 200));
        source.SetFallback(fallback);
        await source.UpdateAsync(_ => Task.FromResult<IAnimatedBitmap?>(original));
        await source.UpdateAsync(_ => Task.FromResult<IAnimatedBitmap?>(replacement));
        source.SetFallback(null);
        source.Dispose();
        Assert.AreEqual(0, fallback.DisposeCount);
        Assert.AreEqual(0, original.DisposeCount);
        Assert.AreEqual(0, replacement.DisposeCount);
    }

    [TestMethod]
    public async Task SharedOwnedFallbackIsDisposedOnlyOnce()
    {
        var source = new UpdatableAnimatedBitmap();
        var image = new TestBitmap(new(100, 100));
        source.SetFallback(image);
        await source.UpdateAsync(_ => Task.FromResult<IAnimatedBitmap?>(image));
        source.SetFallback(null);
        Assert.AreEqual(0, image.DisposeCount);
        source.Dispose();
        Assert.AreEqual(1, image.DisposeCount);
    }

    [TestMethod]
    public async Task LoaderExceptionKeepsExistingSource()
    {
        using var source = new UpdatableAnimatedBitmap();
        var image = new TestBitmap(new(100, 100));
        source.SetFallback(image);
        await Assert.ThrowsExactlyAsync<InvalidOperationException>(() => source.UpdateAsync(
            _ => Task.FromException<IAnimatedBitmap?>(new InvalidOperationException())));
        Assert.IsTrue(source.IsInitialized);
        Assert.AreEqual(image.Size, source.Size);
        Assert.AreEqual(0, image.DisposeCount);
    }

    [TestMethod]
    public void UnsubscribedImageUsesNormalSourceLayout()
    {
        using var source = new UpdatableAnimatedBitmap();
        source.SetFallback(new TestBitmap(new(100, 50)));
        var control = new AnimatedImage { Source = source, Stretch = Stretch.Uniform };
        control.Measure(new(100, 100));
        control.Arrange(new(0, 0, 100, 100));
        Assert.AreEqual(new Size(100, 50), control.Bounds.Size);
    }

    [TestMethod]
    public void SourceSizeEventRunsBeforeLayoutInvalidation()
    {
        using var source = new TestBitmap(new(100, 50));
        using var replacement = new TestBitmap(new(2000, 1500));
        var control = new AnimatedImage { Source = source, Stretch = Stretch.Uniform };
        var available = new Size(double.PositiveInfinity, double.PositiveInfinity);
        control.Measure(available);
        control.Arrange(new(control.DesiredSize));
        var bounds = control.Bounds;
        var events = 0;
        control.SourceSizeChanged += (_, e) =>
        {
            events++;
            Assert.AreEqual(source.Size, e.OldSize);
            Assert.AreEqual(replacement.Size, e.NewSize);
            Assert.AreEqual(bounds, control.Bounds);
            Assert.IsTrue(control.IsMeasureValid);
            Assert.IsTrue(control.IsArrangeValid);
        };
        control.Source = replacement;
        Assert.AreEqual(1, events);
        Assert.IsFalse(control.IsMeasureValid);
        control.Measure(available);
        control.Arrange(new(control.DesiredSize));
        Assert.AreEqual(replacement.Size, control.Bounds.Size);
    }

    [TestMethod]
    public void EqualSizedReplacementDoesNotRequestViewportAdjustment()
    {
        using var source = new TestBitmap(new(100, 50));
        using var replacement = new TestBitmap(new(100, 50));
        var control = new AnimatedImage { Source = source, Stretch = Stretch.Uniform };
        var events = 0;
        control.SourceSizeChanged += (_, _) => events++;
        control.Source = replacement;
        Assert.AreEqual(0, events);
    }

    [TestMethod]
    [DataRow(500, 500, 500, 250)]
    [DataRow(2000, 2000, 1000, 500)]
    public void DownOnlyUsesSourceSize(int width, int height, int expectedWidth, int expectedHeight)
    {
        using var source = new UpdatableAnimatedBitmap();
        source.SetFallback(new TestBitmap(new(1000, 500)));
        var control = new AnimatedImage
        {
            Source = source,
            Stretch = Stretch.Uniform,
            StretchDirection = StretchDirection.DownOnly,
            HorizontalAlignment = global::Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = global::Avalonia.Layout.VerticalAlignment.Center
        };
        control.Measure(new(width, height));
        control.Arrange(new(0, 0, width, height));
        Assert.AreEqual(new Size(expectedWidth, expectedHeight), control.Bounds.Size);
    }

    private sealed class TestBitmap(Size size, Action? initialize = null, bool fail = false) : IAnimatedBitmap
    {
        public bool IsInitialized { get; set; } = initialize is null;
        public bool IsFailed => fail;
        public bool IsCancellable { get; set; }
        public Size Size => size;
        public int FrameCount => 1;
        public IReadOnlyList<Bitmap> Frames => [];
        public IReadOnlyList<int> Delays => [0];
        public int DisposeCount { get; private set; }
        public event EventHandler? Initialized;
        public event EventHandler<AnimatedBitmapFailedEventArgs>? Failed;

        public void Init()
        {
            initialize?.Invoke();
            if (fail)
                Failed?.Invoke(this, new(new InvalidOperationException()));
            else
            {
                IsInitialized = true;
                Initialized?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose() => DisposeCount++;
    }
}
