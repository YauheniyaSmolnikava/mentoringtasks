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
        public void WithoutProvider()
        {
            var client = new E3SQueryClient(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);
            var res = client.SearchFTS<EmployeeEntity>("workstation:(EPRUIZHW0249)", 0, 1);

            foreach (var emp in res)
            {
                Console.WriteLine("{0} {1}", emp.nativename, emp.startworkdate);
            }
        }

        [TestMethod]
        public void WithoutProviderNonGeneric()
        {
            var client = new E3SQueryClient(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);
            var res = client.SearchFTS(typeof(EmployeeEntity), "workstation:(EPRUIZHW0249)", 0, 10);

            foreach (var emp in res.OfType<EmployeeEntity>())
            {
                Console.WriteLine("{0} {1}", emp.nativename, emp.startworkdate);
            }
        }

        [TestMethod]
        public void WithProvider()
        {
            var employees = new E3SEntitySet<EmployeeEntity>(ConfigurationManager.AppSettings["user"], ConfigurationManager.AppSettings["password"]);

            foreach (var emp in employees.Where(e => e.workstation == "EPRUIZHW0249" && e.nativename.Contains("Михаил")))
            {
                Console.WriteLine("{0} {1}", emp.nativename, emp.startworkdate);
            }
        }
    }
}
