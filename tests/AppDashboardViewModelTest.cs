using System.Threading;

namespace MauiMonolith.Tests
{
    public static class AppDashboardViewModelTest
    {
        public static int Run()
        {
            HeadlessMauiApp app = MauiProgram.CreateMauiApp();
            AppResponse put = app.Dashboard.OnPutClicked("product:1001", "Visvantha", CancellationToken.None).GetAwaiter().GetResult();
            Expect.Equal("SUCCESS", put.Status);
            AppResponse get = app.ViewModel.GetAsync("product:1001", CancellationToken.None).GetAwaiter().GetResult();
            Expect.Equal("Visvantha", get.Value);
            Expect.True(app.ViewModel.Log.Count > 0, "ObservableCollection log");
            app.Dashboard.OnDeleteClicked("product:1001", CancellationToken.None).GetAwaiter().GetResult();
            Expect.Equal("NOT_FOUND", app.ViewModel.GetAsync("product:1001", CancellationToken.None).GetAwaiter().GetResult().Status);
            app.ViewModel.RefreshHealthAsync(CancellationToken.None).GetAwaiter().GetResult();
            Expect.Equal("healthy", app.ViewModel.Health);
            Expect.True(app.ViewModel.CustomerVersion.Length > 0, "customer version bound");
            return 0;
        }
    }
}
