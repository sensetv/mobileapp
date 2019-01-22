using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Linq;
using Foundation;
using Toggl.Daneel.Views;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class WorkspaceTableViewSource : ListTableViewSource<SelectableWorkspaceViewModel, WorkspaceViewCell>
    {
        private const int rowHeight = 64;

        public IObservable<SelectableWorkspaceViewModel> WorkspaceSelected
            => Observable
                .FromEventPattern<SelectableWorkspaceViewModel>(e => OnItemTapped += e, e => OnItemTapped -= e)
                .Select(e => e.EventArgs);

        public WorkspaceTableViewSource()
            : base(ImmutableArray<SelectableWorkspaceViewModel>.Empty, WorkspaceViewCell.Identifier)
        {
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => rowHeight;

        public void SetNewWorkspaces(IEnumerable<SelectableWorkspaceViewModel> workspaces)
        {
            items = workspaces.ToImmutableList();
        }
    }
}