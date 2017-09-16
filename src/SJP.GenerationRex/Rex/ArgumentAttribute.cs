using System;
using EnumsNET;

namespace Rex
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class ArgumentAttribute : Attribute
    {
        public ArgumentAttribute(ArgumentType type)
        {
            if (!type.IsValid())
                throw new ArgumentException($"The given { nameof(ArgumentType) } is not a valid enum.", nameof(type));

            Type = type;
        }

        public ArgumentType Type { get; }

        public bool DefaultShortName => ShortName == null;

        public string ShortName { get; set; }

        public bool DefaultLongName => LongName == null;

        public string LongName { get; set; }

        public object DefaultValue { get; set; }

        public bool HasDefaultValue() => DefaultValue != null;

        public bool HasHelpText => HelpText != null;

        public string HelpText { get; set; }
    }
}
