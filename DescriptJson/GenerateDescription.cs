using DescriptJson.CodeWriters_to_Str;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamasoft.JsonClassGenerator;

namespace DescriptJson
{
    public class GenerateDescription : IJsonClassGeneratorConfig
    {
        public string Example { get; set; }
        public string TargetFolder { get; set; }
        public string Namespace { get; set; }
        public string SecondaryNamespace { get; set; }
        public bool UseProperties { get; set; }
        public bool InternalVisibility { get; set; }
        public bool ExplicitDeserialization { get; set; }
        public bool NoHelperClass { get; set; }
        public string MainClass { get; set; }
        public bool UsePascalCase { get; set; }
        public bool UseNestedClasses { get; set; }
        public bool ApplyObfuscationAttributes { get; set; }
        public bool SingleFile { get; set; }
        public ICodeWriter CodeWriter { get; set; }
        public TextWriter OutputStream { get; set; }
        public bool AlwaysUseNullableValues { get; set; }
        public bool ExamplesInDocumentation { get; set; }

        private bool used = false;
        public bool UseNamespaces { get { return Namespace != null; } set { } }

        public IList<JsonType> Types { get; set; }
        private HashSet<string> Names = new HashSet<string>();

        public bool HasSecondaryClasses
        {
            get { return Types.Count > 1; }
            set { }
        }

        bool IJsonClassGeneratorConfig.HasSecondaryClasses { get ; set ; }
        bool IJsonClassGeneratorConfig.UseNamespaces { get ; set ; }


        //Original method name is WriteClassesToFile
        public void WriteDescriptionToStringBuilder(StringBuilder sb, IEnumerable<JsonType> types)
        {
            var inNamespace = false;
            var rootNamespace = false;

            CodeWriter.WriteFileStart(this, sb);
            for(int i = types.Count()-1; i >= 0; i--)
            {
                var type = types.ElementAt(i);
                if (UseNamespaces && inNamespace && rootNamespace != type.IsRoot && SecondaryNamespace != null) { CodeWriter.WriteNamespaceEnd(this, sb, rootNamespace); inNamespace = false; }
                if (UseNamespaces && !inNamespace) { CodeWriter.WriteNamespaceStart(this, sb, type.IsRoot); inNamespace = true; rootNamespace = type.IsRoot; }
                CodeWriter.WriteClass(this, sb, type);
            }
            
            if (UseNamespaces && inNamespace) CodeWriter.WriteNamespaceEnd(this, sb, rootNamespace);
            CodeWriter.WriteFileEnd(this, sb);
        }
    }
}
