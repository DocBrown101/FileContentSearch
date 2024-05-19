namespace ContentSearch.Service
{
    using System.Reflection;

    public class WindowTitleService
    {
        public static string GetMainWindowTitle()
        {
            //var assemblyName = Process.GetCurrentProcess().MainModule.FileName;
            //FileVersionInfo fvi = FileVersionInfo.GetVersionInfo(assemblyName);
            //AppVersion = "v" + fvi.ProductVersion;

            return IsDebugSession() == true ? "File-Content-Search | DEBUG" : $"File-Content-Search | {GetApplicationVersion()}";
        }

        private static string GetApplicationVersion()
        {
            var assembly = Assembly.GetEntryAssembly();
            var version = assembly?.GetName().Version;

            return version?.ToString() ?? "Unknown";
        }

        private static bool IsDebugSession()
        {
#if DEBUG
            return true;
#else
            return false;
#endif
        }
    }
}
