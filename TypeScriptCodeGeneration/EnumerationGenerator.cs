
using Humanizer;
using Reinforced.Typings;
using Reinforced.Typings.Ast;
using Reinforced.Typings.Generators;
using System;
using System.Linq;
using System.Text;

namespace CodeGeneration
{
    public class EnumerationGenerator : EnumGenerator
    {
        public override RtEnum GenerateNode(Type element, RtEnum result, TypeResolver resolver)
        {
            var resultEnum = base.GenerateNode(element, result, resolver);
            resultEnum.Export = true;
            if (Context.Location.CurrentNamespace != null)
            {
                Context.Location.CurrentNamespace.CompilationUnits.Add(resultEnum);

                var enumDescriptionsObject = GenerateEnumDescription(element, resultEnum);
                Context.Location.CurrentNamespace.CompilationUnits.Add(enumDescriptionsObject);
            }

            return null;
        }

        private RtRaw GenerateEnumDescription(Type element, RtEnum resultEnum)
        {
            var enumDescription = new StringBuilder();
            enumDescription.AppendLine();

            var enumName = resultEnum.EnumName;

            var tsType = $"{{[key in {enumName}]: {{long: string, short: string}}}}";
            enumDescription.AppendLine($"export const {enumName}Descriptions: {tsType} = {{");

            foreach (var resultEnumValue in resultEnum.Values)
            {
                var descriptionObj = GetElementDescription(element, resultEnumValue.EnumValueName);
                enumDescription.AppendLine($"\t[{enumName}.{resultEnumValue.EnumValueName}]: {descriptionObj},");
            }

            enumDescription.AppendLine("};");

            var item = new RtRaw(enumDescription.ToString());
            return item;
        }

        private string GetElementDescription(Type element, string enumValueName)
        {
            var member = element.GetMember(enumValueName).FirstOrDefault();
            var descriptionAttr = member.GetCustomAttributes(typeof(LookupDescriptionAttribute), false).FirstOrDefault();

            return descriptionAttr is LookupDescriptionAttribute attribute
                ? MapToJSObject(enumValueName, attribute.LongDescription, attribute.ShortDescription)
                : MapToJSObject(enumValueName);
        }

        private static string MapToJSObject(string enumValue, string longDescription = null, string shortDescription = null)
        {
            var longDesc = longDescription ?? enumValue.Humanize();
            var shortDesc = $"'{shortDescription}'";
            return $"{{long: '{longDesc}', short: {shortDesc}}}";
        }
    }
}