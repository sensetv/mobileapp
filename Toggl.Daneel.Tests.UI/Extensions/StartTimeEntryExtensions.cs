﻿using Xamarin.UITest;

namespace Toggl.Tests.UI.Extensions
{
    public static partial class StartTimeEntryExtensions
    {
        public static void TapCreateTag(this IApp app, string tagName)
        {
            app.TapNthCellInCollection(0);
        }

        public static void TapCreateProject(this IApp app, string projectName)
        {
            app.TapNthCellInCollection(0);
        }

        public static void TapSelectTag(this IApp app, string tagName)
        {
            app.TapNthCellInCollection(0);
        }

        public static void TapSelectProject(this IApp app, string projectName)
        {
            app.TapNthCellInCollection(0);
        }
    }
}