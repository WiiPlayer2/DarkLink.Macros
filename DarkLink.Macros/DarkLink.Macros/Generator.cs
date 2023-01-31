using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace DarkLink.Macros;

[Generator]
public class Generator : IIncrementalGenerator
{
    private const string ATTRIBUTE_NAME = "DarkLink.Macros.MacroAttribute";

    private static readonly Encoding encoding = new UTF8Encoding(false);

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        context.RegisterPostInitializationOutput(PostInitialize);

        // Initialize
        var macroDefinitions = context.SyntaxProvider.ForAttributeWithMetadataName(
            ATTRIBUTE_NAME,
            (node, token) => node is VariableDeclaratorSyntax {Initializer: {Value: LiteralExpressionSyntax literalExpressionSyntax}} && literalExpressionSyntax.IsKind(SyntaxKind.StringLiteralExpression),
            (syntaxContext, token) =>
            {
                var argumentSets = syntaxContext.Attributes
                    .Select(a => a.ConstructorArguments[0].Values.Select(v => v.Value).ToArray())
                    .ToList();
                var template = (string) (syntaxContext.TargetSymbol as IFieldSymbol)!.ConstantValue!;
                var containingType = syntaxContext.TargetSymbol.ContainingType;

                return new MacroDefinition(syntaxContext.TargetSymbol.Name, template, containingType, argumentSets);
            });

        context.RegisterSourceOutput(macroDefinitions, (productionContext, definition) =>
        {
            var containingType = definition.ContainingType;
            var hintName = $"Macros_{containingType!.ToDisplayString()}+{definition.Name}.cs";

            using var codeBuilder = new StringWriter();
            codeBuilder.WriteLine($"namespace {containingType.ContainingNamespace.Name} {{");
            codeBuilder.WriteLine($"partial class {containingType.Name} {{");

            foreach (var arguments in definition.ArgumentSets)
            {
                var generated = string.Format(CultureInfo.InvariantCulture, definition.Template, arguments);
                codeBuilder.WriteLine(generated);
            }

            codeBuilder.WriteLine("}");
            codeBuilder.WriteLine("}");
            var code = codeBuilder.ToString();

            productionContext.AddSource(hintName, SourceText.From(code, encoding));
        });
    }

    private void PostInitialize(IncrementalGeneratorPostInitializationContext context)
    {
        var assembly = typeof(Generator).Assembly;
        var injectedCodeResources = assembly.GetManifestResourceNames()
            .Where(name => name.Contains("InjectedCode"));

        foreach (var resource in injectedCodeResources)
        {
            using var stream = assembly.GetManifestResourceStream(resource)!;
            context.AddSource(resource, SourceText.From(stream, encoding, canBeEmbedded: true));
        }
    }

    private record MacroDefinition(string Name, string Template, INamedTypeSymbol ContainingType, IReadOnlyList<object?[]> ArgumentSets);
}
