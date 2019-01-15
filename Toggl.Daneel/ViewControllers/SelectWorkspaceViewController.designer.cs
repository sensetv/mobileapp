// WARNING
//
// This file has been generated automatically by Visual Studio to store outlets and
// actions made in the UI designer. If it is removed, they will be lost.
// Manual changes to this file may not be handled correctly.
//
using Foundation;
using System.CodeDom.Compiler;

namespace Toggl.Daneel.ViewControllers
{
    [Register ("SelectWorkspaceViewController")]
    partial class SelectWorkspaceViewController
    {
        [Outlet]
        UIKit.UIButton CloseButton { get; set; }

        [Outlet]
        UIKit.UILabel TitleLabel { get; set; }

        [Outlet]
        UIKit.UITableView WorkspaceTableView { get; set; }
        
        void ReleaseDesignerOutlets ()
        {
            if (CloseButton != null) {
                CloseButton.Dispose ();
                CloseButton = null;
            }

            if (WorkspaceTableView != null) {
                WorkspaceTableView.Dispose ();
                WorkspaceTableView = null;
            }

            if (TitleLabel != null) {
                TitleLabel.Dispose ();
                TitleLabel = null;
            }
        }
    }
}
