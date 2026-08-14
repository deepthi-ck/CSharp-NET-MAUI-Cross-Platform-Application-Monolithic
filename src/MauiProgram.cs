using System;
using System.IO;
using System.Text.Json;

namespace MauiMonolith
{
    public static class MauiProgram
    {
        public static HeadlessMauiApp CreateMauiApp()
        {
            BuildContext build = BuildContext.FromAssembly();
            build.ValidateMonolithSameVersion();
            AppConfiguration configuration = LoadConfiguration();
            int portOverride;
            string portEnv = Environment.GetEnvironmentVariable("APP_PORT");
            if (!string.IsNullOrWhiteSpace(portEnv) && int.TryParse(portEnv, out portOverride))
            {
                configuration.Port = portOverride;
            }

            AppManager manager = new AppManager(configuration);
            AppService service = new AppService(manager);
            service.LoadSampleData(FindFile("data", "sample-app-data.json"));
            AppDashboardViewModel viewModel = new AppDashboardViewModel(service, build);
            AppShell shell = new AppShell(viewModel);
            App app = new App(shell);
            MainPage mainPage = new MainPage(viewModel);
            app.MainPage = mainPage;
            return new HeadlessMauiApp(build, configuration, service, viewModel, shell, app, mainPage);
        }

        private static AppConfiguration LoadConfiguration()
        {
            string path = FindFile("config", "appsettings.json");
            if (File.Exists(path))
            {
                return JsonSerializer.Deserialize<AppConfiguration>(File.ReadAllText(path)) ?? AppConfiguration.CreateDefault();
            }

            return AppConfiguration.CreateDefault();
        }

        internal static string FindFile(string folder, string name)
        {
            string[] roots = new[]
            {
                AppContext.BaseDirectory,
                Directory.GetCurrentDirectory(),
                Path.Combine(Directory.GetCurrentDirectory(), "src")
            };

            foreach (string root in roots)
            {
                string candidate = Path.Combine(root, folder, name);
                if (File.Exists(candidate))
                {
                    return candidate;
                }
            }

            return Path.Combine(Directory.GetCurrentDirectory(), folder, name);
        }
    }

    public sealed class HeadlessMauiApp
    {
        public HeadlessMauiApp(
            BuildContext build,
            AppConfiguration configuration,
            AppService service,
            AppDashboardViewModel viewModel,
            AppShell shell,
            App app,
            MainPage mainPage)
        {
            Build = build;
            Configuration = configuration;
            Service = service;
            ViewModel = viewModel;
            Shell = shell;
            App = app;
            MainPage = mainPage;
        }

        public BuildContext Build { get; private set; }
        public AppConfiguration Configuration { get; private set; }
        public AppService Service { get; private set; }
        public AppDashboardViewModel ViewModel { get; private set; }
        public AppShell Shell { get; private set; }
        public App App { get; private set; }
        public MainPage MainPage { get; private set; }
        public AppDashboardPage Dashboard { get { return Shell.Dashboard; } }
    }
}
