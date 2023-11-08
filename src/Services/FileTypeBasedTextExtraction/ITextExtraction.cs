namespace Services.FileTypeBasedTextExtraction
{
    public interface ITextExtraction
    {
        string ExtractText(string filePath);
    }
}