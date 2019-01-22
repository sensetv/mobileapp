using System;
using Xamarin.UITest;

namespace Toggl.Tests.UI.Extensions
{
    public static partial class MainExtensions
    {
        public static void WaitForMainScreen(this IApp app)
        {
            var email = $"{Guid.NewGuid().ToString()}@toggl.space";

            app.WaitForSignUpScreen();

            app.Tap(SignUp.EmailText);
            app.EnterText(email);
            app.Tap(SignUp.PasswordText);
            app.EnterText("123456");
            app.SignUpSuccesfully();
        }

        public static void StartTimeEntryWithDescription(this IApp app, string description)
        {
            app.Tap(Main.StartTimeEntryButton);
            app.WaitForElement(StartTimeEntry.DoneButton);

            app.EnterText(description);
            app.Tap(StartTimeEntry.DoneButton);

            app.WaitForElement(Main.StopTimeEntryButton);
        }

        public static void PullToRefresh(this IApp app)
        {
            app.WaitForNoElement(query => query.Text("Synced"));
            app.ScrollUp(Main.TimeEntriesCollection, ScrollStrategy.Gesture);
            app.WaitForNoElement(query => query.Text("Synced"));
        }
    }
}
