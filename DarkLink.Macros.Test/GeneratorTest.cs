using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DarkLink.Macros.Test;

[TestClass]
public class GeneratorTest : VerifyBase
{
    [TestMethod]
    public Task Empty()
    {
        // Arrange

        // Act
        var driver = GenerateDriver();

        // Assert
        return Verify(driver);
    }

    private GeneratorDriver GenerateDriver(string? source = null)
    {
        var syntaxTree = source is not null
            ? new[] {CSharpSyntaxTree.ParseText(source),}
            : Enumerable.Empty<SyntaxTree>();
        var compilation = CSharpCompilation.Create("Tests", syntaxTree);
        var generator = new Generator();

        var driver = CSharpGeneratorDriver.Create(generator);
        return driver.RunGenerators(compilation);
    }
}
