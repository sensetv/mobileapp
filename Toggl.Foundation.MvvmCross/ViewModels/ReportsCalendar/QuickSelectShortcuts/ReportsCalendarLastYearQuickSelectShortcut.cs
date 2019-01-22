using System;
using Toggl.Foundation.Analytics;
using Toggl.Foundation.MvvmCross.Parameters;
using Toggl.Foundation.Services;

namespace Toggl.Foundation.MvvmCross.ViewModels.ReportsCalendar.QuickSelectShortcuts
{
    public sealed class ReportsCalendarLastYearQuickSelectShortcut
        : ReportsCalendarBaseQuickSelectShortcut
    {
        public ReportsCalendarLastYearQuickSelectShortcut(ITimeService timeService)
            : base(timeService, Resources.LastYear, ReportPeriod.LastYear)
        {
        }

        public override ReportsDateRangeParameter GetDateRange()
        {
            var lastYear = TimeService.CurrentDateTime.Year - 1;
            var start = new DateTimeOffset(lastYear, 1, 1, 0, 0, 0, TimeSpan.Zero);
            var end = start.AddYears(1).AddDays(-1);
            return ReportsDateRangeParameter
                .WithDates(start, end)
                .WithSource(ReportsSource.ShortcutLastYear);
        }
    }
}
