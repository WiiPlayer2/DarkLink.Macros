using System;

namespace DarkLink.Macros.Util;

internal static class Extensions
{
    public static T With<T>(this T obj, Action<T> apply)
    {
        apply(obj);
        return obj;
    }
}
