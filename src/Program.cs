using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MauiMonolith
{
    public static class Program
    {
        public static int Main(string[] args)
        {
            try
            {
                return Run(args).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex);
                return 1;
            }
        }

        private static async Task<int> Run(string[] args)
        {
            HeadlessMauiApp app = MauiProgram.CreateMauiApp();
            string prefix = "http://" + app.Configuration.Host + ":" + app.Configuration.Port + "/";
            string wwwroot = FindDirectory("wwwroot");

            Console.WriteLine("C# .NET MAUI Cross-Platform Application (Scenario 1 - Monolithic)");
            Console.WriteLine("Branch: " + app.Build.Branch);
            Console.WriteLine("Customer Version: " + app.Build.CustomerVersion);
            Console.WriteLine("TFM: " + app.Build.TargetFramework);

            if (HasFlag(args, "--client-e2e"))
            {
                return await HttpSelfTest(prefix).ConfigureAwait(false);
            }

            using (MauiUiHost host = new MauiUiHost(app, prefix, wwwroot))
            {
                host.Start();
                Console.WriteLine("Listening: " + prefix);
                Console.WriteLine("Operator UI: " + prefix);

                if (HasFlag(args, "--self-test"))
                {
                    int inProcess = await InProcessSelfTest(app).ConfigureAwait(false);
                    if (inProcess != 0)
                    {
                        host.Stop();
                        return inProcess;
                    }

                    int http = await HttpSelfTest(prefix).ConfigureAwait(false);
                    host.Stop();
                    return http;
                }

                if (HasFlag(args, "--once"))
                {
                    await app.ViewModel.RefreshHealthAsync(CancellationToken.None).ConfigureAwait(false);
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                    host.Stop();
                    return 0;
                }

                await app.ViewModel.RefreshHealthAsync(CancellationToken.None).ConfigureAwait(false);
                Console.WriteLine("Press Ctrl+C to stop.");
                ManualResetEventSlim done = new ManualResetEventSlim(false);
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    done.Set();
                };
                done.Wait();
                host.Stop();
                return 0;
            }
        }

        private static async Task<int> InProcessSelfTest(HeadlessMauiApp app)
        {
            CancellationToken token = CancellationToken.None;
            if (!app.Shell.GoTo(AppShell.RouteResources) || app.Shell.CurrentPage != app.Shell.Resources)
            {
                Console.Error.WriteLine("Shell navigation to Resources failed");
                return 1;
            }

            AppResponse put = await app.Shell.Resources.OnPutClicked("product:1001", "Visvantha", token).ConfigureAwait(false);
            if (!string.Equals(put.Status, "SUCCESS", StringComparison.Ordinal))
            {
                Console.Error.WriteLine("PUT failed");
                return 1;
            }

            if (!app.Shell.GoTo(AppShell.RouteDashboard))
            {
                Console.Error.WriteLine("Shell navigation to Dashboard failed");
                return 1;
            }

            AppResponse get = await app.Dashboard.OnGetClicked("product:1001", token).ConfigureAwait(false);
            if (!string.Equals(get.Value, "Visvantha", StringComparison.Ordinal))
            {
                Console.Error.WriteLine("GET failed");
                return 1;
            }

            await app.Dashboard.OnDeleteClicked("product:1001", token).ConfigureAwait(false);
            AppResponse missing = await app.Dashboard.OnGetClicked("product:1001", token).ConfigureAwait(false);
            if (!string.Equals(missing.Status, "NOT_FOUND", StringComparison.Ordinal))
            {
                Console.Error.WriteLine("DELETE failed");
                return 1;
            }

            string[] routes = new[]
            {
                AppShell.RouteDashboard,
                AppShell.RouteResources,
                AppShell.RouteStats,
                AppShell.RouteNodes,
                AppShell.RouteHealth,
                AppShell.RouteVersion
            };
            for (int i = 0; i < routes.Length; i++)
            {
                if (!app.Shell.GoTo(routes[i]))
                {
                    Console.Error.WriteLine("Shell navigation failed: " + routes[i]);
                    return 1;
                }
            }

            Console.WriteLine("In-process self-test PASS");
            return 0;
        }

        private static async Task<int> HttpSelfTest(string prefix)
        {
            using (HttpClient client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                string resource = prefix.TrimEnd('/') + "/api/resources/product:1001";
                HttpResponseMessage put = await client.PutAsync(resource, new StringContent("{\"value\":\"Visvantha\"}", Encoding.UTF8, "application/json")).ConfigureAwait(false);
                string putBody = await put.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!put.IsSuccessStatusCode || putBody.IndexOf("SUCCESS", StringComparison.Ordinal) < 0)
                {
                    Console.Error.WriteLine("HTTP PUT failed: " + putBody);
                    return 1;
                }

                string[] pages = new[] { "/", "/resources.html", "/stats.html", "/nodes.html", "/health.html", "/version.html", "/css/app.css", "/js/app.js" };
                foreach (string page in pages)
                {
                    HttpResponseMessage ui = await client.GetAsync(prefix.TrimEnd('/') + page).ConfigureAwait(false);
                    if (!ui.IsSuccessStatusCode)
                    {
                        Console.Error.WriteLine("UI failed: " + page + " " + (int)ui.StatusCode);
                        return 1;
                    }

                    string html = await ui.Content.ReadAsStringAsync().ConfigureAwait(false);
                    if (page.EndsWith(".html") || string.Equals(page, "/", StringComparison.Ordinal))
                    {
                        if (html.IndexOf("MAUI", StringComparison.Ordinal) < 0 || html.IndexOf("sidebar", StringComparison.OrdinalIgnoreCase) < 0)
                        {
                            Console.Error.WriteLine("UI navigation markup missing: " + page);
                            return 1;
                        }
                    }
                }

                Console.WriteLine("HTTP self-test PASS");
                return 0;
            }
        }

        private static string FindDirectory(string name)
        {
            string[] roots = new[]
            {
                AppContext.BaseDirectory,
                Directory.GetCurrentDirectory(),
                Path.Combine(Directory.GetCurrentDirectory(), "src")
            };
            foreach (string root in roots)
            {
                string candidate = Path.Combine(root, name);
                if (Directory.Exists(candidate)) { return candidate; }
            }

            return Path.Combine(AppContext.BaseDirectory, name);
        }

        private static bool HasFlag(string[] args, string flag)
        {
            if (args == null) { return false; }
            foreach (string arg in args)
            {
                if (string.Equals(arg, flag, StringComparison.OrdinalIgnoreCase)) { return true; }
            }

            return false;
        }
    }
}
