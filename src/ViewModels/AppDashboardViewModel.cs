using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace MauiMonolith
{
    public sealed class AppDashboardViewModel : INotifyPropertyChanged
    {
        private readonly AppService _service;
        private readonly BuildContext _build;
        private string _status;
        private string _lastValue;
        private string _health;
        private string _versionLabel;

        public AppDashboardViewModel(AppService service, BuildContext build)
        {
            if (service == null)
            {
                throw new ArgumentNullException("service");
            }

            if (build == null)
            {
                throw new ArgumentNullException("build");
            }

            _service = service;
            _build = build;
            Log = new ObservableCollection<string>();
            _status = "ready";
            _health = "unknown";
            _versionLabel = build.CustomerVersion;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<string> Log { get; private set; }

        public string Status
        {
            get { return _status; }
            private set
            {
                _status = value;
                Raise("Status");
            }
        }

        public string LastValue
        {
            get { return _lastValue; }
            private set
            {
                _lastValue = value;
                Raise("LastValue");
            }
        }

        public string Health
        {
            get { return _health; }
            private set
            {
                _health = value;
                Raise("Health");
            }
        }

        public string VersionLabel
        {
            get { return _versionLabel; }
            private set
            {
                _versionLabel = value;
                Raise("VersionLabel");
            }
        }

        public string Branch { get { return _build.Branch; } }
        public string CustomerVersion { get { return _build.CustomerVersion; } }

        public async Task<AppResponse> PutAsync(string key, string value, CancellationToken cancellationToken)
        {
            AppResponse response = await _service.PutAsync(new AppRequest("PUT", key, value, null), cancellationToken).ConfigureAwait(false);
            Apply(response);
            return response;
        }

        public async Task<AppResponse> GetAsync(string key, CancellationToken cancellationToken)
        {
            AppResponse response = await _service.GetAsync(key, cancellationToken).ConfigureAwait(false);
            Apply(response);
            return response;
        }

        public async Task<AppResponse> DeleteAsync(string key, CancellationToken cancellationToken)
        {
            AppResponse response = await _service.DeleteAsync(key, cancellationToken).ConfigureAwait(false);
            Apply(response);
            return response;
        }

        public async Task RefreshHealthAsync(CancellationToken cancellationToken)
        {
            object health = await _service.GetHealthAsync(cancellationToken).ConfigureAwait(false);
            Health = health == null ? "unavailable" : "healthy";
            VersionInfo version = _build.ToVersionInfo();
            VersionLabel = version.CustomerVersion + " / " + version.Branch;
            Log.Add("health=" + Health + " version=" + VersionLabel);
        }

        private void Apply(AppResponse response)
        {
            Status = response.Status;
            LastValue = response.Value;
            Log.Add(response.Status + " " + response.Key + " " + response.Value);
        }

        private void Raise(string name)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
