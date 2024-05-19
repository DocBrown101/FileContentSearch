namespace ContentSearch.ViewModels
{
    using ContentSearch.Settings;
    using ReactiveUI;

    public class SearchParameterViewModel : ViewModelBase
    {
        private readonly SearchSettings searchSettings;

        public string SearchPath
        {
            get => this.searchSettings.SearchPath;
            set
            {
                if (this.searchSettings.SearchPath == value) return;

                this.searchSettings.SearchPath = value;
                this.RaisePropertyChanged();
            }
        }

        public string SearchText1
        {
            get => this.searchSettings.SearchText1;
            set
            {
                if (this.searchSettings.SearchText1 == value) return;

                this.searchSettings.SearchText1 = value;
                this.RaisePropertyChanged();
            }
        }

        public string SearchText2
        {
            get => this.searchSettings.SearchText2;
            set
            {
                if (this.searchSettings.SearchText2 == value) return;

                this.searchSettings.SearchText2 = value;
                this.RaisePropertyChanged();
            }
        }

        public string FileExtensions
        {
            get => this.searchSettings.FileExtensions;
            set
            {
                if (this.searchSettings.FileExtensions == value) return;

                this.searchSettings.FileExtensions = value;
                this.RaisePropertyChanged();
            }
        }

        public string ExcludedSubdirectoryName1
        {
            get => this.searchSettings.ExcludedSubdirectoryName1;
            set
            {
                if (this.searchSettings.ExcludedSubdirectoryName1 == value) return;

                this.searchSettings.ExcludedSubdirectoryName1 = value;
                this.RaisePropertyChanged();
            }
        }

        public string ExcludedSubdirectoryName2
        {
            get => this.searchSettings.ExcludedSubdirectoryName2;
            set
            {
                if (this.searchSettings.ExcludedSubdirectoryName2 == value) return;

                this.searchSettings.ExcludedSubdirectoryName2 = value;
                this.RaisePropertyChanged();
            }
        }

        public bool CheckUpperLowerCase
        {
            get => this.searchSettings.CheckUpperLowerCase;
            set
            {
                if (this.searchSettings.CheckUpperLowerCase == value) return;

                this.searchSettings.CheckUpperLowerCase = value;
                this.RaisePropertyChanged();
            }
        }

        public bool CheckAllFiles
        {
            get => this.searchSettings.CheckAllFiles;
            set
            {
                if (this.searchSettings.CheckAllFiles == value) return;

                this.searchSettings.CheckAllFiles = value;
                this.RaisePropertyChanged();
            }
        }

        public bool ExcludSubdirectoryNames
        {
            get => this.searchSettings.ExcludSubdirectoryNames;
            set
            {
                if (this.searchSettings.ExcludSubdirectoryNames == value) return;

                this.searchSettings.ExcludSubdirectoryNames = value;
                this.RaisePropertyChanged();
            }
        }

        public SearchParameterViewModel(SearchSettings searchSettings)
        {
            this.searchSettings = searchSettings;
        }
    }
}
