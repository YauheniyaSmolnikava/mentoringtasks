using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mentoring.ExpressionsIQueryable.ClassMapper
{
    public class Mapper<TSource, TDestination>
    {
        Func<TSource, TDestination> mapFunction;

        internal Mapper(Func<TSource, TDestination> func)
        {
            mapFunction = func;
        }
        public TDestination Map(TSource source)
        {
            return mapFunction(source);
        }
    }
}
