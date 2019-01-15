using System;
using Android.Support.V7.Widget;
using Android.Views;

namespace Toggl.Giskard.Fragments
{
	public sealed partial class SelectDateFormatFragment
	{
        private RecyclerView recyclerView;

        private void initializeViews(View rootView)
        {
            recyclerView = rootView.FindViewById<RecyclerView>(Resource.Id.SelectDateFormatRecyclerView);
        }
    }
}
