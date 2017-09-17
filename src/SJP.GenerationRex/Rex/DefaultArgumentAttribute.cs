using System;

namespace SJP.GenerationRex
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class DefaultArgumentAttribute : ArgumentAttribute
    {
        public DefaultArgumentAttribute(ArgumentType type)
          : base(type)
        {
        }
    }
}
