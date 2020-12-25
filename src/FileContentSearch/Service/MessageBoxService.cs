namespace FileContentSearch.Service
{
    using System.Windows;
    using Services;

    public class MessageBoxService : IMessageBoxService
    {
        public void ShowInformation(string text)
        {
            MessageBox.Show(text, string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowError(string text)
        {
            MessageBox.Show(text, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
