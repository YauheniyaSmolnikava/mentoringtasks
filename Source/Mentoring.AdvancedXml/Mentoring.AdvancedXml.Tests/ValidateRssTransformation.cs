using Mentoring.AdvancedXml.RSSTransformation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

namespace Mentoring.AdvancedXml.Tests
{
    [TestClass]
    public class ValidateRssTransformation
    {
        [TestMethod]
        public void TestRssTransformation()
        {
            Transformation.XslCompiledTransform(Path.GetFullPath("InputFiles/ValidCatalog.xml"));
        }
    }
}
