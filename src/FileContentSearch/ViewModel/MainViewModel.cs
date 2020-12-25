namespace FileContentSearch.ViewModel
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.IO;
    using System.Media;
    using System.Reflection;
    using System.Text;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Input;
    using FileContentSearch.Service;
    using FileContentSearch.Settings;
    using FileContentSearch.ViewModel.ChildViewModel;
    using Jot;
    using Services;

    public class MainViewModel : Observable
    {
        private readonly AppState appState;
        private readonly FileSearchService fileSearchService;
        private readonly FileSearchCacheService fileSearchCacheService;
        private readonly FileContentSearchService fileContentSearchService;
        private readonly ILocalizationService localizationService;

        public MainViewModel()
        {
            this.SearchSettings = new SearchSettings
            {
                SearchPath = @"C:\"
            };
        }

        public MainViewModel(AppStateViewModel appStateViewModel, AppState appState, AppSettings appSettings, Tracker tracker)
        {
            if (appSettings == null)
            {
                return;
            }

            this.AppStateViewModel = appStateViewModel;
            this.appState = appState;

            this.Tracker = tracker;
            this.Tracker.Track(appSettings);
            this.SearchSettings = appSettings.SearchSettings;

            this.localizationService = new LocalizationService();
            this.fileSearchService = new FileSearchService(count => appStateViewModel.UpdateFileCountStatus(0, count));
            this.fileSearchCacheService = new FileSearchCacheService();
            this.fileContentSearchService = new FileContentSearchService(this.AddFileContentSearchResults, this.localizationService);

            this.Results = new ObservableCollection<ResultViewModel>();
            this.ResultsCollectionView = CollectionViewSource.GetDefaultView(this.Results);

            this.StartStopSearchCommand = new RelayCommand(this.StartStopSearch);
            this.OpenSelectedResultCommand = new RelayCommand(this.OpenSelectedResult);
            this.OpenCurrentDirectoryCommand = new RelayCommand(this.OpenCurrentDirectory);
            this.ClearSearchTextCommand = new RelayCommand(this.ClearSearchText);
            this.ClearExcludedSubdirectoryNamesCommand = new RelayCommand(this.ClearExcludedSubdirectoryNames);
            this.CopyResultFilelistToClipboardCommand = new RelayCommand(this.CopyResultFilelistToClipboard);
        }

        public ObservableCollection<ResultViewModel> Results { get; }

        public ICollectionView ResultsCollectionView { get; }

        public ICommand StartStopSearchCommand { get; }

        public ICommand OpenSelectedResultCommand { get; }

        public ICommand OpenCurrentDirectoryCommand { get; }

        public ICommand ClearSearchTextCommand { get; }

        public ICommand ClearExcludedSubdirectoryNamesCommand { get; }

        public ICommand CopyResultFilelistToClipboardCommand { get; }

        public AppStateViewModel AppStateViewModel { get; }

        public Tracker Tracker { get; }

        public SearchSettings SearchSettings { get; }

        public ResultViewModel SelectedResult { get; set; }

        public static string Title => IsDebugSession() == true ? "File-Content-Search (DEBUG)" : $"File-Content-Search {Assembly.GetExecutingAssembly().GetName().Version}";

        public void SearchSettingsChanged() => this.OnPropertyChanged(nameof(this.SearchSettings));

        private void OpenSelectedResult()
        {
            if (this.SelectedResult != null)
            {
                var fileInfo = new FileInfo(this.SelectedResult.Result.FileFullName);
                var args = $"/e, /select, \"{(fileInfo.Exists ? fileInfo.FullName : fileInfo.DirectoryName)}\"";

                Process.Start("explorer", args);
            }
        }

        private void OpenCurrentDirectory()
        {
            var directoryInfo = new DirectoryInfo(this.SearchSettings.SearchPath);

            if (directoryInfo.Exists)
            {
                Process.Start("explorer", directoryInfo.FullName);
            }
        }

        private void ClearSearchText()
        {
            this.SearchSettings.SearchText1 = string.Empty;
            this.SearchSettings.SearchText2 = string.Empty;

            this.OnPropertyChanged(nameof(this.SearchSettings));
        }

        private void ClearExcludedSubdirectoryNames()
        {
            this.SearchSettings.ExcludedSubdirectoryName1 = string.Empty;
            this.SearchSettings.ExcludedSubdirectoryName2 = string.Empty;

            this.OnPropertyChanged(nameof(this.SearchSettings));
        }

        private void CopyResultFilelistToClipboard()
        {
            if (this.Results.Count <= 0)
            {
                return;
            }

            var fileFullNames = new StringBuilder();
            var resultFileNames = new StringBuilder();
            var clipboardText = new StringBuilder();

            fileFullNames.AppendLine("FullLengthPaths".GetLocalizedValue());
            resultFileNames.AppendLine("RelativePaths".GetLocalizedValue());

            foreach (var result in this.Results)
            {
                fileFullNames.AppendLine(result.Result.FileFullName);
                resultFileNames.AppendLine(result.Result.ResultFileName);
            }

            clipboardText.AppendLine(fileFullNames.ToString());
            clipboardText.AppendLine();
            clipboardText.AppendLine(resultFileNames.ToString());

            Clipboard.SetText(clipboardText.ToString());
            SystemSounds.Beep.Play();
        }

        private void StartStopSearch()
        {
            switch (this.appState.CurrentState)
            {
                case AppState.State.Idle:
                    this.StartOperationsAsync();
                    break;
                case AppState.State.Running:
                    this.appState.Abort();
                    break;
            }
        }

        private async void StartOperationsAsync()
        {
            var fileContentSearchOptions = new FileContentSearchOptions(
                this.localizationService,
                new MessageBoxService(),
                this.SearchSettings.SearchPath,
                this.GetSearchTags(),
                this.SearchSettings.CheckUpperLowerCase,
                this.SearchSettings.CheckAllFiles,
                this.SearchSettings.FileExtensions,
                this.SearchSettings.ExcludSubdirectoryNames,
                this.GetExcludedSubdirectoryNames());

            if (fileContentSearchOptions.IsNotValid())
            {
                return;
            }

            var cancellationToken = this.appState.Run();

            this.SearchSettings.SearchPath = fileContentSearchOptions.SearchPath;

            this.Results.Clear();

            try
            {
                var searchFiles = this.fileSearchCacheService.CanCachedFilesBeUsed(fileContentSearchOptions)
                                      ? new List<string>(this.fileSearchCacheService.Files)
                                      : await this.fileSearchService.LoadFilesForSearch(fileContentSearchOptions, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    this.AppStateViewModel.UpdateFileCountStatus(0, 0);
                    return;
                }

                this.fileSearchCacheService.SetCache(searchFiles, fileContentSearchOptions);

                await this.fileContentSearchService.StartSearch(searchFiles, fileContentSearchOptions, cancellationToken);

                if (this.Results.Count == 0)
                {
                    this.AppStateViewModel.UpdateFileCountStatus(0, searchFiles.Count);
                    MessageBox.Show("NoMatchFound".GetLocalizedValue());
                }
                else
                {
                    if (this.ResultsCollectionView != null && this.ResultsCollectionView.CanSort == true)
                    {
                        this.ResultsCollectionView.SortDescriptions.Clear();
                        this.ResultsCollectionView.SortDescriptions.Add(new SortDescription("Result.ResultFilePath", ListSortDirection.Ascending));
                    }
                }
            }
            finally
            {
                this.appState.Idle();
            }
        }

        private HashSet<string> GetSearchTags()
        {
            var searchTags = new HashSet<string>();

            if (!string.IsNullOrEmpty(this.SearchSettings.SearchText1))
            {
                searchTags.Add(this.SearchSettings.SearchText1);
            }

            if (!string.IsNullOrEmpty(this.SearchSettings.SearchText2))
            {
                searchTags.Add(this.SearchSettings.SearchText2);
            }

            return searchTags;
        }

        private HashSet<string> GetExcludedSubdirectoryNames()
        {
            var excludedSubdirectoryNames = new HashSet<string>();

            if (!string.IsNullOrEmpty(this.SearchSettings.ExcludedSubdirectoryName1))
            {
                excludedSubdirectoryNames.Add(this.SearchSettings.ExcludedSubdirectoryName1.Trim());
            }

            if (!string.IsNullOrEmpty(this.SearchSettings.ExcludedSubdirectoryName2))
            {
                excludedSubdirectoryNames.Add(this.SearchSettings.ExcludedSubdirectoryName2.Trim());
            }

            return excludedSubdirectoryNames;
        }

        private void AddFileContentSearchResults(FileContentSearchResult searchTextResult, int fileCount)
        {
            if (searchTextResult != null)
            {
                this.Results.Add(new ResultViewModel(searchTextResult));
                this.AppStateViewModel.UpdateFileCountStatus(this.Results.Count, fileCount);
            }

            this.AppStateViewModel.IncrementProgress(fileCount);
        }

        private static bool IsDebugSession()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}
