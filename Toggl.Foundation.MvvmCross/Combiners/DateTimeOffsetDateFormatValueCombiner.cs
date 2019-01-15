using System;
using System.Globalization;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.Combiners
{
    [Preserve(AllMembers = true)]
    public sealed class DateTimeOffsetDateFormatValueCombiner : BaseTwoValuesCombiner<DateTimeOffset, DateFormat>
    {
        private readonly TimeZoneInfo timeZone;

        private readonly bool useLongFormat;

        public DateTimeOffsetDateFormatValueCombiner(TimeZoneInfo timeZone, bool useLongFormat = true)
        {
            this.timeZone = timeZone;
            this.useLongFormat = useLongFormat;
        }

        protected override object Combine(DateTimeOffset date, DateFormat format)
        {
            var formatVariant = useLongFormat ? format.Long : format.Short;
            return getDateTimeOffsetInCorrectTimeZone(date).ToString(formatVariant, CultureInfo.InvariantCulture);
        }

        private DateTimeOffset getDateTimeOffsetInCorrectTimeZone(DateTimeOffset value)
            => value == default(DateTimeOffset) ? value : TimeZoneInfo.ConvertTime(value, timeZone);
    }
}
