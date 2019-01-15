using System.Reactive.Disposables;
using Android.OS;
using Android.Support.V7.Widget;
using Android.Views;
using MvvmCross.Droid.Support.V4;
using MvvmCross.Platforms.Android.Binding.BindingContext;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Adapters;
using Toggl.Giskard.ViewHolders;
using Toggl.Multivac.Extensions;

namespace Toggl.Giskard.Fragments
{
    [MvxFragmentPresentation(typeof(EditProjectViewModel), Resource.Id.SelectWorkspaceContainer, AddToBackStack = true)]
    public partial class SelectWorkspaceFragment : MvxFragment<SelectWorkspaceViewModel>
    {
        public CompositeDisposable DisposeBag { get; } = new CompositeDisposable();

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            base.OnCreateView(inflater, container, savedInstanceState);
            var view = inflater.Inflate(Resource.Layout.SelectWorkspaceFragment, container, false);
            InitializeViews(view);

            var adapter = new SimpleAdapter<SelectableWorkspaceViewModel>(
                Resource.Layout.SelectWorkspaceFragmentCell,
                SelectWorkspaceViewHolder.Create
            );

            adapter.ItemTapObservable
                .Subscribe(ViewModel.SelectWorkspace.Inputs)
                .DisposedBy(DisposeBag);

            adapter.Items = ViewModel.Workspaces;

            recyclerView.SetAdapter(adapter);
            recyclerView.SetLayoutManager(new LinearLayoutManager(Context));

            return view;
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            DisposeBag.Dispose();
        }
    }
}
