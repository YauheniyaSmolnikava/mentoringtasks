using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Mentoring.ExpressionsIQueryable.ClassMapper
{
    public class MappingGenerator
    {
        public Mapper<TSource, TDestination> Generate<TSource, TDestination>()
        {
            var sourceParam = Expression.Parameter(typeof(TSource));

            var fieldBindings = sourceParam.Type.GetFields()
                    .Select(p => Expression.Bind(typeof(TDestination).GetField(p.Name), Expression.Field(sourceParam, p)));
            var propertiesBindings = sourceParam.Type.GetProperties()
                    .Select(p => Expression.Bind(typeof(TDestination).GetProperty(p.Name), Expression.Property(sourceParam, p)));

            var body = Expression
                .MemberInit(
                    Expression.New(typeof(TDestination)), fieldBindings.Concat(propertiesBindings));

            var mapFunction = Expression.Lambda<Func<TSource, TDestination>>(body, sourceParam);
            return new Mapper<TSource, TDestination>(mapFunction.Compile());
        } 
    }
}
