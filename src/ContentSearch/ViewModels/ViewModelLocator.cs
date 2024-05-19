namespace ContentSearch.ViewModels
{
    using Avalonia.Controls;

    public sealed class ViewModelLocator
    {
        public TopLevel? TopLevel { get; private set; }

        private static readonly ViewModelLocator instance = new();

        static ViewModelLocator()
        { }

        private ViewModelLocator()
        { }

        public static ViewModelLocator Instance => instance;

        public void SetTopLevel(Window mainWindow)
        {
            this.TopLevel = TopLevel.GetTopLevel(mainWindow);
        }
    }
}
