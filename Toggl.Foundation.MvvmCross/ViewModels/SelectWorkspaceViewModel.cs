using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using MvvmCross.Navigation;
using MvvmCross.ViewModels;
using Toggl.Foundation.Interactors;
using Toggl.Foundation.Services;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;

namespace Toggl.Foundation.MvvmCross.ViewModels
{
    [Preserve(AllMembers = true)]
    public sealed class SelectWorkspaceViewModel : MvxViewModel<long, long>
    {
        private readonly IInteractorFactory interactorFactory;
        private readonly IMvxNavigationService navigationService;

        private long currentWorkspaceId;

        public string Title { get; } = Resources.SetDefaultWorkspace;
        public ReadOnlyCollection<SelectableWorkspaceViewModel> Workspaces { get; private set; }

        public UIAction Close { get; }
        public InputAction<SelectableWorkspaceViewModel> SelectWorkspace { get; }

        public SelectWorkspaceViewModel(
            IInteractorFactory interactorFactory,
            IMvxNavigationService navigationService,
            IRxActionFactory rxActionFactory)
        {
            Ensure.Argument.IsNotNull(interactorFactory, nameof(interactorFactory));
            Ensure.Argument.IsNotNull(navigationService, nameof(navigationService));
            Ensure.Argument.IsNotNull(rxActionFactory, nameof(rxActionFactory));

            this.interactorFactory = interactorFactory;
            this.navigationService = navigationService;

            Close = rxActionFactory.FromAsync(close);
            SelectWorkspace = rxActionFactory.FromAsync<SelectableWorkspaceViewModel>(selectWorkspace);
        }

        public override void Prepare(long currentWorkspaceId)
        {
            this.currentWorkspaceId = currentWorkspaceId;
        }

        public override async Task Initialize()
        {
            await base.Initialize();

            var workspaces = await interactorFactory.GetAllWorkspaces().Execute();

            Workspaces = workspaces
                .Where(w => w.IsEligibleForProjectCreation())
                .Select(w => new SelectableWorkspaceViewModel(w, w.Id == currentWorkspaceId))
                .ToList()
                .AsReadOnly();
        }

        private Task close()
            => navigationService.Close(this, currentWorkspaceId);

        private Task selectWorkspace(SelectableWorkspaceViewModel workspace)
            => navigationService.Close(this, workspace.WorkspaceId);
    }
}
