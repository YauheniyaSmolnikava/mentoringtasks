using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mentoring.ExpressionsIQueryable.ClassMapper;
using Mentoring.ExpressionsIQueryable.Tests.ExpressionTransform.TestClasses;

namespace Mentoring.ExpressionsIQueryable.Tests.ExpressionTransform
{
    [TestClass]
    public class ClassMapperTest
    {
        [TestMethod]
        public void TestMapping()
        {
            var mapGenerator = new MappingGenerator();
            var mapper = mapGenerator.Generate<Foo, Bar>();
            var res = mapper.Map(new Foo() { Name = "FooName" }); 
        }
    }
}
