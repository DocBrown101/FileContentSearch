namespace FileContentSearch.Service
{
    public class WindowTitleService
    {
        public static string GetMainWindowTitle()
        {
            return IsDebugSession() == true ? "File-Content-Search | DEBUG" : $"File-Content-Search | {System.Windows.Forms.Application.ProductVersion}";
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
