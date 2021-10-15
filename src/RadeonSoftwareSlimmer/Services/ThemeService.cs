using System.Windows;
using ControlzEx.Theming;
using Microsoft.Win32;

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

        public static void SetThemeToUserSettings()
        {
            SetTheme(GetUserAppTheme());
        }


        private static Theme GetUserAppTheme()
        {
            using (RegistryKey cnKey = Registry.CurrentUser.OpenSubKey(PERSONALIZE_REGISTRY_PATH))
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
