using System;
using System.Linq;
using System.Reactive.Disposables;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Adapters;
using Toggl.Giskard.Extensions;
using Toggl.Multivac.Extensions;

namespace Toggl.Giskard.Fragments
{
    [MvxDialogFragmentPresentation(AddToBackStack = true)]
    public sealed partial class SelectDurationFormatFragment : MvxDialogFragment<SelectDurationFormatViewModel>
    {
        private readonly CompositeDisposable disposeBag  = new CompositeDisposable();

        public SelectDurationFormatFragment() { }

        public SelectDurationFormatFragment(IntPtr javaReference, JniHandleOwnership transfer)
            : base (javaReference, transfer) { }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            var view = inflater.Inflate(Resource.Layout.SelectDurationFormatFragment, null);

            initializeViews(view);

            recyclerView.SetLayoutManager(new LinearLayoutManager(Context));
            selectDurationRecyclerAdapter = new SelectDurationFormatRecyclerAdapter();
            selectDurationRecyclerAdapter.Items = ViewModel.DurationFormats.ToList();
            recyclerView.SetAdapter(selectDurationRecyclerAdapter);

            selectDurationRecyclerAdapter.ItemTapObservable
                .Subscribe(ViewModel.SelectDurationFormat.Inputs)
                .DisposedBy(disposeBag);

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();

            Dialog.Window.SetDefaultDialogLayout(Activity, Context, heightDp: 268);
        }

        public override void OnCancel(IDialogInterface dialog)
        {
            ViewModel.Close.Execute();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing) return;
            disposeBag.Dispose();
        }
    }
}
