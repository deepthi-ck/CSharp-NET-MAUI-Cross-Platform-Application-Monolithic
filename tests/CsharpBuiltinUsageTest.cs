using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace MauiMonolith.Tests
{
    public static class CsharpBuiltinUsageTest
    {
        public static int Run()
        {
            string root = Directory.GetCurrentDirectory();
            if (!Directory.Exists(Path.Combine(root, "src")))
            {
                root = Path.GetFullPath(Path.Combine(root, ".."));
            }
            string store = File.ReadAllText(Path.Combine(root, "src", "InMemoryStore.cs"));
            string expiration = File.ReadAllText(Path.Combine(root, "src", "ExpirationManager.cs"));
            string eviction = File.ReadAllText(Path.Combine(root, "src", "EvictionManager.cs"));
            string service = File.ReadAllText(Path.Combine(root, "src", "AppService.cs"));
            string vm = File.ReadAllText(Path.Combine(root, "src", "ViewModels", "AppDashboardViewModel.cs"));
            Expect.True(store.Contains("ConcurrentDictionary") && store.Contains("FirstOrDefault"), "store BCL");
            Expect.True(expiration.Contains("DateTimeOffset") && expiration.Contains("TimeSpan"), "TTL BCL");
            Expect.True(eviction.Contains("FirstOrDefault"), "eviction LINQ");
            Expect.True(service.Contains("Task") && service.Contains("JsonSerializer"), "service BCL");
            Expect.True(vm.Contains("ObservableCollection") && vm.Contains("async"), "viewmodel BCL");
            InMemoryStore memory = new InMemoryStore();
            try { memory.Get(" "); throw new Exception("blank key"); } catch (ArgumentException) { }
            try { memory.Get("missing"); throw new Exception("missing"); } catch (KeyNotFoundException) { }
            DateTimeOffset now = DateTimeOffset.UtcNow;
            memory.Put(new ResourceEntry("k", "v", now, now.Add(TimeSpan.FromMilliseconds(-1)), 1));
            Expect.True(memory.Get("k").IsExpired(DateTimeOffset.UtcNow), "DateTime TTL required");
            AppService app = new AppService(new AppManager(AppConfiguration.CreateDefault()));
            try { app.PutAsync(null, CancellationToken.None).GetAwaiter().GetResult(); throw new Exception("null"); }
            catch (ArgumentNullException) { }
            return 0;
        }
    }
}
