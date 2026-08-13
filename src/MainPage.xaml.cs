namespace MauiMonolith
{
    public partial class MainPage
    {
        public MainPage(AppDashboardViewModel viewModel)
        {
            BindingContext = viewModel;
            ViewModel = viewModel;
        }

        public AppDashboardViewModel ViewModel { get; private set; }
        public object BindingContext { get; set; }
    }
}
