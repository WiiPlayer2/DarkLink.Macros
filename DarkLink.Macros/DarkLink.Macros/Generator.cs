using System;
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
                (node, token) => node is VariableDeclaratorSyntax {Initializer: {Value: LiteralExpressionSyntax literalExpressionSyntax,},} && literalExpressionSyntax.IsKind(SyntaxKind.StringLiteralExpression),
                (syntaxContext, token) =>
                {
                    var attribute = syntaxContext.Attributes.Single(a => a.AttributeClass?.ToDisplayString() == ATTRIBUTE_NAME);
                    var arguments = attribute.ConstructorArguments[0].Values.Select(v => v.Value).ToArray();
                    var template = (string) (syntaxContext.TargetSymbol as IFieldSymbol)!.ConstantValue!;
                    var containingType = syntaxContext.TargetSymbol.ContainingType;

                    return new MacroDefinition(template, containingType, arguments);
                })
            .Collect()
            .Select((arr, _) => arr.GroupBy(o => o.ContainingType, SymbolEqualityComparer.Default).ToList());

        context.RegisterSourceOutput(macroDefinitions, (productionContext, groupings) =>
        {
            foreach (var grouping in groupings)
            {
                var containingType = grouping.Key;
                var hintName = $"Macros_{containingType!.ToDisplayString()}.cs";

                using var codeBuilder = new StringWriter();
                codeBuilder.WriteLine($"namespace {containingType.ContainingNamespace.Name} {{");
                codeBuilder.WriteLine($"partial class {containingType.Name} {{");

                foreach (var definition in grouping)
                {
                    var generated = string.Format(definition.Template, definition.Arguments);
                    codeBuilder.WriteLine(generated);
                }

                codeBuilder.WriteLine("}");
                codeBuilder.WriteLine("}");
                var code = codeBuilder.ToString();

                productionContext.AddSource(hintName, SourceText.From(code, encoding));
            }
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
            context.AddSource(resource, SourceText.From(stream, encoding));
        }
    }

    private record MacroDefinition(string Template, INamedTypeSymbol ContainingType, object?[] Arguments);
}
