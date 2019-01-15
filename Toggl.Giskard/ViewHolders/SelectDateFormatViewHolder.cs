using System;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Toggl.Foundation.MvvmCross.ViewModels.Selectable;

namespace Toggl.Giskard.ViewHolders
{
    public class SelectDateFormatViewHolder : BaseRecyclerViewHolder<SelectableDateFormatViewModel>
    {
        private TextView dateFormatTextView;
        private RadioButton selectedButton;

        public SelectDateFormatViewHolder(View itemView) : base(itemView)
        {
        }

        public SelectDateFormatViewHolder(IntPtr handle, JniHandleOwnership ownership) : base(handle, ownership)
        {
        }

        protected override void InitializeViews()
        {
            dateFormatTextView = ItemView.FindViewById<TextView>(Resource.Id.SelectableDateFormatTextView);
            selectedButton = ItemView.FindViewById<RadioButton>(Resource.Id.SelectableDateFormatRadioButton);
        }

        protected override void UpdateView()
        {
            dateFormatTextView.Text = Item.DateFormat.Localized;
            selectedButton.Checked = Item.Selected;
        }
    }
}
