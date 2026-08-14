namespace MauiMonolith
{
    public partial class VersionPage
    {
        public VersionPage(AppDashboardViewModel viewModel)
        {
            BindingContext = viewModel;
            ViewModel = viewModel;
            Title = "Version";
        }

        public string Title { get; set; }
        public AppDashboardViewModel ViewModel { get; private set; }
        public object BindingContext { get; set; }
    }
}
