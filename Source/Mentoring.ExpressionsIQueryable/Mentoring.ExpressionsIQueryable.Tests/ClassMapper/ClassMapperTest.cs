using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mentoring.ExpressionsIQueryable.Tests.ClassMapper.TestClasses;
using Mentoring.ExpressionsIQueryable.ClassMapper;
using System;

namespace Mentoring.ExpressionsIQueryable.Tests.ClassMapper
{
    [TestClass]
    public class ClassMapperTest
    {
        [TestMethod]
        public void TestMapping()
        {
            var mapGenerator = new MappingGenerator();
            var mapper = mapGenerator.Generate<Foo, Bar>();

            var fooObj = new Foo() { Name = "FooName", LastName = "FooLastName", Age = 25, DayOfWeek = 1, Month = "January" };
            var barObj = mapper.Map(fooObj);

            Console.WriteLine(String.Format("Foo: Name = {0}, LastName = {1}, Age = {2}, Address = {3}, DayOfWeek = {4}, Month = {5}", 
                fooObj.Name, fooObj.LastName, fooObj.Age, fooObj.Address, fooObj.DayOfWeek, fooObj.Month));
            Console.WriteLine(String.Format("Bar: Name = {0}, MiddleName = {1}, PropValue = {2}, Address = {3}, DayOfWeek = {4}, Month = {5}", 
                barObj.Name, barObj.MiddleName, barObj.PropValue, barObj.Address, barObj.DayOfWeek, barObj.Month));

        }
    }
}
