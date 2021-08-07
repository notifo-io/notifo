﻿// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System.IO;
using NJsonSchema.CodeGeneration.TypeScript;
using NSwag;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.OperationNameGenerators;
using NSwag.CodeGeneration.TypeScript;

namespace CodeGenerator
{
    public static class Program
    {
        public static void Main()
        {
            var document = OpenApiDocument.FromUrlAsync("https://localhost:5002/api/openapi.json").Result;

            GenerateTypescript(document);
            GenerateCSharp(document);
        }

        private static void GenerateCSharp(OpenApiDocument document)
        {
            var generatorSettings = new CSharpClientGeneratorSettings
            {
                ExceptionClass = "NotifoException",
                GenerateOptionalParameters = true,
                GenerateClientInterfaces = true,
                GenerateBaseUrlProperty = true,
                OperationNameGenerator = new TagNameGenerator(),
                UseBaseUrl = true
            };

            generatorSettings.CSharpGeneratorSettings.Namespace = "Notifo.SDK";
            generatorSettings.CSharpGeneratorSettings.RequiredPropertiesMustBeDefined = false;
            generatorSettings.CSharpGeneratorSettings.ExcludedTypeNames = new[] { "JsonInheritanceConverter" };

            var codeGenerator = new CSharpClientGenerator(document, generatorSettings);

            var code = codeGenerator.GenerateFile();

            code = code.Replace("https://localhost:5002", "https://app.notifo.io");

            File.WriteAllText(@"..\..\..\..\Notifo.SDK\Generated.cs", code);
        }

        private static void GenerateTypescript(OpenApiDocument document)
        {
            var generatorSettings = new TypeScriptClientGeneratorSettings
            {
                GenerateOptionalParameters = true,
                OperationNameGenerator = new TagNameGenerator()
            };

            generatorSettings.TypeScriptGeneratorSettings.TypeStyle = TypeScriptTypeStyle.Interface;
            generatorSettings.TypeScriptGeneratorSettings.EnumStyle = TypeScriptEnumStyle.StringLiteral;

            var codeGenerator = new TypeScriptClientGenerator(document, generatorSettings);

            var code = codeGenerator.GenerateFile();

            code = code.Replace("file?: FileParameter", "file?: File");
            code = code.Replace("file.data, file.fileName ? file.fileName : \"file\"", "file");

            File.WriteAllText(@"..\..\..\..\..\..\frontend\src\app\service\service.ts", code);
        }

        public sealed class TagNameGenerator : MultipleClientsFromOperationIdOperationNameGenerator
        {
            public override string GetClientName(OpenApiDocument document, string path, string httpMethod, OpenApiOperation operation)
            {
                if (operation.Tags?.Count == 1)
                {
                    return operation.Tags[0];
                }

                return base.GetClientName(document, path, httpMethod, operation);
            }
        }
    }
}
