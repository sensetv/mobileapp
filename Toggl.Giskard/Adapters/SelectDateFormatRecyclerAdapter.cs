using System;
using Android.Views;
using Toggl.Foundation.MvvmCross.ViewModels.Selectable;
using Toggl.Giskard.ViewHolders;

namespace Toggl.Giskard.Adapters
{
    public sealed class SelectDateFormatRecyclerAdapter : BaseRecyclerAdapter<SelectableDateFormatViewModel>
    {
        private const int selectableDateFormatViewType = 1;

        public override int GetItemViewType(int position)
            => selectableDateFormatViewType;

        protected override BaseRecyclerViewHolder<SelectableDateFormatViewModel> CreateViewHolder(ViewGroup parent, LayoutInflater inflater, int viewType)
        {
            switch (viewType)
            {
                case selectableDateFormatViewType:
                    var inflatedView = inflater.Inflate(Resource.Layout.SelectDateFormatFragmentCell, parent, false);
                    return new SelectDateFormatViewHolder(inflatedView);
                default:
                    throw new Exception("unsupported view type");
            }
        }
    }
}
