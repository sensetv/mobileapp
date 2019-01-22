using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Foundation;
using Toggl.Daneel.Cells;
using Toggl.Daneel.Views.Client;
using Toggl.Foundation.MvvmCross.Collections;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class ClientTableViewSource : ListTableViewSource<SelectableClientBaseViewModel, ClientViewCell>
    {
        public IObservable<SelectableClientBaseViewModel> ClientSelected
            => Observable
                .FromEventPattern<SelectableClientBaseViewModel>(e => OnItemTapped += e, e => OnItemTapped -= e)
                .Select(e => e.EventArgs);

        private const int rowHeight = 48;

        public ClientTableViewSource()
            : base(ImmutableArray<SelectableClientBaseViewModel>.Empty, ClientViewCell.Identifier)
        {
        }

        public void SetNewClients(IEnumerable<SelectableClientBaseViewModel> clients)
        {
            items = clients.ToImmutableList();
        }

        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            var item = items[indexPath.Row];
            var identifier = item is SelectableClientCreationViewModel ? CreateClientViewCell.Identifier : cellIdentifier;
            var cell = tableView.DequeueReusableCell(identifier) as BaseTableViewCell<SelectableClientBaseViewModel>;
            cell.Item = item;
            return cell;
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => rowHeight;
    }
}