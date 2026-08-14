namespace MauiMonolith
{
    public partial class NodesPage
    {
        public NodesPage(AppDashboardViewModel viewModel)
        {
            BindingContext = viewModel;
            ViewModel = viewModel;
            Title = "Partition nodes";
        }

        public string Title { get; set; }
        public AppDashboardViewModel ViewModel { get; private set; }
        public object BindingContext { get; set; }
    }
}
