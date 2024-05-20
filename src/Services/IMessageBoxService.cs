namespace Services
{
    public interface IMessageBoxService
    {
        void ShowInformation(string text);

        void ShowError(string text);
    }
}
