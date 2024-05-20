namespace ContentSearch
{
    public static class LocalizationExtension
    {
        public static string GetLocalizedValue(this string self)
        {
            return Lang.Resources.ResourceManager.GetString(self) ?? "unknown";
        }
    }
}
