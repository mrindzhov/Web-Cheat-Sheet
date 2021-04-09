using Extensions;
using Newtonsoft.Json;
using Reinforced.Typings.Ast.TypeNames;
using Reinforced.Typings.Fluent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CodeGeneration
{
    public static class TsConfigurationBuilderExtensions
    {
        private static readonly JsonSerializerSettings opts = new JsonSerializerSettings() { ReferenceLoopHandling = ReferenceLoopHandling.Ignore };

        public static IEnumerable<Type> ProjectAssembliesTypes { get; private set; }

        public static ConfigurationBuilder GenerateEnumsWithDescriptions(this ConfigurationBuilder builder, string rootNamespace, Type[] enums = null)
        {
            enums ??= builder.LoadTypesInNamespace(rootNamespace);

            builder.ExportAsEnums(enums, conf => conf
                .WithCodeGenerator<EnumerationGenerator>()
                .OverrideNamespace("@models/enums")
            );

            return builder;
        }

        public static ConfigurationBuilder GenerateViewModels(this ConfigurationBuilder builder, string rootNamespace, Type[] viewModels = null)
        {
            viewModels ??= builder.LoadTypesInNamespace(rootNamespace);

            builder.ExportAsClasses(viewModels, conf => conf
                .FlattenHierarchy()
                .WithPublicProperties()
                .OverrideNamespace(conf.Type.Namespace.Replace(rootNamespace, "@models"))
                .WithCodeGenerator<ViewModelGenerator>()
            );

            return builder;
        }

        public static ConfigurationBuilder ConfigureExporter(this ConfigurationBuilder builder)
        {
            builder.Global(s => s.UseModules().AutoOptionalProperties().CamelCaseForProperties());

            builder.Substitute(typeof(DateTime), new RtSimpleTypeName("Date"));
            builder.Substitute(typeof(Guid), new RtSimpleTypeName("string"));

            return builder;
        }

        public static Type[] LoadTypesInNamespace(this ConfigurationBuilder builder, string @namespace)
        {
            try
            {
                ProjectAssembliesTypes ??= builder.Context.SourceAssemblies.GetProjectAsemblies().SelectMany(a => a.GetExportedTypes());
                return ProjectAssembliesTypes
                    .Where(t => t.IsPublic && t.Namespace != null && t.Namespace.StartsWith(@namespace))
                    .ToArray();
            }
            catch (Exception e)
            {
                LogAnything(e, "exceptions");
                throw;
            }
        }

        public static void LogAnything(object obj, string name)
        {
            var path = @$".\logs-{name}.json";
            File.WriteAllText(path, JsonConvert.SerializeObject(obj, opts));
        }
    }
}
