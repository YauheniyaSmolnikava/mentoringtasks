using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Mentoring.ExpressionsIQueryable.ExpressionTransform.Tests
{
    [TestClass]
    public class ExpressionTransformTest
    {
        [TestMethod]
        public void TestTransform()
        {
            Dictionary<string, int> dict = new Dictionary<string, int>();
            dict.Add("a", 2);
            dict.Add("b", 10);

            Expression<Func<int, int, int>> source_exp = (a, b) => a + (a + 1) * (a + 5) * (b - 1);

            var expr_transform = new ExpressionTransform();

            var result_exp = expr_transform.VisitAndConvert(source_exp, "", dict);

            Console.WriteLine(source_exp + " " + source_exp.Compile().Invoke(2, 10));
            Console.WriteLine(result_exp + " " + result_exp.Compile().Invoke(2, 10));
        }
    }
}
