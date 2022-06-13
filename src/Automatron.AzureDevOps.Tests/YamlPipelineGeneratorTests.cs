using System.Collections.Immutable;
using System.Reflection;
using System.Runtime;
using Automatron.AzureDevOps.Generators;
using Automatron.AzureDevOps.Generators.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using NSubstitute;
using Xunit;

namespace Automatron.AzureDevOps.Tests
{
    public class YamlPipelineGeneratorTests
    {
   
        [Fact]
        public void SimpleGeneratorTest()
        {
            string userSource = File.ReadAllText("SamplePipeline.cs");
            Compilation comp = CreateCompilation(userSource);
          
            var newComp = RunGenerators(comp, out var generatorDiags, new YamlPipelineGenerator());
         
            //Assert.Empty(generatorDiags);
            //Assert.Empty(newComp.GetDiagnostics());
        }

        private static Compilation CreateCompilation(string source) => CSharpCompilation.Create(
            assemblyName: "compilation",
            syntaxTrees: new[] { CSharpSyntaxTree.ParseText(source, new CSharpParseOptions(LanguageVersion.LatestMajor)) },
            references: new[]
            {
                CorlibReference,
                SystemRuntimeReference,
                NetStdReference,
                SystemCoreReference,
                CSharpSymbolsReference,
                CodeAnalysisReference,
                MetadataReference.CreateFromFile(typeof(Pipeline).Assembly.Location)
            },
            options: new CSharpCompilationOptions(OutputKind.ConsoleApplication)
        );

        private static GeneratorDriver CreateDriver(Compilation compilation, params ISourceGenerator[] generators)
        {
            var expectedOut = "../../";
            var optionsProvider = Substitute.For<AnalyzerConfigOptionsProvider>();
            optionsProvider.GlobalOptions.TryGetValue("build_property.MSBuildProjectDirectory", out Arg.Any<string>()!).Returns(x => {
                x[1] = expectedOut;
                return true;
            });

            return CSharpGeneratorDriver.Create(
                generators: ImmutableArray.Create(generators),
                additionalTexts: ImmutableArray<AdditionalText>.Empty,
                parseOptions: (CSharpParseOptions)compilation.SyntaxTrees.First().Options,
                optionsProvider: optionsProvider
            );
        }

        private static Compilation RunGenerators(Compilation compilation, out ImmutableArray<Diagnostic> diagnostics, params ISourceGenerator[] generators)
        {
            CreateDriver(compilation, generators).RunGeneratorsAndUpdateCompilation(compilation, out var updatedCompilation, out diagnostics);
            return updatedCompilation;
        }


        static readonly MetadataReference CorlibReference =
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location);
        static readonly MetadataReference SystemRuntimeReference =
            MetadataReference.CreateFromFile(typeof(GCSettings).Assembly.Location);
        static readonly MetadataReference NetStdReference =
            MetadataReference.CreateFromFile(typeof(Attribute).Assembly.Location);
        static readonly MetadataReference SystemCoreReference =
            MetadataReference.CreateFromFile(typeof(Enumerable).Assembly.Location);
        static readonly MetadataReference CSharpSymbolsReference =
            MetadataReference.CreateFromFile(typeof(CSharpCompilation).Assembly.Location);
        static readonly MetadataReference CodeAnalysisReference =
            MetadataReference.CreateFromFile(typeof(Compilation).Assembly.Location);
    }
    
}