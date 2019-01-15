using System;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Toggl.Foundation.MvvmCross.Transformations;
using Toggl.Foundation.MvvmCross.ViewModels;

namespace Toggl.Giskard.ViewHolders
{
    public class SelectDurationFormatViewHolder : BaseRecyclerViewHolder<SelectableDurationFormatViewModel>
    {
        private TextView durationFormatTextView;
        private RadioButton selectedButton;

        public SelectDurationFormatViewHolder(View itemView) : base(itemView)
        {
        }

        public SelectDurationFormatViewHolder(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override void InitializeViews()
        {
            durationFormatTextView = ItemView.FindViewById<TextView>(Resource.Id.SelectableDurationFormatTextView);
            selectedButton = ItemView.FindViewById<RadioButton>(Resource.Id.SelectableDurationFormatRadioButton);
        }

        protected override void UpdateView()
        {
            durationFormatTextView.Text = DurationFormatToString.Convert(Item.DurationFormat);
            selectedButton.Checked = Item.Selected;
        }
    }
}
