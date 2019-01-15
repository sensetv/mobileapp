using System;
using Android.Views;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.ViewHolders;

namespace Toggl.Giskard.Adapters
{
    public class SelectDurationFormatRecyclerAdapter : BaseRecyclerAdapter<SelectableDurationFormatViewModel>
    {
        private const int selectableDurationFormatViewType = 1;

        public override int GetItemViewType(int position)
            => selectableDurationFormatViewType;

        protected override BaseRecyclerViewHolder<SelectableDurationFormatViewModel> CreateViewHolder(ViewGroup parent, LayoutInflater inflater, int viewType)
        {
            switch (viewType)
            {
                case selectableDurationFormatViewType:
                    var inflatedView = inflater.Inflate(Resource.Layout.SelectDurationFormatFragmentCell, parent, false);
                    return new SelectDurationFormatViewHolder(inflatedView);
                default:
                    throw new Exception("Unsupported view type");
            }
        }
    }
}
