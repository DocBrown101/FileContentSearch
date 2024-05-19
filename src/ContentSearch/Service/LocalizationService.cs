namespace ContentSearch.Service
{
    using Services;

    public class LocalizationService : ILocalizationService
    {
        public string GetLocalizedValue(string key)
        {
            return key.GetLocalizedValue();
        }
    }
}
