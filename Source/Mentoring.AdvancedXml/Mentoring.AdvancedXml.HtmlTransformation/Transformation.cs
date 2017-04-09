using System.Xml.Xsl;
using System.IO;
using System;

namespace Mentoring.AdvancedXml.HtmlTransformation
{
    public static class Transformation
    {
        public static void GenerateReport(string inputfilePath, string xsltFilePath, string outputFilePath)
        {
            var xsl = new XslCompiledTransform();
            xsl.Load(xsltFilePath, new XsltSettings(false, true), null);

            using (StreamWriter file = new StreamWriter(outputFilePath))
            {
                xsl.Transform(inputfilePath, null, file);
            }
        }
    }
}
