namespace ContentSearch.ViewModels
{
    using System;
    using Avalonia.Media;
    using ReactiveUI;

    public class AppStateViewModel : ViewModelBase
    {
        private readonly AppState appState;

        private string commandText;
        private SolidColorBrush commandTextColor = new SolidColorBrush(Colors.Green);

        private int searchProgressValue;
        private string fileCountStatus = string.Empty;
        private bool isBusy;
        private double progress;

        public AppStateViewModel(AppState appState)
        {
            this.appState = appState;
            this.appState.CurrentStateChanged += this.ApplicationStateOnCurrentStateChanged;

            this.commandText = "StartSearch".GetLocalizedValue();
        }

        public string CommandText
        {
            get => this.commandText;
            private set => this.RaiseAndSetIfChanged(ref this.commandText, value);
        }

        public SolidColorBrush CommandTextColor
        {
            get => this.commandTextColor;
            private set => this.RaiseAndSetIfChanged(ref this.commandTextColor, value);
        }

        public bool IsBusy
        {
            get => this.isBusy;
            private set => this.RaiseAndSetIfChanged(ref this.isBusy, value);
        }

        public double Progress
        {
            get => this.progress;
            private set => this.RaiseAndSetIfChanged(ref this.progress, value);
        }

        public string FileCountStatus
        {
            get => this.fileCountStatus;
            private set => this.RaiseAndSetIfChanged(ref this.fileCountStatus, value);
        }

        public void UpdateFileCountStatus(int newFound, int newCount)
        {
            this.FileCountStatus = $"{newFound} / {newCount}";
        }

        public void IncrementProgress(int fileCount)
        {
            this.Progress = (double)this.searchProgressValue++ / fileCount;
        }

        private void ApplicationStateOnCurrentStateChanged(object? sender, AppState.State state)
        {
            switch (state)
            {
                case AppState.State.Idle:
                    {
                        this.CommandText = "StartSearch".GetLocalizedValue();
                        this.CommandTextColor = new SolidColorBrush(Colors.Green);

                        this.searchProgressValue = 0;
                        this.Progress = 0;
                        this.IsBusy = false;
                    }

                    break;
                case AppState.State.Running:
                    {
                        this.CommandText = "Cancel".GetLocalizedValue();
                        this.CommandTextColor = new SolidColorBrush(Colors.Red);

                        this.searchProgressValue = 0;
                        this.Progress = 0;
                        this.IsBusy = true;
                    }

                    break;
                case AppState.State.Aborting:
                    {
                        this.CommandText = "Aborting".GetLocalizedValue();
                        this.CommandTextColor = new SolidColorBrush(Colors.Orange);
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(state));
            }
        }
    }
}
