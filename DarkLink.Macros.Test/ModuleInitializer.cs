using System.Runtime.CompilerServices;

namespace DarkLink.Macros.Test;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init() => VerifySourceGenerators.Enable();
}
