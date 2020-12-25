namespace FileContentSearch.Service
{
    using WPFLocalizeExtension.Engine;

    public static class LocalizationExtension
    {
        public static string GetLocalizedValue(this string self)
        {
            return LocalizeDictionary.Instance.GetLocalizedObject(self, null, LocalizeDictionary.Instance.Culture).ToString();
        }
    }
}
