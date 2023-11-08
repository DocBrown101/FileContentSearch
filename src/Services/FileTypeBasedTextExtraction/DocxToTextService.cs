namespace Services.FileTypeBasedTextExtraction
{
    using System;
    using System.IO;
    using System.IO.Packaging;
    using System.Text;
    using System.Xml;

    public class DocxToTextService : ITextExtraction
    {
        private const string WordprocessingMlNamespace = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        private const string BodyXPath = "/w:document/w:body";

        public string ExtractText(string filePath)
        {
            var content = this.GetTextFromDocX(filePath);

            return RemoveBlankLines(content);
        }

        private static string RemoveBlankLines(string content)
        {
            var stringBuilder = new StringBuilder();
            var splitString = content.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var row in splitString)
            {
                stringBuilder.AppendLine(row);
            }

            return stringBuilder.ToString();
        }

        private string GetTextFromDocX(string filePath)
        {
            var stringBuilder = new StringBuilder();
            var xmlDocument = new XmlDocument();

            try
            {
                using var package = Package.Open(filePath, FileMode.Open, FileAccess.Read);
                using var packageStream = package.GetPart(new Uri("/word/document.xml", UriKind.Relative)).GetStream();

                xmlDocument.Load(packageStream);

                var xmlNamespaceManager = new XmlNamespaceManager(xmlDocument.NameTable);
                xmlNamespaceManager.AddNamespace("w", WordprocessingMlNamespace);
                var xmlNode = xmlDocument.DocumentElement.SelectSingleNode(BodyXPath, xmlNamespaceManager);

                stringBuilder.Append(this.ReadTextFromNodeRecursive(xmlNode));
            }
            catch
            {
                return string.Empty;
            }

            return stringBuilder.ToString();
        }

        private string ReadTextFromNodeRecursive(XmlNode xmlNode)
        {
            if (xmlNode == null || xmlNode.NodeType != XmlNodeType.Element)
            {
                return string.Empty;
            }

            var stringBuilder = new StringBuilder();

            foreach (XmlNode currentXmlNode in xmlNode.ChildNodes)
            {
                if (currentXmlNode.NodeType != XmlNodeType.Element)
                {
                    continue;
                }

                switch (currentXmlNode.LocalName)
                {
                    case "t":   // Text
                        stringBuilder.Append(currentXmlNode.InnerText.TrimEnd());

                        var space = ((XmlElement)currentXmlNode).GetAttribute("xml:space");
                        if (!string.IsNullOrEmpty(space) && space == "preserve")
                        {
                            stringBuilder.Append(' ');
                        }

                        break;

                    case "cr":  // Carriage return
                    case "br":  // Page break
                        stringBuilder.Append(Environment.NewLine);
                        break;

                    case "tab": // Tab
                        stringBuilder.Append('\t');
                        break;

                    case "p":   // Paragraph
                        stringBuilder.Append(this.ReadTextFromNodeRecursive(currentXmlNode));
                        stringBuilder.Append(Environment.NewLine);
                        stringBuilder.Append(Environment.NewLine);
                        break;

                    default:
                        stringBuilder.Append(this.ReadTextFromNodeRecursive(currentXmlNode));
                        break;
                }
            }

            return stringBuilder.ToString();
        }
    }
}
