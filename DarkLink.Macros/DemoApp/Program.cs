using System;
using DarkLink.Macros;

namespace DemoApp;

//[Macro_Template]
internal static partial class Macros
{
    [Macro("nope"), Macro("hopla")]
    private const string Template = @"public static string Templated_{0} = ""A templated string: \""{0}\"";""";
}

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine(Macros.Templated_nope);
    }
}
