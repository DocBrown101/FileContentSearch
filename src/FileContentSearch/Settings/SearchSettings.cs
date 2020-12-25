namespace FileContentSearch.Settings
{
    using System;
    using System.ComponentModel;

    [Serializable]
    public class SearchSettings
    {
        public string SearchText1 { get; set; }

        public string SearchText2 { get; set; }

        public string ExcludedSubdirectoryName1 { get; set; }

        public string ExcludedSubdirectoryName2 { get; set; }

        [DefaultValue("Bitte auswählen ...")]
        public string SearchPath { get; set; }

        [DefaultValue(".cs .xaml .tt")]
        public string FileExtensions { get; set; }

        [DefaultValue(true)]
        public bool CheckUpperLowerCase { get; set; }

        [DefaultValue(false)]
        public bool CheckAllFiles { get; set; }

        [DefaultValue(false)]
        public bool ExcludSubdirectoryNames { get; set; }

        ////public List<LastFolderSettingModel> LastFolderList { get; set; }
    }
}
