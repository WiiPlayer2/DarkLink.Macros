//HintName: DarkLink.Macros.InjectedCode.Attributes.cs
using System;
using System.Collections.Generic;
using System.Text;

namespace DarkLink.Macros
{
    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true)]
    internal sealed class MacroAttribute : Attribute
    {
        public MacroAttribute(params object[] arguments) { }
    }
}
