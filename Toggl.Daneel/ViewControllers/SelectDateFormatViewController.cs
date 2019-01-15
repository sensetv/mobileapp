using System.Collections.Immutable;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using MvvmCross.Platforms.Ios.Views;
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
    public sealed partial class SelectDateFormatViewController
        : MvxViewController<SelectDateFormatViewModel>,
          IDismissableViewController
    {
        private readonly CompositeDisposable disposeBag = new CompositeDisposable();

        public SelectDateFormatViewController() : base(nameof(SelectDateFormatViewController), null)
        {
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            TitleLabel.Text = Resources.DateFormat;

            var source = new DateFormatsTableViewSource(DateFormatsTableView, ViewModel.DateTimeFormats);
            
            DateFormatsTableView.Source = source;

            source.DateFormatSelected
                .Subscribe(ViewModel.SelectDateFormat.Inputs)
                .DisposedBy(disposeBag);

            BackButton.Rx()
                .BindAction(ViewModel.Close)
                .DisposedBy(disposeBag);
        }

        public async Task<bool> Dismiss()
        {
            ViewModel.Close.Execute();
            return true;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            disposeBag.Dispose();
        }
    }
}

