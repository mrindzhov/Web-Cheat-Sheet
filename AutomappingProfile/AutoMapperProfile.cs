using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Automapper
{
    public class AutoMappingProfile : Profile
    {
        public AutoMappingProfile()
        {
            var types = AppDomain.CurrentDomain.GetSmarTestAssemblies()
                .SelectMany(a => a.GetTypes().Where(t => t.IsClass && t.IsPublic && !t.IsAbstract))
                .ToList();

            RegisterAttributeMappings<TwoWayMapAttribute>(types, ApplyTwoWayMapping);
            RegisterAttributeMappings<ReverseMapAttribute>(types, ApplyReverseMapping);
            RegisterAttributeMappings<MapFromAttribute>(types, ApplyMapFromMapping);
            RegisterAttributeMappings<MapToAttribute>(types, ApplyMapToMapping);
            RegisterInterfaceMappings(types);
        }

        private void RegisterAttributeMappings<TMapAttribute>(List<Type> types, Action<Type, Type> applyMapping)
            where TMapAttribute : BaseMapAttribute
        {
            var typesWithAttribute = types.Where(t => t.GetCustomAttribute(typeof(TMapAttribute), false) != null);
            foreach (var type in typesWithAttribute)
            {
                var attr = type.GetCustomAttribute<TMapAttribute>(true);
                applyMapping(type, attr.MapType);
            }
        }

        private void RegisterInterfaceMappings(List<Type> types)
        {
            var typesWithCustomInterface = types.Where(t => t.GetInterface(nameof(IMapExplicitly)) != null).ToList();

            foreach (var type in typesWithCustomInterface)
            {
                var explicitMapper = (IMapExplicitly)Activator.CreateInstance(type);
                explicitMapper?.ConfigureMapping(this);
            }
        }

        private void ApplyMapToMapping(Type currentType, Type mappingType) =>
            CreateMap(currentType, mappingType, MemberList.Source)
                .ForAllOtherMembers(opt => opt.IgnoreSourceWhenDefault());

        private void ApplyMapFromMapping(Type currentType, Type mappingType) => CreateMap(mappingType, currentType, MemberList.Destination);
        private void ApplyReverseMapping(Type currentType, Type mappingType) =>
            CreateMap(mappingType, currentType, MemberList.Destination)
                .ReverseMap()
                .ForAllOtherMembers(opt => opt.IgnoreSourceWhenDefault());

        private void ApplyTwoWayMapping(Type currentType, Type mappingType)
        {
            CreateMap(mappingType, currentType, MemberList.Destination);
            CreateMap(currentType, mappingType, MemberList.None).ForAllOtherMembers(opt => opt.IgnoreSourceWhenDefault());
        }
    }
}
