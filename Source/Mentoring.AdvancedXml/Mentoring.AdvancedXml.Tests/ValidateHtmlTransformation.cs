using Mentoring.AdvancedXml.HtmlTransformation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
using System;

namespace Mentoring.AdvancedXml.Tests
{
    [TestClass]
    public class ValidateHtmlTransformation
    {
        [TestMethod]
        public void TestHtmlTransformation()
        {
            var x = Assembly.GetExecutingAssembly().Location;
            Transformation.GenerateReport(
                Path.GetFullPath("InputFiles/ValidCatalog.xml"),
                Path.GetFullPath("Transformations/HtmlTransformation.xslt"),
                Path.GetFullPath("OutputFiles/Report.html"));
        }
    }
}
