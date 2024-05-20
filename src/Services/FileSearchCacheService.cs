namespace Services
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    public class FileSearchCacheService
    {
        private FileContentSearchOptions? cachedOptions;

        public ReadOnlyCollection<string> Files { get; private set; }

        public FileSearchCacheService()
        {
            this.Files = new ReadOnlyCollection<string>([]);
        }

        public void SetCache(List<string> filesCorrespondingToTheOptions, FileContentSearchOptions options)
        {
            this.Files = new ReadOnlyCollection<string>(filesCorrespondingToTheOptions);
            this.cachedOptions = options;
        }

        public bool CanCachedFilesBeUsed(FileContentSearchOptions currentOptions)
        {
            if (currentOptions == null)
            {
                throw new ArgumentNullException(nameof(currentOptions));
            }

            if (this.Files == null || this.Files.Count == 0 || this.cachedOptions == null)
            {
                return false;
            }

            if (this.cachedOptions.SearchPath != currentOptions.SearchPath)
            {
                return false;
            }

            if (this.cachedOptions.CheckAllFiles != currentOptions.CheckAllFiles)
            {
                return false;
            }

            if (this.cachedOptions.ExcludSubdirectoryNames != currentOptions.ExcludSubdirectoryNames)
            {
                return false;
            }

            var list1 = this.cachedOptions.ExcludedSubdirectoryNames.ToList();
            var list2 = currentOptions.ExcludedSubdirectoryNames.ToList();
            if (!CompareListWith(list1, list2))
            {
                return false;
            }

            var oldExtensions = this.cachedOptions.FileExtensions;
            var newExtensions = currentOptions.FileExtensions;
            var anyRemoved = oldExtensions.Any(x => newExtensions.All(y => y != x));
            var anyAdded = newExtensions.Any(x => oldExtensions.All(y => y != x));

            return !anyRemoved && !anyAdded;
        }

        private static bool CompareListWith(IList<string> list1, IList<string> list2)
        {
            if (list1 == null || list2 == null)
            {
                return false;
            }

            if (list1.Count != list2.Count)
            {
                return false;
            }

            return !list1.Where((t, i) => t != list2[i]).Any();
        }
    }
}
