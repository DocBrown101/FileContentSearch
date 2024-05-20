using Avalonia.Controls;

namespace ContentSearch.Views;

public partial class MainWindow : Window
{
    // private MainViewModel ViewModel => (MainViewModel)this.DataContext;

    public MainWindow()
    {
        this.InitializeComponent();

        var initialPosition = this.Position;

        App.Tracker.Track(this);

        if (this.WindowState == WindowState.Minimized)
        {
            this.WindowState = WindowState.Normal;
        }
        if (this.Position.X < -1 || this.Position.Y < -1) // -1 is used by Windows11 when docking windows on the side.
        {
            this.Position = initialPosition;
        }
    }
}
