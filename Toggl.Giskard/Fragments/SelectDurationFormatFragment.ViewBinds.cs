using Android.Support.V7.Widget;
using Android.Views;
using Toggl.Giskard.Adapters;

namespace Toggl.Giskard.Fragments
{
    public partial class SelectDurationFormatFragment
    {
        private RecyclerView recyclerView;
        private SelectDurationFormatRecyclerAdapter selectDurationRecyclerAdapter;

        private void initializeViews(View view)
        {
            recyclerView = view.FindViewById<RecyclerView>(Resource.Id.SelectDurationFormatRecyclerView);
        }
    }
}
