using AspNetSkeleton.Core.Infrastructure;
using System.IO;
using System.Linq;
using Autofac;
using Karambolo.Common;

namespace AspNetSkeleton.UI.Infrastructure.Theming
{
    public interface IThemeProvider
    {
        string[] Themes { get; }
    }

    public class NullThemeProvider : IThemeProvider
    {
        public string[] Themes { get; } = { UIConstants.DefaultTheme };
    }

    public class ThemeProvider : IThemeProvider, IStartable
    {
        public const string BaseUrl = "~/Static/Stylesheets/Themes";

        readonly IEnvironment _environment;

        public ThemeProvider(IEnvironment environment)
        {
            _environment = environment;
        }

        public string[] Themes { get; private set; }

        public void Start()
        {
            Themes = Directory.EnumerateDirectories(_environment.MapPath(BaseUrl), "*", SearchOption.TopDirectoryOnly)
                .Select(p => Path.GetFileName(p))
                .ToArray();
        }
    }
}