using System.Windows;
using WPFLocalizeExtension.Engine;

namespace FileContentSearch
{
    public partial class App : Application
    {
        public App()
        {
#if DEBUG
            LocalizeDictionary.Instance.SetCurrentThreadCulture = true;
            LocalizeDictionary.Instance.Culture = new System.Globalization.CultureInfo("en");
#else
            LocalizeDictionary.Instance.Culture = System.Threading.Thread.CurrentThread.CurrentCulture;
#endif
        }
    }
}
