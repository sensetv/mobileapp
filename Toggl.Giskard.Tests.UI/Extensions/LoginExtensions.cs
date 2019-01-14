using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Toggl.Tests.UI.Extensions
{
    public static partial class LoginExtensions
    {
        public static void WaitForLoginScreen(this IApp app)
        {
            //Giskard doesn't have the onboarding screen
        }

        public static void CheckThatLoginButtonIsDisabled(this IApp app, AppResult button)
        {
            var isButtonDisabled = !button.Enabled;
            Assert.AreEqual(true, isButtonDisabled);
        }
    }
}
