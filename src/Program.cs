using System;
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
            Console.WriteLine("C# .NET MAUI Cross-Platform Application (Scenario 1 - Monolithic)");
            Console.WriteLine("Branch: " + app.Build.Branch);
            Console.WriteLine("Customer Version: " + app.Build.CustomerVersion);
            Console.WriteLine("TFM: " + app.Build.TargetFramework);
            Console.WriteLine("Headless MAUI host ready (UI runtime optional).");

            if (HasFlag(args, "--self-test") || HasFlag(args, "--client-e2e"))
            {
                return await SelfTest(app).ConfigureAwait(false);
            }

            if (HasFlag(args, "--once"))
            {
                await app.ViewModel.RefreshHealthAsync(CancellationToken.None).ConfigureAwait(false);
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
            return 0;
        }

        private static async Task<int> SelfTest(HeadlessMauiApp app)
        {
            CancellationToken token = CancellationToken.None;
            AppResponse put = await app.Dashboard.OnPutClicked("product:1001", "Visvantha", token).ConfigureAwait(false);
            if (!string.Equals(put.Status, "SUCCESS", StringComparison.Ordinal))
            {
                Console.Error.WriteLine("PUT failed");
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

            Console.WriteLine("Self-test PASS");
            return 0;
        }

        private static bool HasFlag(string[] args, string flag)
        {
            if (args == null)
            {
                return false;
            }

            foreach (string arg in args)
            {
                if (string.Equals(arg, flag, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
