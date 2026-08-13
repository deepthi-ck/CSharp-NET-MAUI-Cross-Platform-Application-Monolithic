using System;
using System.Collections.Generic;

namespace MauiMonolith.Tests
{
    public static class PartitionRouterTest
    {
        public static int Run()
        {
            PartitionRouter router = new PartitionRouter(new List<StoreNode>
            {
                new StoreNode("node-1"),
                new StoreNode("node-2"),
                new StoreNode("node-3")
            });
            Expect.Equal(router.PrimaryFor("product:1001").Id, router.PrimaryFor("product:1001").Id);
            Expect.Equal(2, router.ReplicasFor("product:1001").Count);
            return 0;
        }
    }
}
