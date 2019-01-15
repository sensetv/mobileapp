using System.Reactive.Disposables;
using System.Threading.Tasks;
using Toggl.Daneel.Extensions;
using Toggl.Daneel.Extensions.Reactive;
using Toggl.Daneel.Presentation.Attributes;
using Toggl.Daneel.ViewSources;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Multivac.Extensions;

namespace Toggl.Daneel.ViewControllers
{
    [ModalCardPresentation]
    public partial class SelectDurationFormatViewController : ReactiveViewController<SelectDurationFormatViewModel>, IDismissableViewController
    {
        CompositeDisposable disposeBag = new CompositeDisposable();

        public SelectDurationFormatViewController()
            : base(nameof(SelectDurationFormatViewController))
        {
        }

        public async Task<bool> Dismiss()
        {
            ViewModel.Close.Execute();
            return true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TitleLabel.Text = Resources.DurationFormat;

            var source = new DurationFormatsTableViewSource(DurationFormatsTableView, ViewModel.DurationFormats);
            
            DurationFormatsTableView.Source = source;

            BackButton.Rx()
                .BindAction(ViewModel.Close)
                .DisposedBy(disposeBag);

            source.DurationFormatSelected
                .Subscribe(ViewModel.SelectDurationFormat.Inputs)
                .DisposedBy(disposeBag);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            disposeBag.Dispose();
        }
    }
}
