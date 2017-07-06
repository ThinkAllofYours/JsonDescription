using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Xamasoft.JsonClassGenerator
{
    public interface ICodeWriter
    {
        string FileExtension { get; }
        string DisplayName { get; }
        string GetTypeName(JsonType type, IJsonClassGeneratorConfig config);

        void WriteClass(IJsonClassGeneratorConfig config, TextWriter sw, JsonType type);
        void WriteFileStart(IJsonClassGeneratorConfig config, TextWriter sw);
        void WriteFileEnd(IJsonClassGeneratorConfig config, TextWriter sw);
        void WriteNamespaceStart(IJsonClassGeneratorConfig config, TextWriter sw, bool root);
        void WriteNamespaceEnd(IJsonClassGeneratorConfig config, TextWriter sw, bool root);

        //For Write Json Description in Winform
        void WriteClass(IJsonClassGeneratorConfig config, StringBuilder sb, JsonType type);
        void WriteFileStart(IJsonClassGeneratorConfig config, StringBuilder sb);
        void WriteFileEnd(IJsonClassGeneratorConfig config, StringBuilder sb);
        void WriteNamespaceStart(IJsonClassGeneratorConfig config, StringBuilder sb, bool root);
        void WriteNamespaceEnd(IJsonClassGeneratorConfig config, StringBuilder sb, bool root);
    }
}
