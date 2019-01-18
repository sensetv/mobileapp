using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;

namespace Toggl.Giskard.Activities
{
    [Activity(
        MainLauncher = true,
        Theme = "@style/AppTheme",
        ScreenOrientation = ScreenOrientation.Portrait,
        ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize
    )]
    public class WheelPlaygroundActivity : AppCompatActivity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.WheelPlaygroundActivity);
        }
    }
}