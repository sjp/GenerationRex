using System;

namespace SJP.GenerationRex
{
    [AttributeUsage(AttributeTargets.Field)]
    internal class ArgumentAttribute : Attribute
    {
        private string shortName;
        private string longName;
        private string helpText;
        private object defaultValue;
        private ArgumentType type;

        public ArgumentAttribute(ArgumentType type)
        {
            this.type = type;
        }

        public ArgumentType Type
        {
            get
            {
                return this.type;
            }
        }

        public bool DefaultShortName
        {
            get
            {
                return this.shortName == null;
            }
        }

        public string ShortName
        {
            get
            {
                return this.shortName;
            }
            set
            {
                this.shortName = value;
            }
        }

        public bool DefaultLongName
        {
            get
            {
                return null == this.longName;
            }
        }

        public string LongName
        {
            get
            {
                return this.longName;
            }
            set
            {
                this.longName = value;
            }
        }

        public object DefaultValue
        {
            get
            {
                return this.defaultValue;
            }
            set
            {
                this.defaultValue = value;
            }
        }

        public bool HasDefaultValue
        {
            get
            {
                return null != this.defaultValue;
            }
        }

        public bool HasHelpText
        {
            get
            {
                return null != this.helpText;
            }
        }

        public string HelpText
        {
            get
            {
                return this.helpText;
            }
            set
            {
                this.helpText = value;
            }
        }
    }
}
