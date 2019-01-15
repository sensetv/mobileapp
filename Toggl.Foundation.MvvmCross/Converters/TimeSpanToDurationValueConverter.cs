using System;
using System.Globalization;
using MvvmCross.Converters;
using Toggl.Multivac;

namespace Toggl.Foundation.MvvmCross.Converters
{
    [Preserve(AllMembers = true)]
    public sealed class TimeSpanToDurationValueConverter : MvxValueConverter<TimeSpan, string>
    {
        protected override string Convert(TimeSpan value, Type targetType, object parameter, CultureInfo culture)
            => $"{(int)value.TotalHours}:{value.Minutes.ToString("D2", CultureInfo.InvariantCulture)}:{value.Seconds.ToString("D2", CultureInfo.InvariantCulture)}";
    }
}
