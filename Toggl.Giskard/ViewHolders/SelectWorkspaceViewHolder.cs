using System;
using Android.Graphics;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Toggl.Foundation.MvvmCross.ViewModels;

namespace Toggl.Giskard.ViewHolders
{
    public class SelectWorkspaceViewHolder : BaseRecyclerViewHolder<SelectableWorkspaceViewModel>
    {
        public static SelectWorkspaceViewHolder Create(View itemView)
            => new SelectWorkspaceViewHolder(itemView);

        private TextView workspaceNameTextView;

        public SelectWorkspaceViewHolder(View itemView)
            : base(itemView)
        {
        }

        public SelectWorkspaceViewHolder(IntPtr handle, JniHandleOwnership ownership)
            : base(handle, ownership)
        {
        }

        protected override void InitializeViews()
        {
            workspaceNameTextView = ItemView.FindViewById<TextView>(Resource.Id.SelectWorkspaceFragmentCellTextView);
        }

        protected override void UpdateView()
        {
            workspaceNameTextView.Text = Item.WorkspaceName;
            workspaceNameTextView.SetTypeface(
                workspaceNameTextView.Typeface,
                Item.Selected ? TypefaceStyle.Bold : TypefaceStyle.Normal);
        }
    }
}