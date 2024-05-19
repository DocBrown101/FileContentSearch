namespace ContentSearch.Service
{
    using MsBox.Avalonia.Enums;
    using MsBox.Avalonia;
    using Services;

    public class MessageBoxService : IMessageBoxService
    {
        public void ShowInformation(string text)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(string.Empty, text, ButtonEnum.Ok, Icon.Info);
            _ = box.ShowAsync();
        }

        public void ShowError(string text)
        {
            var box = MessageBoxManager.GetMessageBoxStandard(string.Empty, text, ButtonEnum.Ok, Icon.Error);
            _ = box.ShowAsync();
        }
    }
}
