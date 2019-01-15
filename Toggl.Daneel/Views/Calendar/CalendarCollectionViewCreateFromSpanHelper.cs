﻿using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using CoreGraphics;
using Foundation;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation.Helper;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;
using UIKit;
using Math = System.Math;

namespace Toggl.Daneel.Views.Calendar
{
    public sealed class CalendarCollectionViewCreateFromSpanHelper : CalendarCollectionViewAutoScrollHelper, IUIGestureRecognizerDelegate
    {
        private static readonly TimeSpan defaultDuration = Constants.CalendarItemViewDefaultDuration;

        private readonly CalendarCollectionViewSource dataSource;

        private UILongPressGestureRecognizer longPressGestureRecognizer;

        private NSIndexPath itemIndexPath;

        private CGPoint firstPoint;

        private readonly ISubject<(DateTimeOffset, TimeSpan)> createFromSpanSuject = new Subject<(DateTimeOffset, TimeSpan)>();

        public IObservable<(DateTimeOffset, TimeSpan)> CreateFromSpan => createFromSpanSuject.AsObservable();

        private List<DateTimeOffset> allItemsStartAndEndTime;

        public CalendarCollectionViewCreateFromSpanHelper(
            UICollectionView collectionView,
            CalendarCollectionViewSource dataSource,
            CalendarCollectionViewLayout Layout) : base(collectionView, Layout)
        {
            Ensure.Argument.IsNotNull(dataSource, nameof(dataSource));

            this.dataSource = dataSource;

            longPressGestureRecognizer = new UILongPressGestureRecognizer(onLongPress);
            longPressGestureRecognizer.Delegate = this;
            collectionView.AddGestureRecognizer(longPressGestureRecognizer);
        }

        public CalendarCollectionViewCreateFromSpanHelper(IntPtr handle) : base(handle)
        {
        }

        [Export("gestureRecognizer:shouldRecognizeSimultaneouslyWithGestureRecognizer:")]
        public bool ShouldRecognizeSimultaneously(UIGestureRecognizer gestureRecognizer, UIGestureRecognizer otherGestureRecognizer)
            => otherGestureRecognizer is UILongPressGestureRecognizer;

        private void onLongPress(UILongPressGestureRecognizer gesture)
        {
            var point = gesture.LocationInView(CollectionView);

            switch (gesture.State)
            {
                case UIGestureRecognizerState.Began:
                    longPressBegan(point);
                    break;

                case UIGestureRecognizerState.Changed:
                    longPressChanged(point);
                    break;

                case UIGestureRecognizerState.Ended:
                    longPressEnded(point);
                    break;

                case UIGestureRecognizerState.Cancelled:
                case UIGestureRecognizerState.Failed:
                    dataSource.RemoveItemView(itemIndexPath);
                    dataSource.StopEditing();
                    itemIndexPath = null;
                    break;
            }
        }

        private void longPressBegan(CGPoint point)
        {
            if (dataSource.IsEditing || dataSource.CalendarItemAtPoint(point) != null)
                return;

            allItemsStartAndEndTime = dataSource.AllItemsStartAndEndTime();

            dataSource.StartEditing();
            firstPoint = point;
            LastPoint = point;
            var startTime = Layout.DateAtPoint(firstPoint).RoundDownToClosestQuarter();
            itemIndexPath = dataSource.InsertItemView(startTime, defaultDuration);
            impactFeedback.ImpactOccurred();
            selectionFeedback.Prepare();
        }

        private bool isDraggingDown(CGPoint point) => firstPoint.Y < point.Y;

        private void longPressChanged(CGPoint point)
        {
            if (itemIndexPath == null)
                return;

            LastPoint = point;

            DateTimeOffset startTime;
            DateTimeOffset endTime;

            if (isDraggingDown(point))
            {
                startTime = Layout.DateAtPoint(firstPoint).RoundDownToClosestQuarter();
                endTime = NewEndTimeWithDynamicDuration(point, allItemsStartAndEndTime);

                if (point.Y > BottomAutoScrollLine)
                    StartAutoScrolDown(longPressChanged);
                else
                    StopAutoScroll();
            }
            else
            {
                startTime = NewStartTimeWithDynamicDuration(point, allItemsStartAndEndTime);
                endTime = Layout.DateAtPoint(firstPoint).RoundDownToClosestQuarter();

                if (point.Y < TopAutoScrollLine)
                    StartAutoScrollUp(longPressChanged);
                else
                    StopAutoScroll();
            }

            var duration = endTime - startTime;

            dataSource.UpdateItemView(itemIndexPath, startTime, duration);
            selectionFeedback.SelectionChanged();
        }

        private void longPressEnded(CGPoint point)
        {
            if (itemIndexPath == null)
                return;

            LastPoint = point;

            DateTimeOffset startTime;
            DateTimeOffset endTime;

            if (firstPoint.Y < point.Y)
            {
                startTime = Layout.DateAtPoint(firstPoint).RoundDownToClosestQuarter();
                endTime = Layout.DateAtPoint(LastPoint).RoundUpToClosestQuarter();
            }
            else
            {
                startTime = Layout.DateAtPoint(LastPoint).RoundDownToClosestQuarter();
                endTime = Layout.DateAtPoint(firstPoint).RoundDownToClosestQuarter();
            }

            var duration = endTime - startTime;
            createFromSpanSuject.OnNext((startTime, duration));

            dataSource.StopEditing();
            itemIndexPath = null;
            StopAutoScroll();

            impactFeedback.ImpactOccurred();
        }
    }
}
