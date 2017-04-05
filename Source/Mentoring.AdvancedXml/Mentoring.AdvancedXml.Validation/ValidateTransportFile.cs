using System.Xml;
using System.Xml.Schema;
using System.Collections.Generic;

namespace Mentoring.AdvancedXml.Validation
{
    public class ValidateTransportFile
    {
        XmlReaderSettings settings;
        List<string> errors;

        public ValidateTransportFile()
        {
            settings = new XmlReaderSettings();

            settings.Schemas.Add("http://library.by/catalog", "CatalogSchema.xsd");
            settings.ValidationEventHandler +=
                delegate (object sender, ValidationEventArgs e)
                {
                    errors.Add(string.Format("[{0}:{1}] {2}", e.Exception.LineNumber, e.Exception.LinePosition, e.Message));
                };

            settings.ValidationFlags = settings.ValidationFlags | XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationType = ValidationType.Schema;
        }

        public List<string> Validate(string filePath)
        {
            errors = new List<string>();

            XmlReader reader = XmlReader.Create("CDCatalog1.xml", settings);

            while (reader.Read()) ;

            return errors;
        }
    }
}
