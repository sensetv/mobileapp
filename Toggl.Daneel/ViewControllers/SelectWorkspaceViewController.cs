using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Toggl.Daneel.Extensions;
using Toggl.Daneel.Extensions.Reactive;
using Toggl.Daneel.Presentation.Attributes;
using Toggl.Daneel.Views;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.Converters;
using Toggl.Foundation.MvvmCross.Helper;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Multivac.Extensions;
using UIKit;

namespace Toggl.Daneel.ViewControllers
{
    [ModalCardPresentation]
    public partial class SelectWorkspaceViewController : ReactiveViewController<SelectWorkspaceViewModel>, IDismissableViewController
    {
        private WorkspaceTableViewSource tableViewSource = new WorkspaceTableViewSource();

        public SelectWorkspaceViewController() 
            : base(nameof(SelectWorkspaceViewController))
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            WorkspaceTableView.SeparatorStyle = UITableViewCellSeparatorStyle.None;
            WorkspaceTableView.RegisterNibForCellReuse(WorkspaceViewCell.Nib, WorkspaceViewCell.Identifier);
            WorkspaceTableView.Source = tableViewSource;

            TitleLabel.Text = ViewModel.Title;

            CloseButton.Rx()
                .BindAction(ViewModel.Close)
                .DisposedBy(DisposeBag);

            tableViewSource.WorkspaceSelected
                .Subscribe(ViewModel.SelectWorkspace.Inputs)
                .DisposedBy(DisposeBag);

            replaceWorkspaces(ViewModel.Workspaces);
        }

        public async Task<bool> Dismiss()
        {
            await ViewModel.Close.Execute();
            return true;
        }

        private void replaceWorkspaces(IReadOnlyCollection<SelectableWorkspaceViewModel> workspaces)
        {
            tableViewSource.SetNewWorkspaces(workspaces);
            WorkspaceTableView.ReloadData();
        }
    }
}

