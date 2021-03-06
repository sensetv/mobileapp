﻿using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using FluentAssertions;
using FsCheck.Xunit;
using NSubstitute;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Foundation.Tests.Extensions;
using Toggl.Foundation.Tests.Generators;
using Xunit;
using Task = System.Threading.Tasks.Task;

namespace Toggl.Foundation.Tests.MvvmCross.ViewModels
{
    public sealed class EditDurationViewModelTests
    {
        public abstract class EditDurationViewModelTest : BaseViewModelTests<EditDurationViewModel>
        {
            protected override EditDurationViewModel CreateViewModel()
                => new EditDurationViewModel(NavigationService, TimeService, DataSource, AnalyticsService, RxActionFactory, SchedulerProvider);
        }

        public sealed class TheConstructor : EditDurationViewModelTest
        {
            [Theory, LogIfTooSlow]
            [ConstructorData]
            public void ThrowsIfAnyOfTheArgumentsIsNull(bool useNavigationService, bool useTimeService, bool useDataSource, bool useAnalyticsService, bool useRxActionFactory, bool useSchedulerProvider)
            {
                var navigationService = useNavigationService ? NavigationService : null;
                var timeService = useTimeService ? TimeService : null;
                var dataSource = useDataSource ? DataSource : null;
                var analyticsService = useAnalyticsService ? AnalyticsService : null;
                var rxActionFactory = useRxActionFactory ? RxActionFactory : null;
                var schedulerProvider = useSchedulerProvider ? SchedulerProvider : null;

                Action tryingToConstructWithEmptyParameters =
                    () => new EditDurationViewModel(navigationService, timeService, dataSource, analyticsService, rxActionFactory, schedulerProvider);

                tryingToConstructWithEmptyParameters.Should().Throw<ArgumentNullException>();
            }

        }

        public sealed class TheDurationProperty : EditDurationViewModelTest
        {
            [Property]
            public void WhenChangedWhileUpdatingTheRunningTimeEntryTriggersTheUpdateOfTheStartTime(DateTimeOffset now)
            {
                var start = now.AddHours(-2);
                var parameter = DurationParameter.WithStartAndDuration(start, null);
                TimeService.CurrentDateTime.Returns(now);
                var observer = TestScheduler.CreateObserver<DateTimeOffset>();
                ViewModel.StartTime.Subscribe(observer);

                ViewModel.Prepare(new EditDurationParameters(parameter));

                ViewModel.ChangeDuration.Execute(TimeSpan.FromHours(4));

                TestScheduler.Start();
                var expectedStart = start.AddHours(-2);
                observer.LastValue().Should().BeSameDateAs(expectedStart);
            }

            [Property]
            public void WhenChangedWhileUpdatingFinishedTimeEntryTriggersTheUpdateOfTheStopTime(DateTimeOffset now)
            {
                var start = now.AddHours(-2);
                var parameter = DurationParameter.WithStartAndDuration(start, now - start);
                TimeService.CurrentDateTime.Returns(now);
                ViewModel.Prepare(new EditDurationParameters(parameter));
                var observer = TestScheduler.CreateObserver<DateTimeOffset>();
                ViewModel.StopTime.Subscribe(observer);

                ViewModel.ChangeDuration.Execute(TimeSpan.FromHours(4));

                TestScheduler.Start();
                var expectedStop = now.AddHours(2);
                observer.LastValue().Should().BeSameDateAs(expectedStop);
            }

            [Property]
            public void IsUpdatedAccordingToTimeServiceForRunningTimeEntries(DateTimeOffset now)
            {
                var start = now.AddHours(-2);
                var parameter = DurationParameter.WithStartAndDuration(start, null);
                var tickSubject = new Subject<DateTimeOffset>();
                var tickObservable = tickSubject.AsObservable().Publish();
                var observer = TestScheduler.CreateObserver<TimeSpan>();
                ViewModel.Duration.Subscribe(observer);
                tickObservable.Connect();
                TimeService.CurrentDateTimeObservable.Returns(tickObservable);
                TimeService.CurrentDateTime.Returns(now);
                ViewModel.Prepare(new EditDurationParameters(parameter));

                tickSubject.OnNext(now.AddHours(2));

                TestScheduler.Start();
                observer.LastValue().Hours.Should().Be(4);
            }
        }

        public sealed class TheDurationTimeProperty : EditDurationViewModelTest
        {
            [Property]
            public void IsUpdatedAccordingToTimeServiceForRunningTimeEntries(DateTimeOffset now, byte hours)
            {
                var duration = TimeSpan.FromHours(hours);
                var parameter = DurationParameter.WithStartAndDuration(now, null);
                var tickSubject = new Subject<DateTimeOffset>();
                var tickObservable = tickSubject.AsObservable().Publish();
                tickObservable.Connect();
                TimeService.CurrentDateTimeObservable.Returns(tickObservable);
                TimeService.CurrentDateTime.Returns(now);
                var durationObserver = TestScheduler.CreateObserver<TimeSpan>();
                ViewModel.Duration.Subscribe(durationObserver);

                ViewModel.Prepare(new EditDurationParameters(parameter));

                var newCurrentTime = now + duration;
                tickSubject.OnNext(newCurrentTime);

                TestScheduler.Start();
                durationObserver.LastValue().Should().Be(duration);
            }
        }

        public sealed class ThePrepareMethod : EditDurationViewModelTest
        {
            [Property]
            public void SetsTheStartTime(DateTimeOffset now)
            {
                var start = now;
                var parameter = DurationParameter.WithStartAndDuration(start, null);
                var startObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                ViewModel.StartTime.Subscribe(startObserver);

                ViewModel.Prepare(new EditDurationParameters(parameter));

                TestScheduler.Start();
                startObserver.LastValue().Should().Be(start);
            }

            [Property]
            public void SetsTheStartTimeToCurrentTimeIfParameterDoesNotHaveStartTime(DateTimeOffset now)
            {
                var start = now.AddHours(-2);
                var parameter = DurationParameter.WithStartAndDuration(start, null);
                TimeService.CurrentDateTime.Returns(now);
                var observer = TestScheduler.CreateObserver<DateTimeOffset>();
                ViewModel.StartTime.Subscribe(observer);

                ViewModel.Prepare(new EditDurationParameters(parameter));

                TestScheduler.Start();
                observer.LastValue().Should().BeSameDateAs(start);
            }

            [Property]
            public void SetsTheStopTimeToParameterStopTimeIfParameterHasStopTime(DateTimeOffset now)
            {
                var start = now.AddHours(-4);
                var stop = start.AddHours(2);
                var parameter = DurationParameter.WithStartAndDuration(start, stop - now);
                var observer = TestScheduler.CreateObserver<DateTimeOffset>();
                ViewModel.StartTime.Subscribe(observer);

                ViewModel.Prepare(new EditDurationParameters(parameter));

                TestScheduler.Start();
                observer.LastValue().Should().BeSameDateAs(start);
            }

            [Property]
            public void SubscribesToCurrentTimeObservableIfParameterDoesNotHaveStopTime(DateTimeOffset now)
            {
                var parameter = DurationParameter.WithStartAndDuration(now, null);
                TimeService.CurrentDateTimeObservable.Returns(Substitute.For<IObservable<DateTimeOffset>>());
                ViewModel.Prepare(new EditDurationParameters(parameter));

                TimeService.CurrentDateTimeObservable.Received().Subscribe(Arg.Any<AnonymousObserver<DateTimeOffset>>());
            }

            [Fact, LogIfTooSlow]
            public void SetsTheIsRunningPropertyWhenTheDurationIsNull()
            {
                var start = new DateTimeOffset(2018, 01, 15, 12, 34, 56, TimeSpan.Zero);
                var parameter = DurationParameter.WithStartAndDuration(start, null);
                var observer = TestScheduler.CreateObserver<bool>();
                ViewModel.IsRunning.Subscribe(observer);

                ViewModel.Prepare(new EditDurationParameters(parameter));

                TestScheduler.Start();
                observer.LastValue().Should().BeTrue();
            }

            [Fact, LogIfTooSlow]
            public void DoesNotSetTheIsRunningPropertyWhenTheDurationIsNotNull()
            {
                var start = new DateTimeOffset(2018, 01, 15, 12, 34, 56, TimeSpan.Zero);
                var duration = TimeSpan.FromMinutes(20);
                var parameter = DurationParameter.WithStartAndDuration(start, duration);
                var observer = TestScheduler.CreateObserver<bool>();
                ViewModel.IsRunning.Subscribe(observer);

                ViewModel.Prepare(new EditDurationParameters(parameter));

                TestScheduler.Start();
                observer.LastValue().Should().BeFalse();
            }
        }

        public sealed class TheCloseCommand : EditDurationViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task ClosesTheViewModel()
            {
                var parameter = DurationParameter.WithStartAndDuration(DateTimeOffset.UtcNow, null);
                ViewModel.Prepare(new EditDurationParameters(parameter));

                ViewModel.Close.Execute();

                TestScheduler.Start();
                await NavigationService.Received().Close(Arg.Is(ViewModel), Arg.Any<DurationParameter>());
            }

            [Fact, LogIfTooSlow]
            public async Task ReturnsTheDefaultParameter()
            {
                var parameter = DurationParameter.WithStartAndDuration(DateTimeOffset.UtcNow, null);
                ViewModel.Prepare(new EditDurationParameters(parameter));

                ViewModel.Close.Execute();

                TestScheduler.Start();
                await NavigationService.Received().Close(Arg.Is(ViewModel), Arg.Is(parameter));
            }
        }

        public sealed class TheSaveCommand : EditDurationViewModelTest
        {
            [Fact, LogIfTooSlow]
            public async Task ClosesTheViewModel()
            {
                var parameter = DurationParameter.WithStartAndDuration(DateTimeOffset.UtcNow, null);
                ViewModel.Prepare(new EditDurationParameters(parameter));

                ViewModel.Save.Execute();

                TestScheduler.Start();
                await NavigationService.Received().Close(Arg.Is(ViewModel), Arg.Any<DurationParameter>());
            }

            [Property]
            public void ReturnsAValueThatReflectsTheChangesToDurationForFinishedTimeEntries(DateTimeOffset start, DateTimeOffset stop)
            {
                if (start >= stop) return;

                var now = DateTimeOffset.UtcNow;
                TimeService.CurrentDateTime.Returns(now);
                if (start >= now) return;

                var durationObserver = TestScheduler.CreateObserver<TimeSpan>();
                var startObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                ViewModel.Duration.Subscribe(durationObserver);
                ViewModel.StartTime.Subscribe(startObserver);


                ViewModel.Prepare(new EditDurationParameters(DurationParameter.WithStartAndDuration(start, stop - start)));
                ViewModel.ChangeDuration.Execute(TimeSpan.FromMinutes(10));

                ViewModel.Save.Execute();

                TestScheduler.Start();
                NavigationService.Received().Close(Arg.Is(ViewModel), Arg.Is<DurationParameter>(
                    p => p.Start == startObserver.LastValue() && p.Duration == durationObserver.LastValue()
                )).Wait();
            }

            [Property]
            public void ReturnsAValueThatReflectsTheChangesToDurationForRunningTimeEntries(DateTimeOffset start, DateTimeOffset now)
            {
                if (start > now) return;
                TimeService.CurrentDateTime.Returns(now);
                var startObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                ViewModel.StartTime.Subscribe(startObserver);

                ViewModel.Prepare(new EditDurationParameters(DurationParameter.WithStartAndDuration(start, null)));
                ViewModel.ChangeDuration.Execute(TimeSpan.FromMinutes(10));

                ViewModel.Save.Execute();

                TestScheduler.Start();
                NavigationService.Received().Close(Arg.Is(ViewModel), Arg.Is<DurationParameter>(
                    p => p.Start == startObserver.LastValue() && p.Duration == null
                )).Wait();
            }
        }

        public sealed class TheEditStartTimeCommand : EditDurationViewModelTest
        {
            private static DurationParameter parameter = DurationParameter.WithStartAndDuration(
                new DateTimeOffset(2018, 01, 13, 0, 0, 0, TimeSpan.Zero),
                TimeSpan.FromMinutes(7));

            [Fact]
            public void SetsTheIsEditingFlagsCorrectlyWhenNothingWasEdited()
            {
                ViewModel.Prepare(new EditDurationParameters(parameter));
                var startObserver = TestScheduler.CreateObserver<bool>();
                var stopObserver = TestScheduler.CreateObserver<bool>();
                ViewModel.IsEditingStartTime.Subscribe(startObserver);
                ViewModel.IsEditingStopTime.Subscribe(stopObserver);

                ViewModel.EditStartTime.Execute();

                TestScheduler.Start();
                startObserver.LastValue().Should().BeTrue();
                stopObserver.LastValue().Should().BeFalse();
            }

            [Fact]
            public void SetsTheIsEditingFlagsCorrectlyWhenStopTimeWasEdited()
            {
                ViewModel.Prepare(new EditDurationParameters(parameter));
                var startObserver = TestScheduler.CreateObserver<bool>();
                var stopObserver = TestScheduler.CreateObserver<bool>();
                ViewModel.IsEditingStartTime.Subscribe(startObserver);
                ViewModel.IsEditingStopTime.Subscribe(stopObserver);
                ViewModel.EditStopTime.Execute();

                ViewModel.EditStartTime.Execute();

                TestScheduler.Start();
                startObserver.LastValue().Should().BeTrue();
                stopObserver.LastValue().Should().BeFalse();
            }

            [Fact]
            public void ClosesEditingWhenStartTimeWasBeingEdited()
            {
                ViewModel.Prepare(new EditDurationParameters(parameter));
                var startObserver = TestScheduler.CreateObserver<bool>();
                var stopObserver = TestScheduler.CreateObserver<bool>();
                ViewModel.IsEditingStartTime.Subscribe(startObserver);
                ViewModel.IsEditingStopTime.Subscribe(stopObserver);

                Observable.Concat(
                    Observable.Defer(() => ViewModel.EditStartTime.Execute()),
                    Observable.Defer(() => ViewModel.EditStartTime.Execute())
                    )
                    .Subscribe();

                TestScheduler.Start();
                startObserver.LastValue().Should().BeFalse();
                stopObserver.LastValue().Should().BeFalse();
            }

            [Fact]
            public void SetsTheMinimumAndMaximumDateForTheDatePicker()
            {
                ViewModel.Prepare(new EditDurationParameters(parameter));
                var minTimeObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                var maxTimeObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                ViewModel.MinimumDateTime.Subscribe(minTimeObserver);
                ViewModel.MaximumDateTime.Subscribe(maxTimeObserver);

                ViewModel.EditStartTime.Execute();

                TestScheduler.Start();
                minTimeObserver.LastValue().Should().Be((parameter.Start + parameter.Duration.Value - TimeSpan.FromHours(999)));
                maxTimeObserver.LastValue().Should().Be((parameter.Start + parameter.Duration.Value));
            }
        }

        public sealed class TheEditStopTimeCommand : EditDurationViewModelTest
        {
            private static DurationParameter parameter = DurationParameter.WithStartAndDuration(
                new DateTimeOffset(2018, 01, 13, 0, 0, 0, TimeSpan.Zero),
                TimeSpan.FromMinutes(7));

            [Fact]
            public void SetsTheIsEditingFlagsCorrectlyWhenNothingWasEdited()
            {
                ViewModel.Prepare(new EditDurationParameters(parameter));
                var startObserver = TestScheduler.CreateObserver<bool>();
                var stopObserver = TestScheduler.CreateObserver<bool>();
                ViewModel.IsEditingStartTime.Subscribe(startObserver);
                ViewModel.IsEditingStopTime.Subscribe(stopObserver);

                ViewModel.EditStopTime.Execute();

                TestScheduler.Start();
                startObserver.LastValue().Should().BeFalse();
                stopObserver.LastValue().Should().BeTrue();
            }

            [Fact]
            public void SetsTheIsEditingFlagsCorrectlyWhenStopTimeWasEdited()
            {
                ViewModel.Prepare(new EditDurationParameters(parameter));
                var startObserver = TestScheduler.CreateObserver<bool>();
                var stopObserver = TestScheduler.CreateObserver<bool>();
                ViewModel.IsEditingStartTime.Subscribe(startObserver);
                ViewModel.IsEditingStopTime.Subscribe(stopObserver);
                ViewModel.EditStartTime.Execute();

                ViewModel.EditStopTime.Execute();

                TestScheduler.Start();
                startObserver.LastValue().Should().BeFalse();
                stopObserver.LastValue().Should().BeTrue();
            }

            [Fact]
            public void ClosesEditingWhenStartTimeWasBeingEdited()
            {
                ViewModel.Prepare(new EditDurationParameters(parameter));
                var startObserver = TestScheduler.CreateObserver<bool>();
                var stopObserver = TestScheduler.CreateObserver<bool>();
                ViewModel.IsEditingStartTime.Subscribe(startObserver);
                ViewModel.IsEditingStopTime.Subscribe(stopObserver);

                Observable.Concat(
                        Observable.Defer(() => ViewModel.EditStopTime.Execute()),
                        Observable.Defer(() => ViewModel.EditStopTime.Execute())
                    )
                    .Subscribe();

                TestScheduler.Start();
                startObserver.LastValue().Should().BeFalse();
                stopObserver.LastValue().Should().BeFalse();
            }

            [Fact]
            public void SetsTheMinimumAndMaximumDateForTheDatePicker()
            {
                ViewModel.Prepare(new EditDurationParameters(parameter));
                var minTimeObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                var maxTimeObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                ViewModel.MinimumDateTime.Subscribe(minTimeObserver);
                ViewModel.MaximumDateTime.Subscribe(maxTimeObserver);

                ViewModel.EditStopTime.Execute();

                TestScheduler.Start();
                minTimeObserver.LastValue().Should().Be(parameter.Start);
                maxTimeObserver.LastValue().Should().Be(parameter.Start + TimeSpan.FromHours(999));
            }

            [Fact]
            public void StopsARunningTimeEntry()
            {
                var now = new DateTimeOffset(2018, 02, 20, 0, 0, 0, TimeSpan.Zero);
                var runningTEParameter = DurationParameter.WithStartAndDuration(parameter.Start, null);
                ViewModel.Prepare(new EditDurationParameters(runningTEParameter));
                TimeService.CurrentDateTime.Returns(now);
                var stopObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                var isRunningObserver = TestScheduler.CreateObserver<bool>();
                ViewModel.StopTime.Subscribe(stopObserver);
                ViewModel.IsRunning.Subscribe(isRunningObserver);


                ViewModel.EditStopTime.Execute();

                TestScheduler.Start();
                isRunningObserver.LastValue().Should().BeFalse();
                stopObserver.LastValue().Should().Be(now);
            }

            [Fact]
            public void UnsubscribesFromTheTheRunningTimeEntryObservable()
            {
                var now = new DateTimeOffset(2018, 02, 20, 0, 0, 0, TimeSpan.Zero);
                var runningTEParameter = DurationParameter.WithStartAndDuration(parameter.Start, null);
                var subject = new BehaviorSubject<DateTimeOffset>(now);
                var observable = subject.AsObservable().Publish();
                var stopObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                ViewModel.StopTime.Subscribe(stopObserver);
                ViewModel.Prepare(new EditDurationParameters(runningTEParameter));
                TimeService.CurrentDateTime.Returns(now);
                TimeService.CurrentDateTimeObservable.Returns(observable);

                ViewModel.EditStopTime.Execute();
                subject.OnNext(now.AddSeconds(1));

                TestScheduler.Start();
                stopObserver.LastValue().Should().Be(now);
            }
        }

        public sealed class TheStopEditingTimeCommand : EditDurationViewModelTest
        {
            private static DurationParameter parameter = DurationParameter.WithStartAndDuration(
                new DateTimeOffset(2018, 01, 13, 0, 0, 0, TimeSpan.Zero),
                TimeSpan.FromMinutes(7));

            [Fact]
            public void ClearsAllTimeEditingFlagsWhenStartTimeWasEdited()
            {
                ViewModel.Prepare(new EditDurationParameters(parameter));
                var editingObserver = TestScheduler.CreateObserver<bool>();
                var startObserver = TestScheduler.CreateObserver<bool>();
                var stopObserver = TestScheduler.CreateObserver<bool>();
                ViewModel.IsEditingTime.Subscribe(editingObserver );
                ViewModel.IsEditingStartTime.Subscribe(startObserver);
                ViewModel.IsEditingStopTime.Subscribe(stopObserver);

                ViewModel.EditStartTime.Execute();
                ViewModel.StopEditingTime.Execute();

                TestScheduler.Start();
                editingObserver.LastValue().Should().BeFalse();
                startObserver.LastValue().Should().BeFalse();
                stopObserver.LastValue().Should().BeFalse();
            }

            [Fact]
            public void ClearsAllTimeEditingFlagsWhenStopTimeWasEdited()
            {
                ViewModel.Prepare(new EditDurationParameters(parameter));
                var editingObserver = TestScheduler.CreateObserver<bool>();
                var startObserver = TestScheduler.CreateObserver<bool>();
                var stopObserver = TestScheduler.CreateObserver<bool>();
                ViewModel.IsEditingTime.Subscribe(editingObserver );
                ViewModel.IsEditingStartTime.Subscribe(startObserver);
                ViewModel.IsEditingStopTime.Subscribe(stopObserver);

                ViewModel.EditStopTime.Execute();
                ViewModel.StopEditingTime.Execute();

                TestScheduler.Start();
                editingObserver.LastValue().Should().BeFalse();
                startObserver.LastValue().Should().BeFalse();
                stopObserver.LastValue().Should().BeFalse();
            }
        }

        public sealed class TheChangeActiveTimeAction : EditDurationViewModelTest
        {
            private static DurationParameter parameter = DurationParameter.WithStartAndDuration(
                new DateTimeOffset(2018, 01, 13, 0, 0, 0, TimeSpan.Zero),
                TimeSpan.FromMinutes(7));

            [Fact]
            public void DoesNotAcceptAnyValueWhenNotEditingNeitherStartNorStopTime()
            {
                var editedValue = new DateTimeOffset(2018, 02, 20, 0, 0, 0, TimeSpan.Zero);
                ViewModel.Prepare(new EditDurationParameters(parameter));

                ViewModel.ChangeActiveTime.Execute(editedValue);

                ViewModel.StartTime.Should().NotBe(editedValue);
                ViewModel.StopTime.Should().NotBe(editedValue);
            }

            [Fact]
            public void ChangesJustTheStartTime()
            {
                var editedValue = new DateTimeOffset(2018, 01, 07, 0, 0, 0, TimeSpan.Zero);
                ViewModel.Prepare(new EditDurationParameters(parameter));
                var startObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                var stopObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                ViewModel.StartTime.Subscribe(startObserver);
                ViewModel.StopTime.Subscribe(stopObserver);

                ViewModel.EditStartTime.Execute();
                ViewModel.ChangeActiveTime.Execute(editedValue);

                TestScheduler.Start();
                startObserver.LastValue().Should().Be(editedValue);
                stopObserver.LastValue().Should().NotBe(editedValue);
            }

            [Fact]
            public void DoesNotAllowChangingTheStartTimeToMoreThanTheMaximumDate()
            {
                var editedValue = parameter.Start.Add(parameter.Duration.Value).AddHours(1);
                ViewModel.Prepare(new EditDurationParameters(parameter));
                var startObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                var stopObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                var maxTimeObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                ViewModel.StartTime.Subscribe(startObserver);
                ViewModel.StopTime.Subscribe(stopObserver);
                ViewModel.MaximumDateTime.Subscribe(maxTimeObserver);

                ViewModel.EditStartTime.Execute();
                ViewModel.ChangeActiveTime.Execute(editedValue);

                TestScheduler.Start();
                startObserver.LastValue().Should().Be(maxTimeObserver.LastValue());
                stopObserver.LastValue().Should().Be(maxTimeObserver.LastValue());
            }

            [Fact]
            public void DoesNotAllowChangingTheStartTimeToLessThanTheMinimumDate()
            {
                var editedValue = parameter.Start.AddHours(-1000);
                ViewModel.Prepare(new EditDurationParameters(parameter));
                var startObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                var stopObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                var minTimeObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                ViewModel.StartTime.Subscribe(startObserver);
                ViewModel.StopTime.Subscribe(stopObserver);
                ViewModel.MinimumDateTime.Subscribe(minTimeObserver);

                ViewModel.EditStartTime.Execute();
                ViewModel.ChangeActiveTime.Execute(editedValue);

                TestScheduler.Start();
                startObserver.LastValue().Should().Be(minTimeObserver.LastValue());
                stopObserver.LastValue().Should().NotBe(minTimeObserver.LastValue());
            }

            [Fact]
            public void ChangesJustTheStopTime()
            {
                var editedValue = new DateTimeOffset(2018, 02, 20, 0, 0, 0, TimeSpan.Zero);
                ViewModel.Prepare(new EditDurationParameters(parameter));
                var startObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                var stopObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                ViewModel.StartTime.Subscribe(startObserver);
                ViewModel.StopTime.Subscribe(stopObserver);

                ViewModel.EditStopTime.Execute();
                ViewModel.ChangeActiveTime.Execute(editedValue);

                TestScheduler.Start();
                startObserver.LastValue().Should().NotBe(editedValue);
                stopObserver.LastValue().Should().Be(editedValue);
            }

            [Fact]
            public void DoesNotAllowChangingTheStopTimeToMoreThanTheMaximumDate()
            {
                var editedValue = parameter.Start.AddHours(1000);
                ViewModel.Prepare(new EditDurationParameters(parameter));
                var stopObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                var maxTimeObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                ViewModel.StopTime.Subscribe(stopObserver);
                ViewModel.MaximumDateTime.Subscribe(maxTimeObserver);

                ViewModel.EditStopTime.Execute();
                ViewModel.ChangeActiveTime.Execute(editedValue);

                TestScheduler.Start();
                stopObserver.LastValue().Should().Be(maxTimeObserver.LastValue());
            }

            [Fact]
            public void DoesNotAllowChangingTheStopTimeToLessThanTheMinimumDate()
            {
                var editedValue = parameter.Start.AddHours(-1);
                ViewModel.Prepare(new EditDurationParameters(parameter));
                var stopObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                var minTimeObserver = TestScheduler.CreateObserver<DateTimeOffset>();
                ViewModel.StopTime.Subscribe(stopObserver);
                ViewModel.MinimumDateTime.Subscribe(minTimeObserver );

                ViewModel.EditStopTime.Execute();
                ViewModel.ChangeActiveTime.Execute(editedValue);

                TestScheduler.Start();
                stopObserver.LastValue().Should().Be(minTimeObserver.LastValue());
            }
        }

        public sealed class TheIsDurationInitiallyFocusedProperty : EditDurationViewModelTest
        {
            private static DurationParameter parameter = DurationParameter.WithStartAndDuration(
                new DateTimeOffset(2018, 01, 13, 0, 0, 0, TimeSpan.Zero),
                TimeSpan.FromMinutes(7));

            [Fact]
            public void DefaultToNone()
            {
                ViewModel.Prepare(new EditDurationParameters(parameter));
                ViewModel.IsDurationInitiallyFocused.Should().Be(false);
            }

            [Fact]
            public void ShouldBeSetProperly()
            {
                ViewModel.Prepare(new EditDurationParameters(parameter, isStartingNewEntry: true, isDurationInitiallyFocused: true));
                ViewModel.IsDurationInitiallyFocused.Should().Be(true);
            }
        }

        public sealed class TheAnalyticsService : EditDurationViewModelTest
        {
            private static readonly DurationParameter parameter = DurationParameter.WithStartAndDuration(
                new DateTimeOffset(2018, 01, 13, 0, 0, 0, TimeSpan.Zero),
                TimeSpan.FromMinutes(7));

            [Fact, LogIfTooSlow]
            public void ReceivesEventWhenViewModelCloses()
            {
                ViewModel.Prepare(new EditDurationParameters(parameter));

                ViewModel.Close.Execute();

                TestScheduler.Start();
                AnalyticsService.Received().Track(
                    Arg.Is<ITrackableEvent>(trackableEvent =>
                        trackableEvent.EventName == "EditDuration"
                        && trackableEvent.ToDictionary().ContainsKey("navigationOrigin")
                        && trackableEvent.ToDictionary().ContainsKey("result")
                        && trackableEvent.ToDictionary()["navigationOrigin"] == EditDurationEvent.NavigationOrigin.Edit.ToString()
                        && trackableEvent.ToDictionary()["result"] == EditDurationEvent.Result.Cancel.ToString()
                    )
                );
            }

            [Fact, LogIfTooSlow]
            public void ReceivesEventWhenViewModelSaves()
            {
                ViewModel.Prepare(new EditDurationParameters(parameter, isStartingNewEntry: true));

                ViewModel.Save.Execute();

                TestScheduler.Start();
                AnalyticsService.Received().Track(
                    Arg.Is<ITrackableEvent>(trackableEvent =>
                        trackableEvent.EventName == "EditDuration"
                        && trackableEvent.ToDictionary().ContainsKey("navigationOrigin")
                        && trackableEvent.ToDictionary().ContainsKey("result")
                        && trackableEvent.ToDictionary()["navigationOrigin"] == EditDurationEvent.NavigationOrigin.Start.ToString()
                        && trackableEvent.ToDictionary()["result"] == EditDurationEvent.Result.Save.ToString()
                    )
                );
            }

            [Fact, LogIfTooSlow]
            public void SetsCorrectParametersOnEdition()
            {
                ViewModel.Prepare(new EditDurationParameters(parameter));

                ViewModel.TimeEditedWithSource(EditTimeSource.WheelBothTimes);
                ViewModel.TimeEditedWithSource(EditTimeSource.BarrelStartDate);
                ViewModel.Save.Execute();

                TestScheduler.Start();
                AnalyticsService.Received().Track(
                    Arg.Is<ITrackableEvent>(trackableEvent =>
                        trackableEvent.EventName == "EditDuration"
                        && trackableEvent.ToDictionary().ContainsKey("changedBothTimesWithWheel")
                        && trackableEvent.ToDictionary().ContainsKey("changedStartDateWithBarrel")
                        && trackableEvent.ToDictionary().ContainsKey("changedEndDateWithBarrel")
                        && trackableEvent.ToDictionary()["changedBothTimesWithWheel"] == true.ToString()
                        && trackableEvent.ToDictionary()["changedStartDateWithBarrel"] == true.ToString()
                        && trackableEvent.ToDictionary()["changedEndDateWithBarrel"] == false.ToString()
                    )
                );
            }
        }
    }
}
