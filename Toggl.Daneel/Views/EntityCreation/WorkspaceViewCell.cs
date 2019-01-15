using System;
using Foundation;
using MvvmCross.Binding.BindingContext;
using MvvmCross.Platforms.Ios.Binding;
using MvvmCross.Platforms.Ios.Binding.Views;
using Toggl.Daneel.Cells;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.PrimeRadiant.Models;
using UIKit;

namespace Toggl.Daneel.Views
{
    public partial class WorkspaceViewCell : BaseTableViewCell<SelectableWorkspaceViewModel>
    {
        public static readonly string Identifier = nameof(WorkspaceViewCell);
        public static readonly UINib Nib;

        static WorkspaceViewCell()
        {
            Nib = UINib.FromName(nameof(WorkspaceViewCell), NSBundle.MainBundle);
        }

        protected WorkspaceViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        public override void AwakeFromNib()
        {
            base.AwakeFromNib();
            SelectionStyle = UITableViewCellSelectionStyle.None;
        }

        protected override void UpdateView()
        {
            NameLabel.Text = Item.WorkspaceName;
            SelectedImage.Hidden = !Item.Selected;
        }
    }
}
