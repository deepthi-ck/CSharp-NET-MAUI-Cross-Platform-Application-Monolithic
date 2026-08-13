namespace MauiMonolith
{
    public partial class App
    {
        public App(AppShell shell)
        {
            Shell = shell;
            MainPage = shell.Dashboard;
        }

        public AppShell Shell { get; private set; }
        public object MainPage { get; set; }
    }
}
