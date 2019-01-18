using Android.App;
using Android.Content.PM;
using Android.OS;
using MvvmCross;
using MvvmCross.Droid.Support.V7.AppCompat;
using MvvmCross.Navigation;
using Toggl.Foundation;
using Toggl.Foundation.MvvmCross;
using Toggl.Foundation.MvvmCross.ViewModels;
using Toggl.Giskard.Helper;
using static Android.Content.Intent;

namespace Toggl.Giskard
{
    [Activity(Label = "Toggl for Devs",
              MainLauncher = false,
              Icon = "@mipmap/ic_launcher",
              Theme = "@style/Theme.Splash",
              ScreenOrientation = ScreenOrientation.Portrait,
              ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
    [IntentFilter(
        new[] { "android.intent.action.VIEW", "android.intent.action.EDIT" },
        Categories = new[] { "android.intent.category.BROWSABLE", "android.intent.category.DEFAULT" },
        DataSchemes = new[] { "toggl" },
        DataHost = "*")]
    [IntentFilter(
        new[] { "android.intent.action.PROCESS_TEXT" },
        Categories = new[] { "android.intent.category.DEFAULT" },
        DataMimeType = "text/plain")]
    public class SplashScreen : MvxSplashScreenAppCompatActivity<Setup, App<LoginViewModel>>
    {
        public SplashScreen()
            : base(Resource.Layout.SplashScreen)
        {

        }

        protected override void RunAppStart(Bundle bundle)
        {
            base.RunAppStart(bundle);
            var navigationUrl = Intent.Data?.ToString() ?? getTrackUrlFromProcessedText();
            var navigationService = Mvx.Resolve<IMvxNavigationService>();
            if (string.IsNullOrEmpty(navigationUrl))
            {
                Finish();
                return;
            }

            navigationService.Navigate(navigationUrl).ContinueWith(_ =>
            {
                Finish();
            });
        }

        private string getTrackUrlFromProcessedText()
        {
            if (MarshmallowApis.AreNotAvailable)
                return null;

            var description = Intent.GetStringExtra(ExtraProcessText);
            if (string.IsNullOrWhiteSpace(description))
                return null;

            var applicationUrl = ApplicationUrls.Main.Track(description);
            return applicationUrl;
        }
    }
}
