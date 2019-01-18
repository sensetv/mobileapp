using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using FluentAssertions;
using NSubstitute;
using NSubstitute.Core;
using Toggl.Foundation.DataSources;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.Models.Interfaces;
using Toggl.Foundation.MvvmCross.Onboarding.MainView;
using Toggl.Foundation.Tests.Mocks;
using Toggl.Multivac.Extensions;
using Toggl.PrimeRadiant.Models;
using Xunit;
using TimeEntryExtensions = Toggl.Foundation.Models.TimeEntryExtensions;

namespace Toggl.Foundation.Tests.Interactors.TimeEntry
{
    public sealed class TimeTrackedTodayInteractorTests
    {
        private static readonly DateTimeOffset now = new DateTimeOffset(2018, 12, 31, 1, 2, 3, TimeSpan.Zero);
        private static readonly IThreadSafeWorkspace accessibleWorkspace = new MockWorkspace { IsInaccessible = false };
        private static readonly IThreadSafeWorkspace inaccessibleWorkspace = new MockWorkspace { IsInaccessible = true };

        public sealed class WhenThereIsNoRunningTimeEntry : BaseInteractorTests
        {
            private readonly ISubject<Unit> timeEntryChange = new Subject<Unit>();
            private readonly ISubject<Unit> midnight = new Subject<Unit>();
            private readonly ISubject<Unit> significantTimeChange = new Subject<Unit>();

            private readonly ObserveTimeTrackedTodayInteractor interactor;


            private readonly IThreadSafeTimeEntry[] timeEntries =
            {
                new MockTimeEntry { Start = now.AddDays(-1), Duration = 1, Workspace = accessibleWorkspace },
                new MockTimeEntry { Start = now, Duration = 2, Workspace = accessibleWorkspace },
                new MockTimeEntry { Start = now, Duration = 3, Workspace = accessibleWorkspace },
                new MockTimeEntry { Start = now.AddDays(1), Duration = 4, Workspace = accessibleWorkspace }
            };

            public WhenThereIsNoRunningTimeEntry()
            {
                DataSource.TimeEntries.Created.Returns(timeEntryChange.Select(_ => new MockTimeEntry()));
                DataSource.TimeEntries.Updated.Returns(Observable.Never<EntityUpdate<IThreadSafeTimeEntry>>());
                DataSource.TimeEntries.Deleted.Returns(Observable.Never<long>());
                TimeService.MidnightObservable.Returns(midnight.Select(_ => now));
                TimeService.SignificantTimeChangeObservable.Returns(significantTimeChange);
                TimeService.CurrentDateTime.Returns(now);

                interactor = new ObserveTimeTrackedTodayInteractor(TimeService, DataSource.TimeEntries);
            }

            [Fact, LogIfTooSlow]
            public async Task SumsTheDurationOfTheTimeEntriesStartedOnTheCurrentDay()
            {
                DataSource.TimeEntries.GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>(), Arg.Any<bool>())
                    .Returns(wherePredicateApplies(timeEntries));

                var time = await interactor.Execute().FirstAsync();

                time.TotalSeconds.Should().Be(5);
            }

            [Fact, LogIfTooSlow]
            public void RecalculatesTheSumOfTheDurationOfTheTimeEntriesStartedOnTheCurrentDayWhenTimeEntriesChange()
            {
                recalculatesOn(timeEntryChange);
            }

            [Fact, LogIfTooSlow]
            public void RecalculatesTheSumOfTheDurationOfTheTimeEntriesOnMidnight()
            {
                recalculatesOn(midnight);
            }

            [Fact, LogIfTooSlow]
            public void RecalculatesTheSumOfTheDurationOfTheTimeEntriesWhenThereIsSignificantTimeChange()
            {
                recalculatesOn(significantTimeChange);
            }

            [Fact, LogIfTooSlow]
            public void DoesNotCountDeletedTimeEntries()
            {
                var timeEntriesWhereOneIsDeleted = timeEntries.Concat(new[] { new MockTimeEntry { Start = now, Duration = 5, IsDeleted = true, Workspace = accessibleWorkspace } });
                DataSource.TimeEntries.GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>(), Arg.Any<bool>())
                    .Returns(wherePredicateApplies(timeEntriesWhereOneIsDeleted));
                var observer = Substitute.For<IObserver<TimeSpan>>();

                interactor.Execute().Subscribe(observer);

                observer.Received().OnNext(TimeSpan.FromSeconds(5));
            }

            [Fact, LogIfTooSlow]
            public void DoesNotCountTimeEntriesInInaccessibleWorkspaces()
            {
                var timeEntriesWhereOneIsInaccessible = timeEntries.Concat(new[] { new MockTimeEntry { Start = now, Duration = 5, Workspace = inaccessibleWorkspace } });
                DataSource.TimeEntries.GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>(), Arg.Any<bool>())
                    .Returns(wherePredicateApplies(timeEntriesWhereOneIsInaccessible));
                var observer = Substitute.For<IObserver<TimeSpan>>();

                interactor.Execute().Subscribe(observer);

                observer.Received().OnNext(TimeSpan.FromSeconds(5));
            }

            [Fact, LogIfTooSlow]
            public void UpdatesWhenTimeEntryIsDeleted()
            {
                var timeEntriesWhereNoOneIsDeleted = timeEntries.Concat(new[] { new MockTimeEntry { Start = now, Duration = 5, Workspace = accessibleWorkspace } });
                var timeEntriesWhereOneIsDeleted = timeEntries.Concat(new[] { new MockTimeEntry { Start = now, Duration = 5, IsDeleted = true, Workspace = accessibleWorkspace } });
                DataSource.TimeEntries.GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>(), Arg.Any<bool>())
                    .Returns(wherePredicateApplies(timeEntriesWhereNoOneIsDeleted), wherePredicateApplies(timeEntriesWhereOneIsDeleted));
                var observer = Substitute.For<IObserver<TimeSpan>>();

                interactor.Execute().Subscribe(observer);
                timeEntryChange.OnNext(Unit.Default);

                Received.InOrder(() =>
                {
                    observer.OnNext(TimeSpan.FromSeconds(10));
                    observer.OnNext(TimeSpan.FromSeconds(5));
                });
            }

            private void recalculatesOn(IObserver<Unit> trigger)
            {
                var updatedTimeEntries = timeEntries.Concat(new[] { new MockTimeEntry { Start = now, Duration = 5, Workspace = accessibleWorkspace } });
                DataSource.TimeEntries.GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>(), Arg.Any<bool>())
                    .Returns(wherePredicateApplies(timeEntries), wherePredicateApplies(updatedTimeEntries));
                var observer = Substitute.For<IObserver<TimeSpan>>();

                interactor.Execute().Skip(1).Subscribe(observer);
                trigger.OnNext(Unit.Default);

                observer.Received().OnNext(TimeSpan.FromSeconds(10));
            }
        }

        public sealed class WhenThereIsARunningTimeEntry : BaseInteractorTests
        {
            private readonly ISubject<DateTimeOffset> currentDateTimeSubject = new Subject<DateTimeOffset>();
            private readonly ISubject<Unit> timeEntriesUpdated = new Subject<Unit>();
            private readonly ITimeService timeService = Substitute.For<ITimeService>();

            private readonly IThreadSafeTimeEntry[] timeEntries =
            {
                new MockTimeEntry { Start = now.AddDays(-1), Duration = 1, Workspace = accessibleWorkspace },
                new MockTimeEntry { Start = now, Duration = 2, Workspace = accessibleWorkspace },
                new MockTimeEntry { Start = now, Duration = 3, Workspace = accessibleWorkspace },
                new MockTimeEntry { Start = now, Duration = null, Workspace = accessibleWorkspace },
                new MockTimeEntry { Start = now.AddDays(1), Duration = 4, Workspace = accessibleWorkspace }
            };

            public WhenThereIsARunningTimeEntry()
            {
                timeService.CurrentDateTimeObservable.Returns(currentDateTimeSubject);
                timeService.CurrentDateTime.Returns(now);
                timeService.MidnightObservable.Returns(Observable.Never<DateTimeOffset>());
                timeService.SignificantTimeChangeObservable.Returns(Observable.Never<Unit>());
                currentDateTimeSubject.Subscribe(currentTime => timeService.CurrentDateTime.Returns(currentTime));

                var timeEntriesSource = Substitute.For<ITimeEntriesSource>();
                timeEntriesSource.Created.Returns(Observable.Never<IThreadSafeTimeEntry>());
                timeEntriesSource.Updated.Returns(timeEntriesUpdated.Select(_ => default(EntityUpdate<IThreadSafeTimeEntry>)));
                timeEntriesSource.Deleted.Returns(Observable.Never<long>());

                DataSource.TimeEntries.Returns(timeEntriesSource);
            }

            [Fact, LogIfTooSlow]
            public async Task ReturnsATickingObservable()
            {
                DataSource.TimeEntries.GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>(), Arg.Any<bool>())
                    .Returns(wherePredicateApplies(timeEntries));

                var observer = Substitute.For<IObserver<TimeSpan>>();

                var interactor = new ObserveTimeTrackedTodayInteractor(timeService, DataSource.TimeEntries);
                interactor.Execute().Subscribe(observer);
                currentDateTimeSubject.OnNext(now.AddSeconds(1));
                currentDateTimeSubject.OnNext(now.AddSeconds(2));
                currentDateTimeSubject.OnNext(now.AddSeconds(3));

                Received.InOrder(() =>
                {
                    observer.OnNext(Arg.Is(TimeSpan.FromSeconds(5)));
                    observer.OnNext(Arg.Is(TimeSpan.FromSeconds(6)));
                    observer.OnNext(Arg.Is(TimeSpan.FromSeconds(7)));
                    observer.OnNext(Arg.Is(TimeSpan.FromSeconds(8)));
                });
            }

            [Fact, LogIfTooSlow]
            public void StopsTickingWhenRunningTimeEntryIsStopped()
            {
                var stoppedTimeEntries = timeEntries
                    .Select(te => te.IsRunning() ? TimeEntryExtensions.With(te, 3) : te)
                    .ToArray();
                var observer = Substitute.For<IObserver<TimeSpan>>();

                updateRunningTimeEntryAfterThreeSeconds(stoppedTimeEntries, observer);

                observer.Received(4).OnNext(Arg.Any<TimeSpan>());
                Received.InOrder(() =>
                {
                    observer.OnNext(Arg.Is(TimeSpan.FromSeconds(5)));
                    observer.OnNext(Arg.Is(TimeSpan.FromSeconds(6)));
                    observer.OnNext(Arg.Is(TimeSpan.FromSeconds(7)));
                    observer.OnNext(Arg.Is(TimeSpan.FromSeconds(8)));
                });
            }

            [Fact, LogIfTooSlow]
            public void StopsTickingWhenRunningTimeEntryIsDeleted()
            {
                var withDeletedRunningTimeEntry = timeEntries
                    .Select(te => te.IsRunning()
                        ? new MockTimeEntry { Start = te.Start, Duration = null, IsDeleted = true, Workspace = accessibleWorkspace }
                        : te)
                    .ToArray();
                var observer = Substitute.For<IObserver<TimeSpan>>();

                updateRunningTimeEntryAfterThreeSeconds(withDeletedRunningTimeEntry, observer);

                observer.Received(5).OnNext(Arg.Any<TimeSpan>());
                Received.InOrder(() =>
                {
                    observer.OnNext(Arg.Is(TimeSpan.FromSeconds(5)));
                    observer.OnNext(Arg.Is(TimeSpan.FromSeconds(6)));
                    observer.OnNext(Arg.Is(TimeSpan.FromSeconds(7)));
                    observer.OnNext(Arg.Is(TimeSpan.FromSeconds(8)));
                    observer.OnNext(Arg.Is(TimeSpan.FromSeconds(5)));
                });
            }

            private void updateRunningTimeEntryAfterThreeSeconds(
                IEnumerable<IThreadSafeTimeEntry> updatedTimeEntries, IObserver<TimeSpan> observer)
            {
                var interactor = new ObserveTimeTrackedTodayInteractor(timeService, DataSource.TimeEntries);

                DataSource.TimeEntries.GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>(), Arg.Any<bool>())
                    .Returns(wherePredicateApplies(timeEntries));

                interactor.Execute().Subscribe(observer);
                currentDateTimeSubject.OnNext(now.AddSeconds(1));
                currentDateTimeSubject.OnNext(now.AddSeconds(2));
                currentDateTimeSubject.OnNext(now.AddSeconds(3));

                DataSource.TimeEntries.GetAll(Arg.Any<Func<IDatabaseTimeEntry, bool>>(), Arg.Any<bool>())
                    .Returns(wherePredicateApplies(updatedTimeEntries));
                timeEntriesUpdated.OnNext(Unit.Default);

                currentDateTimeSubject.OnNext(now.AddSeconds(4));
                currentDateTimeSubject.OnNext(now.AddSeconds(5));
                currentDateTimeSubject.OnNext(now.AddSeconds(6));
            }
        }

        private static Func<CallInfo, IObservable<IEnumerable<IThreadSafeTimeEntry>>> wherePredicateApplies(
            IEnumerable<IThreadSafeTimeEntry> entries)
            => callInfo =>
            {
                var predicate = callInfo.Arg<Func<IThreadSafeTimeEntry, bool>>();
                var filteredEntries = predicate == null ? entries : entries.Where(predicate);

                return Observable.Return(filteredEntries);
            };
    }
}
