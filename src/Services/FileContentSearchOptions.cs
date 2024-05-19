namespace Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;

    public class FileContentSearchOptions
    {
        private readonly ILocalizationService localizationService;
        private readonly IMessageBoxService messageBoxService;

        public FileContentSearchOptions(ILocalizationService localizationService,
            IMessageBoxService messageBoxService,
            string searchPath,
            ICollection<string> searchTags,
            bool checkUpperLowerCase,
            bool checkAllFiles,
            string fileExtensions,
            bool excludSubdirectoryNames,
            ICollection<string> excludedSubdirectoryNames)
        {
            this.SearchPath = searchPath.Trim();
            this.SearchTags = searchTags;
            this.localizationService = localizationService;
            this.messageBoxService = messageBoxService;
            this.CheckUpperLowerCase = checkUpperLowerCase;
            this.CheckAllFiles = checkAllFiles;
            this.ExcludSubdirectoryNames = excludSubdirectoryNames;
            this.ExcludedSubdirectoryNames = excludedSubdirectoryNames;
            this.FileExtensions = checkAllFiles || string.IsNullOrEmpty(fileExtensions)
                ? new List<string>()
                : new List<string>(fileExtensions.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
        }

        public string SearchPath { get; }

        public ICollection<string> SearchTags { get; }

        public bool CheckUpperLowerCase { get; }

        public bool CheckAllFiles { get; }

        public bool ExcludSubdirectoryNames { get; }

        public ICollection<string> ExcludedSubdirectoryNames { get; }

        public ICollection<string> FileExtensions { get; }

        public bool IsNotValid()
        {
            if (this.SearchTags.Count == 0)
            {
                this.messageBoxService.ShowInformation(this.localizationService.GetLocalizedValue("PleaseEnterSearchTermFirst"));
                return true;
            }

            if (this.FileExtensions.Count == 0 && !this.CheckAllFiles)
            {
                this.messageBoxService.ShowInformation(this.localizationService.GetLocalizedValue("PleaseEnterFileExtensionFirst"));
                return true;
            }

            if (this.ExcludedSubdirectoryNames.Count == 0 && this.ExcludSubdirectoryNames)
            {
                this.messageBoxService.ShowInformation(this.localizationService.GetLocalizedValue("PleaseEnterDirectoryNameThatShouldBeIgnored"));
                return true;
            }

            if (!Directory.Exists(this.SearchPath))
            {
                this.messageBoxService.ShowError(this.localizationService.GetLocalizedValue("TheSpecifiedDirectoryDoesNotSeemToExist"));
                return true;
            }

            return false;
        }
    }
}
