using System;
using System.Reactive;
using System.Reactive.Subjects;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Text;
using Android.Text.Style;
using Android.Views;
using Android.Widget;
using MvvmCross.Plugin.Color.Platforms.Android;
using Toggl.Foundation.Extensions;
using Toggl.Giskard.Extensions;
using Toggl.Giskard.ViewHelpers;
using Toggl.Giskard.Views;
using Toggl.Multivac;
using static Toggl.Foundation.MvvmCross.Helper.Color.Reports;
using Color = Android.Graphics.Color;

namespace Toggl.Giskard.ViewHolders
{
    public class ReportsHeaderCellViewHolder : BaseRecyclerViewHolder<ReportsSummaryData>
    {

        private static readonly Color disabledColor = Disabled.ToNativeColor();
        private static readonly Color timeSpanNormalColor = TotalTimeActivated.ToNativeColor();
        private static readonly Color percentageNormalColor = PercentageActivated.ToNativeColor();

        private CardView summaryCard;
        private TextView reportsSummaryTotal;
        private ImageView reportsTotalChartImageView;
        private TextView reportsSummaryBillable;
        private View billablePercentageView;

        private CardView pieChartCard;
        private PieChartView pieChartView;
        private LinearLayout emptyStateView;

        private Drawable reportsTotalChartImageDrawable;

        public ISubject<Unit> SummaryCardClicksSubject { get; set; }

        public ReportsHeaderCellViewHolder(View itemView) : base(itemView)
        {
        }

        public ReportsHeaderCellViewHolder(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
        {

        }

        protected override void InitializeViews()
        {
            summaryCard = ItemView.FindViewById<CardView>(Resource.Id.SummaryCard);
            reportsSummaryTotal = ItemView.FindViewById<TextView>(Resource.Id.ReportsSummaryTotal);
            reportsTotalChartImageView = ItemView.FindViewById<ImageView>(Resource.Id.ReportsTotalChartImageView);
            reportsTotalChartImageDrawable = reportsTotalChartImageView.Drawable;
            reportsSummaryBillable = ItemView.FindViewById<TextView>(Resource.Id.ReportsSummaryBillable);
            billablePercentageView = ItemView.FindViewById(Resource.Id.BillablePercentageView);
            pieChartCard = ItemView.FindViewById<CardView>(Resource.Id.PieChartCard);
            pieChartView = ItemView.FindViewById<PieChartView>(Resource.Id.PieChartView);
            emptyStateView = ItemView.FindViewById<LinearLayout>(Resource.Id.EmptyStateView);

            summaryCard.Click += hideCalendar;
        }

        protected override void UpdateView()
        {
            reportsSummaryTotal.TextFormatted = convertReportTimeSpanToDurationString(Item.TotalTime, Item.DurationFormat);
            reportsTotalChartImageDrawable.SetColorFilter(Item.TotalTimeIsZero ? Disabled.ToNativeColor() : TotalTimeActivated.ToNativeColor(), PorterDuff.Mode.SrcIn);
            reportsSummaryBillable.TextFormatted = convertBillablePercentageToSpannable(Item.BillablePercentage);
            updateBillablePercentageViewWidth();
            pieChartCard.Visibility = (!Item.ShowEmptyState).ToVisibility();
            pieChartView.Segments = Item.Segments;
            emptyStateView.Visibility = Item.ShowEmptyState.ToVisibility();
        }

        private void updateBillablePercentageViewWidth()
        {
            billablePercentageView.Post(() =>
            {
                if (billablePercentageView.Parent == null) return;

                var percentage = Item.BillablePercentage;
                var availableWidth = ((View) billablePercentageView.Parent).Width;
                var targetWidth = (availableWidth / 100.0f) * percentage;

                var layoutParams = billablePercentageView.LayoutParameters;
                layoutParams.Width = (int) targetWidth;
                billablePercentageView.LayoutParameters = layoutParams;
            });
        }

        public ISpannable convertReportTimeSpanToDurationString(TimeSpan timeSpan, DurationFormat durationFormat)
        {
            var timeString = timeSpan.ToFormattedString(durationFormat);

            var emphasizedPartLength = durationFormat == DurationFormat.Improved 
                ? timeString.Length - 3
                : timeString.Length;

            var isDisabled = timeSpan.Ticks == 0;

            return selectTimeSpanEnabledStateSpan(timeString, emphasizedPartLength, isDisabled);
        }

        private ISpannable selectTimeSpanEnabledStateSpan(string timeString, int emphasizedPartLength, bool isDisabled)
        {
            var color = isDisabled ? disabledColor : timeSpanNormalColor;

            var spannable = new SpannableString(timeString);
            spannable.SetSpan(new TypefaceSpan("sans-serif"), 0, emphasizedPartLength, SpanTypes.InclusiveInclusive);
            if (emphasizedPartLength < timeString.Length)
            {
                spannable.SetSpan(new TypefaceSpan("sans-serif-light"), emphasizedPartLength, timeString.Length, SpanTypes.InclusiveInclusive);
            }
            spannable.SetSpan(new ForegroundColorSpan(color), 0, spannable.Length(), SpanTypes.InclusiveInclusive);

            return spannable;
        }

        public ISpannable convertBillablePercentageToSpannable(float? value)
        {
            var isDisabled = value == null;
            var actualValue = isDisabled ? 0 : value.Value;

            var percentage = $"{actualValue:0.00}%";

            return selectBillablePercentageEnabledStateSpan(percentage, isDisabled);
        }

        private ISpannable selectBillablePercentageEnabledStateSpan(string percentage, bool isDisabled)
        {
            var color = isDisabled ? disabledColor : percentageNormalColor;
            var spannable = new SpannableString(percentage);
            spannable.SetSpan(new ForegroundColorSpan(color), 0, spannable.Length(), SpanTypes.ExclusiveExclusive);

            return spannable;
        }

        private void hideCalendar(object sender, EventArgs args)
        {
            SummaryCardClicksSubject?.OnNext(Unit.Default);
        }
    }
}
