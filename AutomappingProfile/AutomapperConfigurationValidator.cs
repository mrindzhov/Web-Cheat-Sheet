using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Automapper
{
    public static class AutomapperConfigurationValidator
    {
        public static void ValidateMappings(AppDomain currentDomain, bool validatePropertyMaps = true)
        {
            var mapperConfiguration = GetMapperConfiguration(currentDomain, validatePropertyMaps);
            mapperConfiguration.AssertConfigurationIsValid();
        }

        private static MapperConfiguration GetMapperConfiguration(AppDomain appDomain, bool validatePropertyMaps = true)
        {
            var errors = new List<string>();
            var profiles = appDomain.GetSmarTestAssemblies()
                .SelectMany(a => a.GetTypes()
                    .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(Profile)))
                    .Select(type => (Profile)Activator.CreateInstance(type)))
                .Select(p =>
                {
                    if (validatePropertyMaps) p.ForAllMaps((typeMap, mappingExpr) => ValidatePropertyTypes(typeMap, errors));
                    return p;
                }).ToList();
            var mapperConfiguration = new MapperConfiguration(cfg => cfg.AddProfiles(profiles));

            if (errors.Count > 0) throw new Exception(string.Join("\n", errors));

            return mapperConfiguration;
        }


        public static void ValidatePropertyTypes(TypeMap typeMap, List<string> errors)
        {
            foreach (var map in typeMap.PropertyMaps)
            {
                var sourcePropInfo = map.SourceMember as PropertyInfo;
                var destinationPropInfo = map.DestinationMember as PropertyInfo;
                if (sourcePropInfo == null || destinationPropInfo == null) continue;

                if (sourcePropInfo.PropertyType != destinationPropInfo.PropertyType)
                {
                    var underlyingDestinationType = Nullable.GetUnderlyingType(destinationPropInfo.PropertyType);
                    if (underlyingDestinationType != null)
                        if (underlyingDestinationType == sourcePropInfo.PropertyType) continue;

                    var underlyingSourceType = Nullable.GetUnderlyingType(sourcePropInfo.PropertyType);
                    if (underlyingSourceType != null)
                        if (underlyingSourceType == destinationPropInfo.PropertyType) continue;

                    errors.Add($"Invalid type of `{map.DestinationName}` property in mapping for `{typeMap.SourceType.Name} -> {typeMap.DestinationType.Name}`!");
                }
            }
        }
    }
}