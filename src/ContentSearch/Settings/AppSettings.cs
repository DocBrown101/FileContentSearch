namespace ContentSearch.Settings
{
    using ContentSearch.ViewModels;

    public class AppSettings
    {
        public SearchSettings SearchSettings { get; set; }

        public GeneralSettings GeneralSettings { get; set; }

        public AppSettings()
        {
            this.SearchSettings = new SearchSettings();
            this.GeneralSettings = new GeneralSettings();
        }
    }
}
