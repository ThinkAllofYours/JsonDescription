using System;
using System.IO;
using System.Text;
using Xamasoft.JsonClassGenerator;

namespace DescriptJson.CodeWriters_to_Str
{
    public class TypeScriptCodeWriterToText : ICodeWriter
    {
        public string FileExtension
        {
            get { return ".ts"; }
        }


        public string DisplayName
        {
            get { return "TypeScript"; }
        }


        public string GetTypeName(JsonType type, IJsonClassGeneratorConfig config)
        {
            switch (type.Type)
            {
                case JsonTypeEnum.Anything: return "any";
                case JsonTypeEnum.String: return "string";
                case JsonTypeEnum.Boolean: return "bool";
                case JsonTypeEnum.Integer:
                case JsonTypeEnum.Long:
                case JsonTypeEnum.Float: return "number";
                case JsonTypeEnum.Date: return "Date";
                case JsonTypeEnum.NullableInteger:
                case JsonTypeEnum.NullableLong:
                case JsonTypeEnum.NullableFloat: return "number";
                case JsonTypeEnum.NullableBoolean: return "bool";
                case JsonTypeEnum.NullableDate: return "Date";
                case JsonTypeEnum.Object: return type.AssignedName;
                case JsonTypeEnum.Array: return GetTypeName(type.InternalType, config) + "[]";
                case JsonTypeEnum.Dictionary: return "{ [key: string]: " + GetTypeName(type.InternalType, config) + "; }";
                case JsonTypeEnum.NullableSomething: return "any";
                case JsonTypeEnum.NonConstrained: return "any";
                default: throw new NotSupportedException("Unsupported type");
            }
        }


        private bool IsNullable(JsonTypeEnum type)
        {
            return
                type == JsonTypeEnum.NullableBoolean ||
                type == JsonTypeEnum.NullableDate ||
                type == JsonTypeEnum.NullableFloat ||
                type == JsonTypeEnum.NullableInteger ||
                type == JsonTypeEnum.NullableLong ||
                type == JsonTypeEnum.NullableSomething;
        }


        private string GetNamespace(IJsonClassGeneratorConfig config, bool root)
        {
            return root ? config.Namespace : (config.SecondaryNamespace ?? config.Namespace);
        }


        public void WriteClass(IJsonClassGeneratorConfig config, StringBuilder sb, JsonType type)
        {
            var prefix = GetNamespace(config, type.IsRoot) != null ? "    " : "";
            var exported = !config.InternalVisibility || config.SecondaryNamespace != null;

            sb.AppendLine(prefix + "interface "
                                 + type.AssignedName + " {");

            int cnt = 1;
            foreach (var field in type.Fields)
            {
                var shouldDefineNamespace = type.IsRoot && config.SecondaryNamespace != null && config.Namespace != null && (field.Type.Type == JsonTypeEnum.Object || (field.Type.InternalType != null && field.Type.InternalType.Type == JsonTypeEnum.Object));
                if (config.ExamplesInDocumentation)
                {
                    sb.AppendLine(prefix + "    /**")
                      .AppendLine(prefix + "      // Examples: " + field.GetExamplesText())
                      .AppendLine(prefix + "      */");
                }
                sb.AppendLine(prefix + "    " + field.JsonMemberName + (IsNullable(field.Type.Type) ? "?" : "") + ": " + (shouldDefineNamespace ? config.SecondaryNamespace + "." : string.Empty) + GetTypeName(field.Type, config) + ";");
                if (type.Fields.Count != cnt++) sb.AppendLine();
            }
            sb.AppendLine(prefix + "}")
              .AppendLine();
        }


        public void WriteNamespaceStart(IJsonClassGeneratorConfig config, StringBuilder sb, bool root)
        {
            if (GetNamespace(config, root) != null)
            {
                sb.AppendLine("module " + GetNamespace(config, root) + " {")
                  .AppendLine();
            }
        }


        public void WriteNamespaceEnd(IJsonClassGeneratorConfig config, StringBuilder sb, bool root)
        {
            if (GetNamespace(config, root) != null)
            {
                sb.AppendLine("}")
                  .AppendLine();
            }
        }

        #region "not implement"

        public void WriteFileStart(IJsonClassGeneratorConfig config, StringBuilder sb) {}


        public void WriteClass(IJsonClassGeneratorConfig config, TextWriter sw, JsonType type){}


        public void WriteFileEnd(IJsonClassGeneratorConfig config, StringBuilder sb){}


        public void WriteFileStart(IJsonClassGeneratorConfig config, TextWriter sw){}


        public void WriteFileEnd(IJsonClassGeneratorConfig config, TextWriter sw){}


        public void WriteNamespaceStart(IJsonClassGeneratorConfig config, TextWriter sw, bool root){}


        public void WriteNamespaceEnd(IJsonClassGeneratorConfig config, TextWriter sw, bool root){}

        #endregion

    }
}
