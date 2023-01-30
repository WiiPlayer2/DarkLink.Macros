//HintName: DarkLink.Macros.InjectedCode.Attributes.cs
using System;
using System.Collections.Generic;
using System.Text;

namespace DarkLink.Macros
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class Macro : Attribute
    {
        public Macro(int argumentCount) { }
    }
}
