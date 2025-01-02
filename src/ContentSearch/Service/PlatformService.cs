namespace ContentSearch.Service
{
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using ContentSearch.ViewModels;

    public class PlatformService
    {
        public static void OpenSelectedResult(ResultViewModel? resultViewModel)
        {
            if (resultViewModel != null)
            {
                var fileInfo = new FileInfo(resultViewModel.Result.FileFullName);
                var isWindowsOs = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
                var fileOrDirectoryName = fileInfo.Exists ? fileInfo.FullName : fileInfo.DirectoryName;

                if (isWindowsOs)
                {
                    var args = $"/e, /select, \"{fileOrDirectoryName}\"";

                    _ = Process.Start("explorer", args);
                }
                else
                {
                    _ = Process.Start("xdg-open", $"\"{fileOrDirectoryName}\"");
                }
            }
        }
    }
}
