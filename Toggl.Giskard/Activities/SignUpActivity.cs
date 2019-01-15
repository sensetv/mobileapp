using System;
using System.Reactive.Linq;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Views;
using MvvmCross.Platforms.Android.Binding.Views;
using MvvmCross.Platforms.Android.Presenters.Attributes;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Extensions;
using Toggl.Giskard.Extensions.Reactive;
using Toggl.Multivac;
using Toggl.Multivac.Extensions;

namespace Toggl.Giskard.Activities
{
    [MvxActivityPresentation]
    [Activity(Theme = "@style/AppTheme.WhiteStatusBar",
              ScreenOrientation = ScreenOrientation.Portrait,
              WindowSoftInputMode = SoftInput.AdjustPan | SoftInput.StateHidden,
              ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    public sealed partial class SignUpActivity : ReactiveActivity<SignupViewModel>
    {
        protected override void OnCreate(Bundle bundle)
        {
            this.ChangeStatusBarColor(Android.Graphics.Color.White, true);

            base.OnCreate(bundle);
            SetContentView(Resource.Layout.SignUpActivity);
            OverridePendingTransition(Resource.Animation.abc_slide_in_bottom, Resource.Animation.abc_fade_out);

            InitializeViews();

            emailEditText.Text = ViewModel.Email.FirstAsync().GetAwaiter().GetResult();
            passwordEditText.Text = ViewModel.Password.FirstAsync().GetAwaiter().GetResult();

            //Text
            ViewModel.ErrorMessage
                .Subscribe(errorTextView.Rx().TextObserver())
                .DisposedBy(DisposeBag);

            emailEditText.Rx().Text()
                .Select(Email.From)
                .Subscribe(ViewModel.SetEmail)
                .DisposedBy(DisposeBag);

            passwordEditText.Rx().Text()
                .Select(Password.From)
                .Subscribe(ViewModel.SetPassword)
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Select(signupButtonTitle)
                .Subscribe(signupButton.Rx().TextObserver())
                .DisposedBy(DisposeBag);

            ViewModel.CountryButtonTitle
                .Subscribe(countryNameTextView.Rx().TextObserver())
                .DisposedBy(DisposeBag);

            //Visibility
            ViewModel.HasError
                .Subscribe(errorTextView.Rx().IsVisible(useGone: false))
                .DisposedBy(DisposeBag);

            ViewModel.IsLoading
                .Subscribe(progressBar.Rx().IsVisible(useGone: false))
                .DisposedBy(DisposeBag);

            ViewModel.SignupEnabled
                .Subscribe(signupButton.Rx().Enabled())
                .DisposedBy(DisposeBag);

            ViewModel.IsCountryErrorVisible
                .Subscribe(countryErrorView.Rx().IsVisible(useGone: false))
                .DisposedBy(DisposeBag);

            //Commands
            loginCard.Rx().Tap()
                .Subscribe(ViewModel.Login.Inputs)
                .DisposedBy(DisposeBag);

            signupButton.Rx().Tap()
                .Subscribe(ViewModel.Signup.Inputs)
                .DisposedBy(DisposeBag);

            passwordEditText.Rx().EditorActionSent()
                .Subscribe(ViewModel.Signup.Inputs)
                .DisposedBy(DisposeBag);

            googleSignupButton.Rx().Tap()
                .Subscribe(ViewModel.GoogleSignup.Inputs)
                .DisposedBy(DisposeBag);

            countrySelection.Rx().Tap()
                .Subscribe(ViewModel.PickCountry.Inputs)
                .DisposedBy(DisposeBag);

            string signupButtonTitle(bool isLoading)
                => isLoading ? "" : Resources.GetString(Resource.String.SignUpForFree);
        }

        protected override void AttachBaseContext(Context @base)
        {
            base.AttachBaseContext(MvxContextWrapper.Wrap(@base, this));
        }
    }
}
