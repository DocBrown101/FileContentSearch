namespace ContentSearch.ViewModels.DesignTime
{
    using ContentSearch.Settings;
    using Jot;

    public class DesignMainViewModel : MainViewModel
    {
        public DesignMainViewModel() : base(new AppStateViewModel(GetAppState()), GetAppState(), GetAppSettings())
        { }

        private static AppState GetAppState()
        {
            return new AppState();
        }

        private static AppSettings GetAppSettings()
        {
            return new AppSettings()
            {
                SearchSettings = new SearchSettings()
                {
                    SearchPath = @"C:\"
                }
            };
        }
    }
}
