using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mentoring.ExpressionsIQueryable.E3SLinqProvider.E3SClient.Entities;
using Mentoring.ExpressionsIQueryable.E3SLinqProvider.E3SClient;
using System.Linq;
using System.Configuration;

namespace Mentoring.ExpressionsIQueryable.Tests.E3SProvider
{
    [TestClass]
    public class E3SProviderTest
    {
        [TestMethod]
        public void WithProviderAndContains()
        {
            var employees = new E3SLinqProvider.E3SEntitySet<EmployeeEntity>(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);

            foreach (var emp in employees.Where(e => e.workstation == "EPRUIZHW0249" && e.nativename.Contains("Михаил")))
            {
                Console.WriteLine("{0} {1}", emp.nativename, emp.startworkdate);
            }
        }

        [TestMethod]
        public void WithProviderAndStartsWithEnsWith()
        {
            var employees = new E3SLinqProvider.E3SEntitySet<EmployeeEntity>(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);

            foreach (var emp in employees.Where(e => e.workstation.StartsWith("EPRUIZHW02") && e.nativename.EndsWith("Романов")))
            {
                Console.WriteLine("{0} {1}", emp.nativename, emp.startworkdate);
            }
        }
    }
}
