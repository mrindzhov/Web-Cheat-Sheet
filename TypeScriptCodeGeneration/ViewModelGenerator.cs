using Reinforced.Typings;
using Reinforced.Typings.Ast;
using Reinforced.Typings.Ast.TypeNames;
using Reinforced.Typings.Generators;
using System;
using System.Linq;
using System.Text;

namespace CodeGeneration
{
    public class ViewModelGenerator : ClassCodeGenerator
    {
        public override RtClass GenerateNode(Type element, RtClass result, TypeResolver resolver)
        {
            result = base.GenerateNode(element, result, resolver);

            try
            {
                AddViewModelToNode(element, result);
            }
            catch (Exception e)
            {
                TsConfigurationBuilderExtensions.LogAnything(e, "e");
                throw;
            }

            return result;
        }

        private void AddViewModelToNode(Type element, RtClass result)
        {
            var collectionName = new RtRaw($"static Name = '{result.Name.TypeName}';");
            result.Members.Insert(0, collectionName);

            var initializationMethod = GenerateInitializationMethod(element, result);
            result.Members.Add(initializationMethod);
        }

        private RtRaw GenerateInitializationMethod(Type element, RtClass result)
        {
            var initializationMethod = GenerateInitializationMethodText(element, result);
            return new RtRaw(initializationMethod);

            string GenerateInitializationMethodText(Type element, RtClass result)
            {
                var initializeMethod = new StringBuilder();
                initializeMethod.AppendLine();
                initializeMethod.AppendLine($"static init = (): {result.Name.TypeName} => ({{");

                var fields = result.Members.Where(m => m is RtField).Select(m => m as RtField);
                var properties = element.GetProperties();

                foreach (var field in fields)
                {
                    if (field.Type is RtSimpleTypeName fieldType)
                    {
                        var propertyType = properties
                            .First(p => p.Name.ToLower() == field.Identifier.IdentifierName.ToLower())
                            .PropertyType;

                        var defaultType = fieldType.TypeName switch
                        {
                            var type when type == "number" => "-1",
                            var type when type == "string" => "''",
                            var type when type == "boolean" => "false",
                            var type when type == "Date" => "new Date()",
                            _ => GetDefaultForComplexType(propertyType),
                        };

                        if (defaultType != null)
                            initializeMethod.AppendLine($"\t {field.Identifier.IdentifierName}: {defaultType},");
                    }
                }
                initializeMethod.AppendLine("});");

                return initializeMethod.ToString();
            }
        }

        private string GetDefaultForComplexType(Type propertyType)
        {
            var prop = propertyType.IsNullable()
                ? Nullable.GetUnderlyingType(propertyType)
                : propertyType;

            return prop.IsEnum
                ? $"{prop.Name}.{prop.GetEnumValues().GetValue(0).ToString()}"
                : prop.IsClass
                    ? "{}"
                    : null;
        }

    }
}
