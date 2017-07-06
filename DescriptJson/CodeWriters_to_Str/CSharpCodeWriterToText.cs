using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamasoft.JsonClassGenerator;

namespace DescriptJson.CodeWriters_to_Str
{
    public class CSharpCodeWriterToText : ICodeWriter
    {
        public string FileExtension
        {
            get { return ".cs"; }
        }

        public string DisplayName
        {
            get { return "C#"; }
        }

        private const string NoRenameAttribute = "[Obfuscation(Feature = \"renaming\", Exclude = true)]";
        private const string NoPruneAttribute = "[Obfuscation(Feature = \"trigger\", Exclude = false)]";

        public string GetTypeName(JsonType type, IJsonClassGeneratorConfig config)
        {
            var arraysAsLists = !config.ExplicitDeserialization;

            switch (type.Type)
            {
                case JsonTypeEnum.Anything: return "object";
                case JsonTypeEnum.Array: return arraysAsLists ? "IList<" + GetTypeName(type.InternalType, config) + ">" : GetTypeName(type.InternalType, config) + "[]";
                case JsonTypeEnum.Dictionary: return "Dictionary<string, " + GetTypeName(type.InternalType, config) + ">";
                case JsonTypeEnum.Boolean: return "bool";
                case JsonTypeEnum.Float: return "double";
                case JsonTypeEnum.Integer: return "int";
                case JsonTypeEnum.Long: return "long";
                case JsonTypeEnum.Date: return "DateTime";
                case JsonTypeEnum.NonConstrained: return "object";
                case JsonTypeEnum.NullableBoolean: return "bool?";
                case JsonTypeEnum.NullableFloat: return "double?";
                case JsonTypeEnum.NullableInteger: return "int?";
                case JsonTypeEnum.NullableLong: return "long?";
                case JsonTypeEnum.NullableDate: return "DateTime?";
                case JsonTypeEnum.NullableSomething: return "object";
                case JsonTypeEnum.Object: return type.AssignedName;
                case JsonTypeEnum.String: return "string";
                default: throw new System.NotSupportedException("Unsupported json type");
            }
        }


        public void WriteFileStart(IJsonClassGeneratorConfig config, StringBuilder sb)
        {
            if (config.UseNestedClasses)
            {
                sb.AppendLine(string.Format("    {0} class {1}", config.InternalVisibility ? "internal" : "public", config.MainClass))
                  .AppendLine("    {");
            }
        }


        public void WriteFileEnd(IJsonClassGeneratorConfig config, StringBuilder sb)
        {
            if (config.UseNestedClasses)
            {
                sb.AppendLine("    }");
            }
        }


        public void WriteNamespaceStart(IJsonClassGeneratorConfig config, StringBuilder sb, bool root)
        {
            sb.AppendLine()
              .AppendLine(string.Format("namespace {0}", root && !config.UseNestedClasses ? config.Namespace : (config.SecondaryNamespace ?? config.Namespace)))
              .AppendLine("{")
              .AppendLine();
        }


        public void WriteNamespaceEnd(IJsonClassGeneratorConfig config, StringBuilder sb, bool root)
        {
            sb.AppendLine("}");
        }


        public void WriteClass(IJsonClassGeneratorConfig config, StringBuilder sb, JsonType type)
        {
            var visibility = config.InternalVisibility ? "internal" : "public";
            if (config.UseNestedClasses)
            {
                if (!type.IsRoot)
                {
                    if (ShouldApplyNoRenamingAttribute(config)) sb.AppendLine("        " + NoRenameAttribute);
                    if (ShouldApplyNoPruneAttribute(config)) sb.AppendLine("        " + NoPruneAttribute);
                    sb.AppendLine(string.Format("        {0} class {1}", visibility, type.AssignedName))
                      .AppendLine("        {");
                }
            }
            else
            {
                if (ShouldApplyNoRenamingAttribute(config)) sb.AppendLine("    " + NoRenameAttribute);
                if (ShouldApplyNoPruneAttribute(config)) sb.AppendLine("    " + NoPruneAttribute);
                sb.AppendLine(string.Format("    {0} class {1}", visibility, type.AssignedName))
                  .AppendLine("    {");
            }

            var prefix = config.UseNestedClasses && !type.IsRoot ? "            " : "        ";


            var shouldSuppressWarning = config.InternalVisibility && !config.UseProperties && !config.ExplicitDeserialization;
            if (shouldSuppressWarning)
            {
                sb.AppendLine("#pragma warning disable 0649");
                if (!config.UsePascalCase) sb.AppendLine();
            }

            if (type.IsRoot && config.ExplicitDeserialization) WriteStringConstructorExplicitDeserialization(config, sb, type, prefix);

            if (config.ExplicitDeserialization)
            {
                if (config.UseProperties) WriteClassWithPropertiesExplicitDeserialization(sb, type, prefix);
                else WriteClassWithFieldsExplicitDeserialization(sb, type, prefix);
            }
            else
            {
                WriteClassMembers(config, sb, type, prefix);
            }

            if (shouldSuppressWarning)
            {
                sb.AppendLine()
                  .AppendLine("#pragma warning restore 0649")
                  .AppendLine();
            }

            
            if (config.UseNestedClasses && !type.IsRoot)
            {
                sb.AppendLine("        }");
            }


            if (!config.UseNestedClasses)
            {
                sb.AppendLine("    }");
            }


            sb.AppendLine();
        }



        private void WriteClassMembers(IJsonClassGeneratorConfig config, StringBuilder sb, JsonType type, string prefix)
        {
            int cnt = 1;
            foreach (var field in type.Fields)
            {
                //if (config.UsePascalCase || config.ExamplesInDocumentation) sb.AppendLine();

                if (config.ExamplesInDocumentation)
                {
                    sb.AppendLine(prefix + "/// <summary>")
                      .AppendLine(prefix + "/// Examples: " + field.GetExamplesText())
                      .AppendLine(prefix + "/// </summary>");
                }

                if (config.UsePascalCase)
                {

                    sb.AppendLine(prefix + string.Format("[JsonProperty(\"{0}\")]", field.JsonMemberName));
                }

                if (config.UseProperties)
                {
                    sb.AppendLine(prefix + string.Format("public {0} {1} {{ get; set; }}", field.Type.GetTypeName(), field.MemberName));
                }
                else
                {
                    sb.AppendLine(prefix + string.Format("public {0} {1};", field.Type.GetTypeName(), field.MemberName));
                }

                if (type.Fields.Count != cnt++) sb.AppendLine();
            }

        }

        #region Code for (obsolete) explicit deserialization
        private void WriteClassWithPropertiesExplicitDeserialization(StringBuilder sb, JsonType type, string prefix)
        {

            sb.AppendLine(prefix + "private JObject __jobject;")
              .AppendLine(prefix + "{")
              .AppendLine(prefix + "    this.__jobject = obj;")
              .AppendLine(prefix + "}")
              .AppendLine();

            foreach (var field in type.Fields)
            {

                string variable = null;
                if (field.Type.MustCache)
                {
                    variable = "_" + char.ToLower(field.MemberName[0]) + field.MemberName.Substring(1);
                    sb.AppendLine(prefix + "[System.Diagnostics.DebuggerBrowsable(System.Diagnostics.DebuggerBrowsableState.Never)]")
                      .AppendLine(prefix + string.Format("private {0} {1};", field.Type.GetTypeName(), variable));
                }

                sb.AppendLine(prefix + string.Format("public {0} {1}", field.Type.GetTypeName(), field.MemberName))
                  .AppendLine(prefix + "{")
                  .AppendLine(prefix + "    get")
                  .AppendLine(prefix + "    {");

                if (field.Type.MustCache)
                {
                    sb.AppendLine(prefix + string.Format("        if ({0} == null)", variable))
                      .AppendLine(prefix + string.Format("            {0} = {1};", variable, field.GetGenerationCode("__jobject")))
                      .AppendLine(prefix + string.Format("        return {0};", variable));
                }
                else
                {
                    sb.AppendLine(prefix + string.Format("        return {0};", field.GetGenerationCode("__jobject")));
                }

                sb.AppendLine(prefix + "    }")
                  .AppendLine(prefix + "}")
                  .AppendLine();
            }
        }


        private void WriteStringConstructorExplicitDeserialization(IJsonClassGeneratorConfig config, StringBuilder sb, JsonType type, string prefix)
        {
            sb.AppendLine()
              .AppendLine(prefix + string.Format("public {1}(string json)", config.InternalVisibility ? "internal" : "public", type.AssignedName))
              .AppendLine(prefix + "    : this(JObject.Parse(json))")
              .AppendLine(prefix + "{")
              .AppendLine(prefix + "}")
              .AppendLine();
        }


        private void WriteClassWithFieldsExplicitDeserialization(StringBuilder sb, JsonType type, string prefix)
        {
            sb.AppendLine(prefix + string.Format("public {0}(JObject obj)", type.AssignedName))
              .AppendLine(prefix + "{");

            foreach (var field in type.Fields)
            {
                sb.AppendLine(prefix + string.Format("    this.{0} = {1};", field.MemberName, field.GetGenerationCode("obj")));
            }

            sb.AppendLine(prefix + "}")
              .AppendLine();

            foreach (var field in type.Fields)
            {
                sb.AppendLine(prefix + string.Format("public readonly {0} {1};", field.Type.GetTypeName(), field.MemberName));
            }
        }

        private bool ShouldApplyNoRenamingAttribute(IJsonClassGeneratorConfig config)
        {
            return config.ApplyObfuscationAttributes && !config.ExplicitDeserialization && !config.UsePascalCase;
        }


        private bool ShouldApplyNoPruneAttribute(IJsonClassGeneratorConfig config)
        {
            return config.ApplyObfuscationAttributes && !config.ExplicitDeserialization && config.UseProperties;
        }
        #endregion

        #region "not implement"
        public void WriteFileStart(IJsonClassGeneratorConfig config, TextWriter sw)
        {
            throw new NotImplementedException();
        }


        public void WriteNamespaceStart(IJsonClassGeneratorConfig config, TextWriter sw, bool root)
        {
            throw new NotImplementedException();
        }


        public void WriteClass(IJsonClassGeneratorConfig config, TextWriter sw, JsonType type)
        {
            throw new NotImplementedException();
        }


        public void WriteNamespaceEnd(IJsonClassGeneratorConfig config, TextWriter sw, bool root)
        {
            throw new NotImplementedException();
        }


        public void WriteFileEnd(IJsonClassGeneratorConfig config, TextWriter sw)
        {
            throw new NotImplementedException();
        }
        #endregion

    }
}
