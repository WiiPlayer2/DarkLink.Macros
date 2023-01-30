using System;
using System.Collections.Generic;
using System.Text;

namespace DarkLink.Macros.InjectedCode
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class Macro : Attribute
    {
        public Macro(int argumentCount) { }
    }
}
