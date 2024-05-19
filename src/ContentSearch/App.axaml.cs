using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ContentSearch.Settings;
using ContentSearch.ViewModels;
using ContentSearch.Views;
using Jot;
using Microsoft.Extensions.DependencyInjection;

namespace ContentSearch;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static Tracker Tracker = new();

    public override void OnFrameworkInitializationCompleted()
    {
        Lang.Resources.Culture = new CultureInfo("de");

        if (this.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            this.InitAppTracker(desktop);

            var collection = new ServiceCollection();
            collection.AddSingleton<AppState>();
            collection.AddSingleton<AppSettings>();
            collection.AddSingleton<AppStateViewModel>();
            collection.AddSingleton<MainViewModel>();

            var services = collection.BuildServiceProvider();

            desktop.MainWindow = new MainWindow
            {
                DataContext = services.GetRequiredService<MainViewModel>()
            };

            ViewModelLocator.Instance.SetTopLevel(desktop.MainWindow);
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void InitAppTracker(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var name = string.Empty;

#if DEBUG
        name = "DEBUG";
#endif

        Tracker.Configure<MainWindow>()
               .Id(w => string.Empty, name)
               .Properties(w => new { w.WindowState, w.Position, w.Width, w.Height })
               .PersistOn(nameof(Window.Closing))
               .StopTrackingOn(nameof(Window.Closing));

        Tracker.Configure<AppSettings>()
               .Id(w => string.Empty, name)
               .Properties(s => new { s.SearchSettings, s.GeneralSettings })
               .PersistOn(nameof(desktop.ShutdownRequested), desktop)
               .StopTrackingOn(nameof(desktop.ShutdownRequested), desktop);
    }
}
