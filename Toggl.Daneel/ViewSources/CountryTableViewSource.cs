using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Linq;
using Foundation;
using MvvmCross.Platforms.Ios.Binding.Views;
using Toggl.Daneel.Views.CountrySelection;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.ViewSources
{
    public sealed class CountryTableViewSource : ListTableViewSource<SelectableCountryViewModel, CountryViewCell>
    {
        public IObservable<SelectableCountryViewModel> CountrySelected
            => Observable
                .FromEventPattern<SelectableCountryViewModel>(e => OnItemTapped += e, e => OnItemTapped -= e)
                .Select(e => e.EventArgs);

        private const string cellIdentifier = nameof(CountryViewCell);
        private const int rowHeight = 48;

        public CountryTableViewSource()
            : base(ImmutableArray<SelectableCountryViewModel>.Empty, cellIdentifier)
        {
        }

        public void SetNewCountries(IEnumerable<SelectableCountryViewModel> countries)
        {
            items = countries.ToImmutableList();
        }

        public override nfloat GetHeightForRow(UITableView tableView, NSIndexPath indexPath) => rowHeight;
    }
}
