using System;
using NUnit.Framework;
using Xamarin.UITest;
using Toggl.Tests.UI.Extensions;
using System.Threading.Tasks;
using Toggl.Tests.UI.Helpers;

namespace Toggl.Tests.UI
{
    [TestFixture]
    public sealed class SignUpTests
    {
        private IApp app;

        [SetUp]
        public void BeforeEachTest()
        {
            app = Configuration.GetApp();

            app.WaitForSignUpScreen();
        }

        [Test]
        public void SigningUpWithCorrectCredentialsLeadsToTheMainScreen()
        {
            var email = randomEmail();
            var password = "qwerty123";

            app.Tap(SignUp.EmailText);
            app.EnterText(email);
            app.Tap(SignUp.PasswordText);
            app.EnterText(password);

            app.WaitForDefaultCountryToBeAutoSelected();
            app.Tap(SignUp.SignUpButton);
            app.Tap(SignUp.GdprButton);

            app.WaitForElement(Main.StartTimeEntryButton);
        }

        [Test]
        public async Task SigningUpWithExistingAccountShowsTheErrorLabel()
        {
            var email = randomEmail();
            var password = await User.Create(email);

            app.Tap(SignUp.EmailText);
            app.EnterText(email);
            app.Tap(SignUp.PasswordText);
            app.EnterText(password);

            app.WaitForDefaultCountryToBeAutoSelected();
            app.Tap(SignUp.SignUpButton);
            app.Tap(SignUp.GdprButton);

            app.WaitForElement(SignUp.ErrorLabel);
        }

        [Test]
        public void RejectingTermsAndConditionsCancelsSignUp()
        {
            var email = randomEmail();
            var password = "asdads";

            app.Tap(SignUp.EmailText);
            app.EnterText(email);
            app.Tap(SignUp.PasswordText);
            app.EnterText(password);

            app.WaitForDefaultCountryToBeAutoSelected();
            app.Tap(SignUp.SignUpButton);
            app.RejectTerms();

            app.WaitForNoElement(Main.StartTimeEntryButton);
        }

        private string randomEmail()
            => $"{Guid.NewGuid().ToString()}@toggl.space";
    }
}
