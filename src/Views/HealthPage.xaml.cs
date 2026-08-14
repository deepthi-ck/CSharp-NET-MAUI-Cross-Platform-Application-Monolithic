namespace MauiMonolith
{
    public partial class HealthPage
    {
        public HealthPage(AppDashboardViewModel viewModel)
        {
            BindingContext = viewModel;
            ViewModel = viewModel;
            Title = "Health";
        }

        public string Title { get; set; }
        public AppDashboardViewModel ViewModel { get; private set; }
        public object BindingContext { get; set; }
    }
}
