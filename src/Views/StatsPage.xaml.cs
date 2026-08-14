namespace MauiMonolith
{
    public partial class StatsPage
    {
        public StatsPage(AppDashboardViewModel viewModel)
        {
            BindingContext = viewModel;
            ViewModel = viewModel;
            Title = "Statistics";
        }

        public string Title { get; set; }
        public AppDashboardViewModel ViewModel { get; private set; }
        public object BindingContext { get; set; }
    }
}
