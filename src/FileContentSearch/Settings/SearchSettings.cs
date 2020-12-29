namespace FileContentSearch.Settings
{
    using System;

    [Serializable]
    public class SearchSettings
    {
        public string SearchText1 { get; set; }

        public string SearchText2 { get; set; }

        public string ExcludedSubdirectoryName1 { get; set; }

        public string ExcludedSubdirectoryName2 { get; set; }

        public string SearchPath { get; set; }

        public string FileExtensions { get; set; }

        public bool CheckUpperLowerCase { get; set; }

        public bool CheckAllFiles { get; set; }

        public bool ExcludSubdirectoryNames { get; set; }

        ////public List<LastFolderSettingModel> LastFolderList { get; set; }

        public SearchSettings()
        {
            this.SearchPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            this.FileExtensions = ".cs .xaml";
        }
    }
}
