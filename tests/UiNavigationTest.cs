using System;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace MauiMonolith.Tests
{
    public static class UiNavigationTest
    {
        public static int Run()
        {
            HeadlessMauiApp app = MauiProgram.CreateMauiApp();
            Expect.True(app.Shell.Routes.Count == 6, "six shell routes");
            Expect.True(app.Shell.GoTo(AppShell.RouteResources), "goto resources");
            Expect.Equal(AppShell.RouteResources, app.Shell.CurrentRoute);
            Expect.True(app.Shell.Resources.KeyPlaceholder == "product:1001", "key placeholder");
            Expect.True(app.Shell.Resources.ValuePlaceholder == "Visvantha", "value placeholder");
            Expect.True(app.Shell.Dashboard.KeyPlaceholder == "product:1001", "dashboard key placeholder");
            Expect.True(app.Shell.GoTo(AppShell.RouteStats), "goto stats");
            Expect.True(app.Shell.GoTo(AppShell.RouteNodes), "goto nodes");
            Expect.True(app.Shell.GoTo(AppShell.RouteHealth), "goto health");
            Expect.True(app.Shell.GoTo(AppShell.RouteVersion), "goto version");
            Expect.True(app.Shell.GoTo(AppShell.RouteDashboard), "goto dashboard");
            Expect.True(!app.Shell.GoTo("missing-page"), "unknown route rejected");

            string prefix = "http://127.0.0.1:18082/";
            using (MauiUiHost host = new MauiUiHost(app, prefix, FindWwwroot()))
            {
                host.Start();
                Thread.Sleep(250);
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(8);
                    string[] pages = new[] { "/", "/resources.html", "/stats.html", "/nodes.html", "/health.html", "/version.html" };
                    for (int i = 0; i < pages.Length; i++)
                    {
                        string html = client.GetStringAsync(prefix.TrimEnd('/') + pages[i]).GetAwaiter().GetResult();
                        Expect.True(html.IndexOf("MAUI", StringComparison.Ordinal) >= 0, "title " + pages[i]);
                        Expect.True(html.IndexOf("id=\"sidebar\"", StringComparison.Ordinal) >= 0, "nav " + pages[i]);
                    }

                    string resources = client.GetStringAsync(prefix.TrimEnd('/') + "/resources.html").GetAwaiter().GetResult();
                    Expect.True(resources.IndexOf("product:1001", StringComparison.Ordinal) >= 0, "key placeholder html");
                    Expect.True(resources.IndexOf("Visvantha", StringComparison.Ordinal) >= 0, "value placeholder html");
                    Expect.True(resources.IndexOf("placeholder=\"Name\"", StringComparison.OrdinalIgnoreCase) < 0, "no default Name");

                    string list = client.GetStringAsync(prefix.TrimEnd('/') + "/api/resources").GetAwaiter().GetResult();
                    Expect.True(list.IndexOf("product:1001", StringComparison.Ordinal) >= 0, "sample data listed");
                }
            }

            return 0;
        }

        private static string FindRoot()
        {
            string cwd = Directory.GetCurrentDirectory();
            if (Directory.Exists(Path.Combine(cwd, "src"))) { return cwd; }
            if (Directory.Exists(Path.Combine(cwd, "..", "src"))) { return Path.GetFullPath(Path.Combine(cwd, "..")); }
            return cwd;
        }

        private static string FindWwwroot()
        {
            string[] candidates = new[]
            {
                Path.Combine(AppContext.BaseDirectory, "wwwroot"),
                Path.Combine(FindRoot(), "src", "wwwroot")
            };
            for (int i = 0; i < candidates.Length; i++)
            {
                if (Directory.Exists(candidates[i])) { return candidates[i]; }
            }

            return Path.Combine(FindRoot(), "src", "wwwroot");
        }
    }
}
