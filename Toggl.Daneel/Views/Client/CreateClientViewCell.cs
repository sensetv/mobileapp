using System;
using Foundation;
using Toggl.Daneel.Cells;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross.ViewModels;
using UIKit;

namespace Toggl.Daneel.Views.Client
{
    public partial class CreateClientViewCell : BaseTableViewCell<SelectableClientBaseViewModel>
    {
        public static readonly string Identifier = nameof(CreateClientViewCell);
        public static readonly UINib Nib;

        static CreateClientViewCell()
        {
            Nib = UINib.FromName("CreateClientViewCell", NSBundle.MainBundle);
        }

        protected CreateClientViewCell(IntPtr handle) : base(handle)
        {
            // Note: this .ctor should not contain any initialization logic.
        }

        protected override void UpdateView()
        {
            TextLabel.Text = $"{Resources.CreateClient} \"{Item.Name.Trim()}\"";
        }
    }
}

