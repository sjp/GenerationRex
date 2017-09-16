using System;

namespace Rex
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
