using Xamarin.UITest;

namespace Toggl.Tests.UI.Extensions
{
    public static partial class MainExtensions
    {
        public static void OpenEditView(this IApp app)
        {
            app.Tap("MainLogContentView");
        }
    }
}
