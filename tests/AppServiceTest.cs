using System.Threading;

namespace MauiMonolith.Tests
{
    public static class AppServiceTest
    {
        public static int Run()
        {
            AppService service = new AppService(new AppManager(AppConfiguration.CreateDefault()));
            Expect.Equal("SUCCESS", service.PutAsync(new AppRequest("PUT", "order:2001", "pending", null), CancellationToken.None).GetAwaiter().GetResult().Status);
            Expect.Equal("pending", service.GetAsync("order:2001", CancellationToken.None).GetAwaiter().GetResult().Value);
            service.DeleteAsync("order:2001", CancellationToken.None).GetAwaiter().GetResult();
            Expect.Equal("NOT_FOUND", service.GetAsync("order:2001", CancellationToken.None).GetAwaiter().GetResult().Status);
            return 0;
        }
    }
}
