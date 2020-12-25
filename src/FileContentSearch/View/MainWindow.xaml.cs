namespace FileContentSearch.View
{
    using FileContentSearch.ViewModel;
    using WinForms = System.Windows.Forms;

    public partial class MainWindow
    {
        public MainWindow()
        {
            this.InitializeComponent();

            this.ViewModel.Tracker.Track(this);
        }

        private MainViewModel ViewModel => (MainViewModel)this.DataContext;

        private void OpenFolderBrowserDialog_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            using var folderBrowserDialog = new WinForms.FolderBrowserDialog
            {
                ShowNewFolderButton = false
            };

            if (folderBrowserDialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                this.ViewModel.SearchSettings.SearchPath = folderBrowserDialog.SelectedPath;
                this.ViewModel.SearchSettingsChanged();
            }
        }
    }
}
