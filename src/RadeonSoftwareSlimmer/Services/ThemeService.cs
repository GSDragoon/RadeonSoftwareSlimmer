using System.Windows;
using ControlzEx.Theming;
using RadeonSoftwareSlimmer.Intefaces;

namespace RadeonSoftwareSlimmer.Services
{
    public static class ThemeService
    {
        private const string PERSONALIZE_REGISTRY_PATH = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Themes\Personalize";
        private const string APP_THEME_REGISTRY_VALUE = "AppsUseLightTheme";


        public enum Theme : int
        {
            Dark = 0,
            Light = 1,
        }


        public static void SetTheme(Theme theme)
        {
            string themeString;

            switch (theme)
            {
                case Theme.Dark:
                    themeString = "Dark.Red";
                    break;
                case Theme.Light:
                default:
                    themeString = "Light.Crimson";
                    break;
            }

            ThemeManager.Current.ChangeTheme(Application.Current, themeString);
        }

        public static void SetThemeToUserSettings(IRegistry registry)
        {
            SetTheme(GetUserAppTheme(registry));
        }


        private static Theme GetUserAppTheme(IRegistry registry)
        {
            using (IRegistryKey cnKey = registry.CurrentUser.OpenSubKey(PERSONALIZE_REGISTRY_PATH, false))
            {
                if (cnKey != null)
                {
                    object value = cnKey.GetValue(APP_THEME_REGISTRY_VALUE);

                    if (value != null)
                    {
                        return (Theme)value;
                    }
                }
            }

            return Theme.Light;
        }
    }
}
