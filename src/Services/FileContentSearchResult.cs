namespace Services
{
    using System;
    using System.IO;

    public class FileContentSearchResult
    {
        private readonly ILocalizationService localizationService;
        private readonly FileInfo fileInfo;
        private readonly string rootSearchPath;
        private readonly bool hasAnyFileReadError;

        public FileContentSearchResult(ILocalizationService localizationService, FileInfo fileInfo, string rootSearchPath, bool hasAnyFileReadError)
        {
            this.localizationService = localizationService;
            this.fileInfo = fileInfo ?? throw new ArgumentNullException(nameof(fileInfo));
            this.rootSearchPath = rootSearchPath;
            this.hasAnyFileReadError = hasAnyFileReadError;

            this.FileName = fileInfo.Name;
            this.FileFullName = fileInfo.FullName;
            this.CreationTime = fileInfo.CreationTime;
            this.LastWriteTime = fileInfo.LastWriteTime;
            this.ResultFileName = this.GetResultFileName();
            this.ResultFilePath = this.ResultFileName.Replace(this.FileName, string.Empty);
        }

        public string ResultFileName { get; }

        public string ResultFilePath { get; }

        public string FileName { get; }

        public string FileFullName { get; }

        public DateTime CreationTime { get; }

        public DateTime LastWriteTime { get; }

        private string GetResultFileName()
        {
            var substringLength = this.rootSearchPath.Length > 3 ? this.rootSearchPath.Length : 2;
            var file = this.fileInfo.FullName;

            return this.hasAnyFileReadError
                       ? $"..{file.Substring(substringLength)} {this.localizationService.GetLocalizedValue("CouldNotBeRead")}"
                       : $"..{file.Substring(substringLength)}";
        }
    }
}
