using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Mentoring.AdvancedXml.Validation;


namespace Mentoring.AdvancedXml.Tests
{
    [TestClass]
    public class ValidateTransportFileTest
    {
        ValidateTransportFile validator;

        [TestInitialize]
        public void Init()
        {
            validator = new ValidateTransportFile();
        }

        [TestMethod]
        public void TestValidation_ValidFile()
        {
            validator.Validate("InputFiles/ValidCatalog.xml");
        }

        [TestMethod]
        public void TestValidation_InvalidFile()
        {
            validator.Validate("InputFiles/InvalidCatalog.xml");

            foreach(var error in validator.errors)
            {
                Console.WriteLine(error);
            }
        }
    }
}
