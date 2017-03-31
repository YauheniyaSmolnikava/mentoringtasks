using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Mentoring.ExpressionsIQueryable.ClassMapper
{
    public class MappingGenerator
    {
        public Mapper<TSource, TDestination> Generate<TSource, TDestination>()
        {
            var sourceParam = Expression.Parameter(typeof(TSource));

            // Bindings for fields that have the same names and types
            var fieldBindings = sourceParam.Type
                .GetFields()
                .Where(x => typeof(TDestination).GetField(x.Name) != null 
                && typeof(TDestination).GetField(x.Name).FieldType == x.FieldType)
                .Select(p => Expression.Bind(typeof(TDestination).GetField(p.Name), Expression.Field(sourceParam, p)));

            // Bindings for properties that have the same names and types
            var propertiesBindings = sourceParam.Type
                .GetProperties()
                .Where(x => typeof(TDestination).GetProperty(x.Name) != null 
                && typeof(TDestination).GetProperty(x.Name).PropertyType == x.PropertyType)
                .Select(p => Expression.Bind(typeof(TDestination).GetProperty(p.Name), Expression.Property(sourceParam, p)));

            var body = Expression
                .MemberInit(
                    Expression.New(typeof(TDestination)), fieldBindings.Concat(propertiesBindings));

            var mapFunction = Expression.Lambda<Func<TSource, TDestination>>(body, sourceParam);

            return new Mapper<TSource, TDestination>(mapFunction.Compile());
        }
    }
}
