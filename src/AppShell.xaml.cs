using System;
using System.Collections.Generic;

namespace MauiMonolith
{
    public partial class AppShell
    {
        public const string RouteDashboard = "dashboard";
        public const string RouteResources = "resources";
        public const string RouteStats = "stats";
        public const string RouteNodes = "nodes";
        public const string RouteHealth = "health";
        public const string RouteVersion = "version";

        private readonly Dictionary<string, object> _pages;

        public AppShell(AppDashboardViewModel viewModel)
        {
            if (viewModel == null)
            {
                throw new ArgumentNullException("viewModel");
            }

            ViewModel = viewModel;
            Dashboard = new AppDashboardPage(viewModel);
            Resources = new ResourcesPage(viewModel);
            Stats = new StatsPage(viewModel);
            Nodes = new NodesPage(viewModel);
            Health = new HealthPage(viewModel);
            Version = new VersionPage(viewModel);
            _pages = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            _pages[RouteDashboard] = Dashboard;
            _pages[RouteResources] = Resources;
            _pages[RouteStats] = Stats;
            _pages[RouteNodes] = Nodes;
            _pages[RouteHealth] = Health;
            _pages[RouteVersion] = Version;
            GoTo(RouteDashboard);
        }

        public AppDashboardViewModel ViewModel { get; private set; }
        public AppDashboardPage Dashboard { get; private set; }
        public ResourcesPage Resources { get; private set; }
        public StatsPage Stats { get; private set; }
        public NodesPage Nodes { get; private set; }
        public HealthPage Health { get; private set; }
        public VersionPage Version { get; private set; }
        public string CurrentRoute { get; private set; }
        public object CurrentPage { get; private set; }

        public IList<string> Routes
        {
            get { return new List<string> { RouteDashboard, RouteResources, RouteStats, RouteNodes, RouteHealth, RouteVersion }; }
        }

        public bool GoTo(string route)
        {
            if (string.IsNullOrWhiteSpace(route) || !_pages.ContainsKey(route))
            {
                return false;
            }

            CurrentRoute = route;
            CurrentPage = _pages[route];
            return true;
        }
    }
}
