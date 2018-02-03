using AspNetSkeleton.Core.Infrastructure;
using System.IO;
using System.Linq;
using Autofac;
using Karambolo.Common;
using Microsoft.AspNetCore.Hosting;
using AspNetSkeleton.Common.Infrastructure;
using AspNetSkeleton.Core;
using Microsoft.Extensions.Options;

namespace AspNetSkeleton.UI.Infrastructure.Theming
{
    public interface IThemeProvider
    {
        string[] Themes { get; }
    }

    public class NullThemeProvider : IThemeProvider
    {
        public NullThemeProvider(IOptions<UISettings> settings)
        {
            Themes = new[] { settings.Value.DefaultTheme };
        }

        public string[] Themes { get; }
    }

    public class ThemeProvider : IThemeProvider, IAppBranchInitializer
    {
        public const string BasePath = "/css/themes";

        readonly IHostingEnvironment _env;

        public ThemeProvider(IHostingEnvironment env)
        {
            _env = env;
        }

        public string[] Themes { get; private set; }

        public void Initialize()
        {
            Themes = _env.WebRootFileProvider.GetDirectoryContents(BasePath)
                .Where(fi => fi.IsDirectory)
                .Select(fi => fi.Name)
                .ToArray();
        }
    }
}