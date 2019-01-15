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
    public sealed class DateTimeOffsetTimeFormatValueCombinerTests
    {
        private static bool tryGetValue(DateTimeOffsetTimeFormatValueCombiner combiner, DateTimeOffset duration, object format, out object convertedValue)
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
                [InlineData("H:mm")]
                [InlineData(false)]
                public void FailsForFormatsWhichAreNotTimeFormat(object parameter)
                {
                    var combiner = new DateTimeOffsetTimeFormatValueCombiner(TimeZoneInfo.Utc);
                    var successful = tryGetValue(combiner, DateTimeOffset.UtcNow, parameter, out var convertedValue);

                    successful.Should().BeFalse();
                    convertedValue.Should().Be(MvxBindingConstant.UnsetValue);
                }
            }

            public sealed class ValidFormats
            {
                [Theory, LogIfTooSlow]
                [MemberData(nameof(TimeFormats))]
                public void WorksForValidFormats(TimeFormat format)
                {
                    var time = DateTimeOffset.UtcNow;
                    var combiner = new DateTimeOffsetTimeFormatValueCombiner(TimeZoneInfo.Utc);
                    var successful = tryGetValue(combiner, time, format, out var convertedValue);

                    successful.Should().BeTrue();
                    convertedValue.Should().BeOfType<string>();
                    convertedValue.Should().Be(time.ToString(format.Format, CultureInfo.InvariantCulture));
                }

                public static IEnumerable<object[]> TimeFormats()
                    => new List<object[]>
                    {
                        new object[] { TimeFormat.FromLocalizedTimeFormat("h:mm A") },
                        new object[] { TimeFormat.FromLocalizedTimeFormat("H:mm") }
                    };
            }
        }
    }
}
