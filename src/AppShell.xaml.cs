namespace MauiMonolith
{
    public partial class AppShell
    {
        public AppShell(AppDashboardViewModel viewModel)
        {
            ViewModel = viewModel;
            Dashboard = new AppDashboardPage(viewModel);
        }

        public AppDashboardViewModel ViewModel { get; private set; }
        public AppDashboardPage Dashboard { get; private set; }
    }
}
