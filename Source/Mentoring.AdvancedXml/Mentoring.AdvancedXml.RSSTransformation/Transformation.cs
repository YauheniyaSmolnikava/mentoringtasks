using System.Xml.Xsl;
using System;

namespace Mentoring.AdvancedXml.RSSTransformation
{
    public static class Transformation
    {
        public static void XslCompiledTransform(string filePath)
        {
            var xsl = new XslCompiledTransform();
            xsl.Load("RSSTransformation.xslt");

            xsl.Transform(filePath, null, Console.Out);
        }
    }
}
