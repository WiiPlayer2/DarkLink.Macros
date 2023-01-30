using System;
using System.Linq;
using System.Text;
using CodeGenHelpers;
using DarkLink.Macros.Util;
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
                var argumentCount = (int) attribute.ConstructorArguments[0].Value!;
                var template = (string) (syntaxContext.TargetSymbol as IFieldSymbol)!.ConstantValue!;
                var containingType = syntaxContext.TargetSymbol.ContainingType;

                return new MacroDefinition(
                    argumentCount,
                    template,
                    syntaxContext.TargetSymbol.Name,
                    containingType);
            });

        context.RegisterSourceOutput(macroDefinitions, (productionContext, definition) =>
        {
            var attributeName = $"{definition.Name}Attribute";
            var hintName = $"{definition.ContainingType.ToDisplayString()}+{attributeName}.cs";
            var code = CodeBuilder.Create(definition.ContainingType)
                .AddNestedClass(attributeName)
                .WithAccessModifier(Accessibility.Private)
                .SetBaseClass("System.Attribute")
                .AddAttribute("AttributeUsage(AttributeTargets.Class)")
                .AddConstructor(Accessibility.Public)
                .With(cb =>
                {
                    for (var i = 0; i < definition.ArgumentCount; i++) cb.AddParameter("object", $"arg{i}");
                })
                .Class
                .Builder
                .Build();
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
            context.AddSource(resource, SourceText.From(stream, encoding));
        }
    }

    private record MacroDefinition(int ArgumentCount, string Template, string Name, INamedTypeSymbol ContainingType);
}
