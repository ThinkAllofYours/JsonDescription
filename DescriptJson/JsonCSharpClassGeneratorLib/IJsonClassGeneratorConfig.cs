﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Xamasoft.JsonClassGenerator
{
    public interface IJsonClassGeneratorConfig
    {
        string Namespace { get; set; }
        string SecondaryNamespace { get; set; }
        bool UseProperties { get; set; }
        bool InternalVisibility { get; set; }
        bool ExplicitDeserialization { get; set; }
        bool NoHelperClass { get; set; }
        string MainClass { get; set; }
        bool UsePascalCase { get; set; }
        bool UseNestedClasses { get; set; }
        bool ApplyObfuscationAttributes { get; set; }
        bool SingleFile { get; set; }
        ICodeWriter CodeWriter { get; set; }
        bool HasSecondaryClasses { get; set; }
        bool AlwaysUseNullableValues { get; set; }
        bool UseNamespaces { get; set; }
        bool ExamplesInDocumentation { get; set; }
    }
}
