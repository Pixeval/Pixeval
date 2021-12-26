#region Copyright (c) Pixeval/Pixeval
// GPL v3 License
// 
// Pixeval/Pixeval
// Copyright (c) 2021 Pixeval/SplitView.cs
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.
#endregion

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Pixeval.Attributes;

namespace Pixeval.Controls.SlideView;

[DependencyProperty("AnimationDuration", typeof(TimeSpan),nameof(OnAnimationDurationChanged), DefaultValue = "TimeSpan.FromMilliseconds(500)")]
[DependencyProperty("ContentTemplate", typeof(DataTemplate))]
[DependencyProperty("Interval", typeof(TimeSpan), nameof(OnIntervalChanged),DefaultValue = "TimeSpan.FromSeconds(2)")]
[DependencyProperty("Slides", typeof(ICollection<object>),nameof(OnSlidesChanged))]
[TemplatePart(Name = PartAlternatorContainer, Type = typeof(Grid))]
public partial class SlideView : Control
{
    private const string PartAlternatorContainer = "AlternatorContainer";

    private Grid? _alternatorContainer;

    private SliderCircularLinkedList? _currentSlide;

    private readonly Vector3Transition _sliderTransition;

    private readonly DispatcherTimer _sliderTimer;

    private readonly TaskCompletionSource _currentSlideAwaiter;

    public Slide? Base => _alternatorContainer?.Children[0] as Slide;

    public Slide? Slider => _alternatorContainer?.Children[1] as Slide;

    public SlideView()
    {
        DefaultStyleKey = typeof(SplitView);

        _sliderTransition = new Vector3Transition();
        _currentSlideAwaiter = new TaskCompletionSource();
        _sliderTimer = new DispatcherTimer();

        Loaded += (_, _) => Initialize();
    }

    private async void Initialize()
    {
        await _currentSlideAwaiter.Task;
        _alternatorContainer?.Children.Add(InitializeNewSlider());
        _alternatorContainer?.Children.Add(InitializeNewSlider());
        MakeBase();
        _sliderTimer.Tick += SliderTimerOnTick;
        _sliderTimer.Interval = Interval;
        _sliderTimer.Start();
    }

    private async void SliderTimerOnTick(object? sender, object e)
    {
        if (_currentSlide is null)
        {
            _sliderTimer.Stop();
        }
        Slider!.Translation = new Vector3(0f, 0f, 0f);
        await Task.Delay(AnimationDuration.Milliseconds);
        AlterSlide();
    }

    private static void OnIntervalChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SlideView view && e.NewValue is TimeSpan span)
        {
            view._sliderTimer.Interval = span;
        }
    }

    private static void OnSlidesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SlideView view && e.NewValue is ICollection<object> { Count: > 0 } slides)
        {
            if (!view._currentSlideAwaiter.Task.IsCompleted)
            {
                view._currentSlideAwaiter.SetResult();
            }
            view._currentSlide = SliderCircularLinkedList.Create(slides);
            view._sliderTimer.Start();
        }
    }

    private static void OnAnimationDurationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is SlideView view && e.NewValue is TimeSpan span)
        {
            view._sliderTransition.Duration = span;
        }
    }

    private void MakeBase()
    {
        Base!.TranslationTransition = null;
        Base.Translation = new Vector3(0f, 0f, 0f);
    }

    private Slide InitializeNewSlider()
    {
        var current = _currentSlide!;
        _currentSlide = _currentSlide!.Next;
        return new Slide
        {
            Translation = new Vector3((float) ActualWidth, 0f, 0f),
            TranslationTransition = _sliderTransition,
            ContentTemplate = ContentTemplate,
            SlideContent = current
        };
    }

    private void AlterSlide()
    {
        _alternatorContainer?.Children.RemoveAt(0);
        _alternatorContainer?.Children.Add(InitializeNewSlider());
        MakeBase();
    }

    private void AlternatorContainerOnSizeChanged(object sender, SizeChangedEventArgs e)
    {
        // fixme: performance impact
        if (Slider is not null)
        {
            Slider.TranslationTransition = null;
            Slider.Translation = new Vector3(e.NewSize._width, 0f, 0f);
            Slider.TranslationTransition = _sliderTransition;
        }
    }

    protected override void OnApplyTemplate()
    {
        if (_alternatorContainer is not null)
        {
            _alternatorContainer.SizeChanged -= AlternatorContainerOnSizeChanged;
        }

        if ((_alternatorContainer = (GetTemplateChild(PartAlternatorContainer) as Grid)!) is not null)
        {
            _alternatorContainer.SizeChanged += AlternatorContainerOnSizeChanged;
        }

        base.OnApplyTemplate();
    }
}