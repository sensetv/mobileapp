using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Toggl.Foundation.MvvmCross.ViewModels.Selectable;
using Toggl.Foundation.Services;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectDateFormatViewModel : MvxViewModel<DateFormat, DateFormat>
    {
        private readonly IMvxNavigationService navigationService;
        private readonly DateFormat[] availableDateFormats =
        {
            DateFormat.FromLocalizedDateFormat("MM/DD/YYYY"),
            DateFormat.FromLocalizedDateFormat("DD-MM-YYYY"),
            DateFormat.FromLocalizedDateFormat("MM-DD-YYYY"),
            DateFormat.FromLocalizedDateFormat("YYYY-MM-DD"),
            DateFormat.FromLocalizedDateFormat("DD/MM/YYYY"),
            DateFormat.FromLocalizedDateFormat("DD.MM.YYYY")
        };

        private DateFormat defaultResult;

        public ImmutableList<SelectableDateFormatViewModel> DateTimeFormats { get; }

        public UIAction Close { get; }

        public InputAction<SelectableDateFormatViewModel> SelectDateFormat { get; }

        public SelectDateFormatViewModel(IMvxNavigationService navigationService, IRxActionFactory rxActionFactory)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(rxActionFactory, nameof(rxActionFactory));

            this.navigationService = navigationService;

            Close = rxActionFactory.FromAsync(close);
            SelectDateFormat = rxActionFactory.FromAsync<SelectableDateFormatViewModel>(selectFormat);

            DateTimeFormats = availableDateFormats
                .Select(dateFormat => new SelectableDateFormatViewModel(dateFormat, false))
                .ToImmutableList();
        }

        public override void Prepare(DateFormat parameter)
        {
            defaultResult = parameter;

            updateSelectedFormat(parameter);
        }

        private Task close() => navigationService.Close(this, defaultResult);

        private Task selectFormat(SelectableDateFormatViewModel dateFormatViewModel)
            => navigationService.Close(this, dateFormatViewModel.DateFormat);

        private void updateSelectedFormat(DateFormat selected)
            => DateTimeFormats.ForEach(dateFormat
                => dateFormat.Selected = dateFormat.DateFormat == selected);
    }
}
