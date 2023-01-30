namespace DarkLink.Macros.Test;

[TestClass]
public class GeneratorTest : VerifySourceGenerator
{
    [TestMethod]
    public async Task Empty()
    {
        var source = string.Empty;

        await Verify(source);
    }

    [TestMethod]
    public async Task MacroDefinition()
    {
        var source = @"
using DarkLink.Macros;

namespace Tests;

internal static partial class Templates
{
    [Macro(1)]
    private const string TEMPLATE = @""public static string Templated_{0} = """"A templated string: \""""{0}\"""";"""""";
}
";

        await Verify(source);
    }
}
