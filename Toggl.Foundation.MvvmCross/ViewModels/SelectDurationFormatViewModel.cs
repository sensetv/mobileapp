using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Toggl.Foundation.Services;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public class SelectDurationFormatViewModel : MvxViewModel<DurationFormat, DurationFormat>
    {
        private readonly IMvxNavigationService navigationService;

        private DurationFormat defaultResult;

        public IImmutableList<SelectableDurationFormatViewModel> DurationFormats { get; }

        public UIAction Close { get; }

        public InputAction<SelectableDurationFormatViewModel> SelectDurationFormat { get; }

        public SelectDurationFormatViewModel(
            IMvxNavigationService navigationService,
            IRxActionFactory rxActionFactory)
        {
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(rxActionFactory, nameof(rxActionFactory));

            this.navigationService = navigationService;

            Close = rxActionFactory.FromAsync(close);
            SelectDurationFormat = rxActionFactory.FromAsync<SelectableDurationFormatViewModel>(selectFormat);

            DurationFormats = Enum.GetValues(typeof(DurationFormat))
                            .Cast<DurationFormat>()
                            .Select(durationFormat => new SelectableDurationFormatViewModel(durationFormat, false))
                            .ToImmutableList();
        }

        public override void Prepare(DurationFormat parameter)
        {
            defaultResult = parameter;
            updateSelectedFormat(parameter);
        }

        private Task close() => navigationService.Close(this, defaultResult);

        private Task selectFormat(SelectableDurationFormatViewModel viewModel)
            => navigationService.Close(this, viewModel.DurationFormat);

        private void updateSelectedFormat(DurationFormat selected)
            => DurationFormats.ForEach(viewModel
                => viewModel.Selected = viewModel.DurationFormat == selected);
    }
}
