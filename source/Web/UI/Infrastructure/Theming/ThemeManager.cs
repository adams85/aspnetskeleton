using System;

namespace AspNetSkeleton.UI.Infrastructure.Theming
{
    public interface IThemeManager
    {
        IThemeProvider Provider { get; }
        string CurrentTheme { get; set; }
    }

    public class NullThemeManager : IThemeManager
    {
        public NullThemeManager(IThemeProvider provider)
        {
            Provider = provider;
        }

        public IThemeProvider Provider { get; }

        public string CurrentTheme
        {
            get => Provider.Themes[0];
            set => throw new NotSupportedException();
        }
    }

    public class ThemeManager : IThemeManager
    {
        public ThemeManager(IThemeProvider provider)
        {
            Provider = provider;
        }

        public IThemeProvider Provider { get; }

        // TODO: implement if needed
        public string CurrentTheme
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }
    }
}