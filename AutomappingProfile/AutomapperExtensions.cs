using AutoMapper;

namespace Automapper
{
    public static class AutoMapperExtensions
    {
        public static void IgnoreSourceWhenDefault<TSource, TDestination>(this IMemberConfigurationExpression<TSource, TDestination, object> opt)
        {
            opt.Condition((src, dest, srcValue) =>
            {
                var propertyInfo = src.GetType().GetProperty(opt.DestinationMember.Name);
                var srcPropValue = propertyInfo.GetValue(src, null);
                bool isNull = srcPropValue == null;

                bool useSource = !isNull;
                return useSource;
            });
        }
    }
}
