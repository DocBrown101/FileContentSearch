namespace ContentSearch.ViewModels
{
    using System.Diagnostics;
    using System.Windows.Input;
    using Services;

    public class ResultViewModel : ViewModelBase
    {
        public ResultViewModel(FileContentSearchResult result)
        {
            this.Result = result;

            this.OpenResultCommand = new RelayCommand(this.OpenResult);
        }

        public FileContentSearchResult Result { get; }

        public ICommand OpenResultCommand { get; }

        private void OpenResult()
        {
            var processStartInfo = new ProcessStartInfo
            {
                FileName = this.Result.FileFullName,
                UseShellExecute = true
            };
            Process.Start(processStartInfo);
        }
    }
}
