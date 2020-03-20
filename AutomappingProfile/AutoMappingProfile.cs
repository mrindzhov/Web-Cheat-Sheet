using AutoMapper;
using System;
using System.Linq;
using System.Reflection;

namespace AutoMapper
{
    public class AutoMappingProfile : Profile
    {
        public AutoMappingProfile()
        {
            var mapFromType = typeof(IMapFrom<>);
            var mapToType = typeof(IMapTo<>);
            var mapExplicitlyType = typeof(IMapExplicitly);

            var solutionAssemblyName = Assembly
                .GetExecutingAssembly()
                .GetName()
                .Name; 

            var mappings = AppDomain.CurrentDomain
                .GetAssemblies()
                .Where(a => a.GetName().Name.StartsWith(solutionAssemblyName))
                .SelectMany(a => a.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract)
                .Select(t => new
                {
                    Type = t,
                    MapFrom = this.GetMappingModel(t, mapFromType),
                    MapTo = this.GetMappingModel(t, mapToType),
                    MapExplicitly = t
                        .GetInterfaces()
                        .Where(i => i == mapExplicitlyType)
                        .Select(i => (IMapExplicitly)Activator.CreateInstance(t))
                        .FirstOrDefault()
                });

            foreach (var mapping in mappings)
            {
                if (mapping.MapFrom != null)
                {
                    this.CreateMap(mapping.MapFrom, mapping.Type);
                }

                if (mapping.MapTo != null)
                {
                    this.CreateMap(mapping.Type, mapping.MapTo);
                }

                mapping.MapExplicitly?.ConfigureMapping(this);
            }
        }

        private Type GetMappingModel(Type type, Type mapType)
            => type
            .GetInterfaces()
            .FirstOrDefault(i =>
                i.IsGenericType
                && i.GetGenericTypeDefinition() == mapType)
            ?.GetGenericArguments()
            .First();
    }
}
