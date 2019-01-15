using System;
using System.Collections.Immutable;
using System.Reactive.Linq;
using Foundation;
using Toggl.Daneel.Views.Settings;
using Toggl.Foundation.MvvmCross.ViewModels.Selectable;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class DateFormatsTableViewSource : ListTableViewSource<SelectableDateFormatViewModel, DateFormatViewCell>
    {
        private const string cellIdentifier = nameof(DateFormatViewCell);
        private const int rowHeight = 48;

        public IObservable<SelectableDateFormatViewModel> DateFormatSelected
            => Observable
                .FromEventPattern<SelectableDateFormatViewModel>(e => OnItemTapped += e, e => OnItemTapped -= e)
                .Select(e => e.EventArgs);

        public DateFormatsTableViewSource(UITableView tableView, IImmutableList<SelectableDateFormatViewModel> items)
            : base(items, cellIdentifier)
        {
            tableView.RegisterNibForCellReuse(DateFormatViewCell.Nib, cellIdentifier);
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => rowHeight;
    }
}
