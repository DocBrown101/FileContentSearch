namespace FileContentSearch.ViewModel
{
    using System.Windows;
    using Autofac;
    using FileContentSearch.Settings;
    using FileContentSearch.ViewModel.ChildViewModel;
    using Jot;

    public class ViewModelLocator
    {
        private readonly IContainer container;

        public ViewModelLocator()
        {
            var builder = new ContainerBuilder();

            builder.Register((context) =>
            {
                return GetTracker();
            }).SingleInstance();

            builder.RegisterType<AppState>().SingleInstance();
            builder.RegisterType<AppSettings>().SingleInstance();

            builder.RegisterType<AppStateViewModel>().SingleInstance();
            builder.RegisterType<MainViewModel>().SingleInstance();

            this.container = builder.Build();
        }

        public MainViewModel Main => this.container.Resolve<MainViewModel>();

        private static Tracker GetTracker()
        {
#if DEBUG
            const string build = "DEBUG";
#else
            const string build = "MAIN";
#endif

            var tracker = new Tracker();

            tracker.Configure<Window>()
                   .Id(w => w.Name, $"{SystemParameters.VirtualScreenWidth}.{build}")
                   .Properties(w => new { w.Top, w.Width, w.Height, w.Left, w.WindowState })
                   .PersistOn(nameof(Window.Closing))
                   .StopTrackingOn(nameof(Window.Closing));

            tracker.Configure<AppSettings>()
                   .Id(w => nameof(AppSettings), build)
                   .Properties(s => new { s.SearchSettings, s.GeneralSettings })
                   .PersistOn("Exit", Application.Current)
                   .StopTrackingOn("Exit", Application.Current);

            return tracker;
        }
    }
}
