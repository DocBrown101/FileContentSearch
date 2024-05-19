namespace ContentSearch.ViewModels;

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Collections;
using Avalonia.Platform.Storage;
using ContentSearch.Service;
using ContentSearch.Settings;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using ReactiveUI;
using Services;

public class MainViewModel : ViewModelBase
{
    private readonly AppState appState;
    private readonly FileSearchService fileSearchService;
    private readonly FileSearchCacheService fileSearchCacheService;
    private readonly FileContentSearchService fileContentSearchService;
    private readonly ILocalizationService localizationService;

    public MainViewModel(AppStateViewModel appStateViewModel, AppState appState, AppSettings appSettings)
    {
        this.AppStateViewModel = appStateViewModel;
        this.appState = appState;

        App.Tracker.Track(appSettings);
        this.SearchSettings = new SearchParameterViewModel(appSettings.SearchSettings);

        this.localizationService = new LocalizationService();
        this.fileSearchService = new FileSearchService(count => appStateViewModel.UpdateFileCountStatus(0, count));
        this.fileSearchCacheService = new FileSearchCacheService();
        this.fileContentSearchService = new FileContentSearchService(this.AddFileContentSearchResults, this.localizationService);

        this.Results = new ObservableCollection<ResultViewModel>();
        this.ResultsCollectionView = new DataGridCollectionView(this.Results);

        this.StartStopSearchCommand = new RelayCommand(this.StartStopSearch);
        this.OpenFolderPickerCommand = ReactiveCommand.CreateFromTask(this.OpenFolderPicker);
        this.OpenSelectedResultCommand = ReactiveCommand.Create(() => { PlatformService.OpenSelectedResult(this.SelectedResult); });
        this.OpenCurrentDirectoryCommand = new RelayCommand(this.OpenCurrentDirectory);
        this.ClearSearchTextCommand = new RelayCommand(this.ClearSearchText);
        this.ClearExcludedSubdirectoryNamesCommand = new RelayCommand(this.ClearExcludedSubdirectoryNames);
        this.CopyResultFilelistToClipboardCommand = new RelayCommand(this.CopyResultFilelistToClipboard);
    }

    public ObservableCollection<ResultViewModel> Results { get; }

    public DataGridCollectionView ResultsCollectionView { get; }

    public ICommand StartStopSearchCommand { get; }

    public ICommand OpenFolderPickerCommand { get; }

    public ICommand OpenSelectedResultCommand { get; }

    public ICommand OpenCurrentDirectoryCommand { get; }

    public ICommand ClearSearchTextCommand { get; }

    public ICommand ClearExcludedSubdirectoryNamesCommand { get; }

    public ICommand CopyResultFilelistToClipboardCommand { get; }

    public AppStateViewModel AppStateViewModel { get; }

    public SearchParameterViewModel SearchSettings { get; }

    public ResultViewModel? SelectedResult { get; set; }

    public static string Title => WindowTitleService.GetMainWindowTitle();

    public void SearchSettingsChanged() => this.RaisePropertyChanged(nameof(this.SearchSettings));

    private async Task OpenFolderPicker()
    {
        var folderDialogOptions = new FolderPickerOpenOptions() { Title = "SearchFolder".GetLocalizedValue(), AllowMultiple = false, };
        var storageProvider = ViewModelLocator.Instance.TopLevel?.StorageProvider;

        if (storageProvider != null)
        {
            var folderDialog = await storageProvider.OpenFolderPickerAsync(folderDialogOptions);

            if (folderDialog != null && folderDialog.Count == 1)
            {
                var result = folderDialog[0];
                this.SearchSettings.SearchPath = result.Path.AbsolutePath;
                this.SearchSettingsChanged();
            }
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

        this.RaisePropertyChanged(nameof(this.SearchSettings));
    }

    private void ClearExcludedSubdirectoryNames()
    {
        this.SearchSettings.ExcludedSubdirectoryName1 = string.Empty;
        this.SearchSettings.ExcludedSubdirectoryName2 = string.Empty;

        this.RaisePropertyChanged(nameof(this.SearchSettings));
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

        _ = ViewModelLocator.Instance.TopLevel?.Clipboard?.SetTextAsync(clipboardText.ToString());
    }

    private void StartStopSearch()
    {
        switch (this.appState.CurrentState)
        {
            case AppState.State.Idle:
                _ = this.StartOperationsAsync();
                break;
            case AppState.State.Running:
                this.appState.Abort();
                break;
        }
    }

    private async Task StartOperationsAsync()
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

        var searchFiles = this.fileSearchCacheService.CanCachedFilesBeUsed(fileContentSearchOptions)
                              ? new List<string>(this.fileSearchCacheService.Files)
                              : await this.fileSearchService.LoadFilesForSearch(fileContentSearchOptions, cancellationToken);

        if (cancellationToken.IsCancellationRequested)
        {
            this.appState.Idle();
            this.AppStateViewModel.UpdateFileCountStatus(0, 0);
            return;
        }

        this.fileSearchCacheService.SetCache(searchFiles, fileContentSearchOptions);

        await this.fileContentSearchService.StartSearch(searchFiles, fileContentSearchOptions, cancellationToken);

        this.appState.Idle();

        if (this.Results.Count == 0)
        {
            this.AppStateViewModel.UpdateFileCountStatus(0, searchFiles.Count);

            var box = MessageBoxManager.GetMessageBoxStandard(string.Empty, "NoMatchFound".GetLocalizedValue(), ButtonEnum.Ok);
            await box.ShowAsync();
        }
        else
        {
            if (this.ResultsCollectionView != null && this.ResultsCollectionView.CanSort == true)
            {
                this.ResultsCollectionView.SortDescriptions.Clear();
                this.ResultsCollectionView.SortDescriptions.Add(DataGridSortDescription.FromPath("Result.ResultFilePath", ListSortDirection.Ascending));
            }
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
}
