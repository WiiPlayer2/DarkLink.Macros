using System;
using DarkLink.Macros.InjectedCode;

namespace DemoApp;

//[Macro_Template]
internal static class Macros
{
    [Macro(1)]
    private const string Template = @"public static string Templated_{0} = ""A templated string: \""{0}\"";""";
}

internal class Program
{
    private static void Main(string[] args)
    {
        Console.WriteLine("Hello World!");
    }
}
