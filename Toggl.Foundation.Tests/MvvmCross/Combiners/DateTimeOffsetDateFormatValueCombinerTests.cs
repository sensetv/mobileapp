using System;
using System.Collections.Generic;
using System.Globalization;
using FluentAssertions;
using MvvmCross.Binding.Bindings.SourceSteps;
using MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.Combiners;
using Toggl.Multivac;
using Xunit;

namespace Toggl.Foundation.Tests.MvvmCross.Combiners
{
    public sealed class DateTimeOffsetDateFormatValueCombinerTests
    {
        private static bool tryGetValue(DateTimeOffsetDateFormatValueCombiner combiner, DateTimeOffset duration, object format, out object convertedValue)
        {
            var dateStepDescription = new MvxLiteralSourceStepDescription();
            dateStepDescription.Literal = duration;

            var formatStepDescription = new MvxLiteralSourceStepDescription();
            formatStepDescription.Literal = format;

            var steps = new[]
            {
                new MvxLiteralSourceStep(dateStepDescription),
                new MvxLiteralSourceStep(formatStepDescription)
            };

            return combiner.TryGetValue(steps, out convertedValue);
        }

        public sealed class TheTryGetValueMethod
        {
            public sealed class InvalidFormats
            {
                [Theory, LogIfTooSlow]
                [InlineData(null)]
                [InlineData("YYYY-MM-DD")]
                [InlineData(false)]
                public void FailsForFormatsWhichAreNotDateFormat(object parameter)
                {
                    var combiner = new DateTimeOffsetDateFormatValueCombiner(TimeZoneInfo.Utc, true);
                    var successful = tryGetValue(combiner, DateTimeOffset.UtcNow, parameter, out var convertedValue);

                    successful.Should().BeFalse();
                    convertedValue.Should().Be(MvxBindingConstant.UnsetValue);
                }
            }

            public sealed class ValidFormats
            {
                [Theory, LogIfTooSlow]
                [MemberData(nameof(DateFormats))]
                public void WorksForValidFormats(DateFormat format)
                {
                    var now = DateTimeOffset.UtcNow;
                    var combiner = new DateTimeOffsetDateFormatValueCombiner(TimeZoneInfo.Utc, true);
                    var successful = tryGetValue(combiner, now, format, out var convertedValue);

                    successful.Should().BeTrue();
                    convertedValue.Should().BeOfType<string>();
                    convertedValue.Should().Be(now.ToString(format.Long, CultureInfo.InvariantCulture));
                }

                public static IEnumerable<object[]> DateFormats()
                    => new List<object[]>
                    {
                        new object[] { DateFormat.FromLocalizedDateFormat("YYYY-MM-DD") },
                        new object[] { DateFormat.FromLocalizedDateFormat("DD.MM.YYYY") },
                        new object[] { DateFormat.FromLocalizedDateFormat("DD-MM-YYYY") },
                        new object[] { DateFormat.FromLocalizedDateFormat("MM/DD/YYYY") },
                        new object[] { DateFormat.FromLocalizedDateFormat("DD/MM/YYYY") },
                        new object[] { DateFormat.FromLocalizedDateFormat("MM-DD-YYYY") }
                    };
            }
        }
    }
}
