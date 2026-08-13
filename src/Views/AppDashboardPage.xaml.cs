using System.Threading;
using System.Threading.Tasks;

namespace MauiMonolith
{
    public partial class AppDashboardPage
    {
        public AppDashboardPage(AppDashboardViewModel viewModel)
        {
            BindingContext = viewModel;
            ViewModel = viewModel;
        }

        public AppDashboardViewModel ViewModel { get; private set; }
        public object BindingContext { get; set; }

        public Task<AppResponse> OnPutClicked(string key, string value, CancellationToken cancellationToken)
        {
            return ViewModel.PutAsync(key, value, cancellationToken);
        }

        public Task<AppResponse> OnGetClicked(string key, CancellationToken cancellationToken)
        {
            return ViewModel.GetAsync(key, cancellationToken);
        }

        public Task<AppResponse> OnDeleteClicked(string key, CancellationToken cancellationToken)
        {
            return ViewModel.DeleteAsync(key, cancellationToken);
        }
    }
}
