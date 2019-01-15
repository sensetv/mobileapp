using Android.Support.V7.Widget;
using Android.Views;

namespace Toggl.Giskard.Fragments
{
    public partial class SelectDefaultWorkspaceFragment
    {
        private RecyclerView recyclerView;

        public void InitializeViews(View rootView)
        {
            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.SelectWorkspaceRecyclerView);
        }
    }
}
