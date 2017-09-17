using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Text;

namespace SJP.GenerationRex
{
    internal sealed class CommandLineParser
    {
        public const string NewLine = "\r\n";
        private const int spaceBeforeParam = 2;
        private ArrayList arguments;
        private Hashtable argumentMap;
        private CommandLineParser.Argument defaultArgument;
        private ErrorReporter reporter;

        public static bool ParseArgumentsWithUsage(string[] arguments, object destination)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (CommandLineParser.ParseHelp(arguments))
            {
                Console.Write(CommandLineParser.ArgumentsUsage(destination.GetType()));
                return false;
            }
            return CommandLineParser.ParseArguments(arguments, destination);
        }

        public static bool ParseArgumentsWithUsage(string[] arguments, Type destination)
        {
            return !CommandLineParser.ParseHelp(arguments) && CommandLineParser.ParseArguments(arguments, destination);
        }

        public static bool ParseArguments(string[] arguments, object destination)
        {
            return CommandLineParser.ParseArguments(arguments, destination, new ErrorReporter(Console.Error.WriteLine));
        }

        public static bool ParseArguments(string[] arguments, Type destination)
        {
            return CommandLineParser.ParseArguments(arguments, destination, new ErrorReporter(Console.Error.WriteLine));
        }

        public static bool ParseArguments(string[] arguments, object destination, ErrorReporter reporter)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            return new CommandLineParser(destination.GetType(), reporter).Parse(arguments, destination);
        }

        public static bool ParseArguments(string[] arguments, Type destination, ErrorReporter reporter)
        {
            return new CommandLineParser(destination, reporter).Parse(arguments, (object)null);
        }

        private static void NullErrorReporter(string message)
        {
        }

        public static bool ParseHelp(string[] args)
        {
            CommandLineParser commandLineParser = new CommandLineParser(typeof(CommandLineParser.HelpArgument), new ErrorReporter(CommandLineParser.NullErrorReporter));
            CommandLineParser.HelpArgument helpArgument = new CommandLineParser.HelpArgument();
            commandLineParser.Parse(args, (object)helpArgument);
            return helpArgument.help;
        }

        public static string ArgumentsUsage(Type argumentType)
        {
            int columns = Console.WindowWidth;
            if (columns == 0)
                columns = 80;
            return CommandLineParser.ArgumentsUsage(argumentType, columns);
        }

        public static string ArgumentsUsage(Type argumentType, int columns)
        {
            return new CommandLineParser(argumentType, (ErrorReporter)null).GetUsageString(columns);
        }

        public static int IndexOf(StringBuilder text, char value, int startIndex)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            for (int index = startIndex; index < text.Length; ++index)
            {
                if ((int)text[index] == (int)value)
                    return index;
            }
            return -1;
        }

        public static int LastIndexOf(StringBuilder text, char value, int startIndex)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            for (int index = Math.Min(startIndex, text.Length - 1); index >= 0; --index)
            {
                if ((int)text[index] == (int)value)
                    return index;
            }
            return -1;
        }

        public CommandLineParser(Type argumentSpecification, ErrorReporter reporter)
        {
            if (argumentSpecification == null)
                throw new ArgumentNullException(nameof(argumentSpecification));
            this.reporter = reporter;
            this.arguments = new ArrayList();
            this.argumentMap = new Hashtable();
            foreach (FieldInfo field in argumentSpecification.GetFields(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                if (!field.IsInitOnly && !field.IsLiteral)
                {
                    ArgumentAttribute attribute = CommandLineParser.GetAttribute(field);
                    if (attribute is DefaultArgumentAttribute)
                        this.defaultArgument = new CommandLineParser.Argument(attribute, field, reporter);
                    else if (attribute != null)
                        this.arguments.Add((object)new CommandLineParser.Argument(attribute, field, reporter));
                }
            }
            foreach (CommandLineParser.Argument obj in this.arguments)
            {
                this.argumentMap[(object)obj.LongName] = (object)obj;
                if (obj.ExplicitShortName)
                {
                    if (obj.ShortName != null && obj.ShortName.Length > 0)
                        this.argumentMap[(object)obj.ShortName] = (object)obj;
                    else
                        obj.ClearShortName();
                }
            }
            foreach (CommandLineParser.Argument obj in this.arguments)
            {
                if (!obj.ExplicitShortName)
                {
                    if (obj.ShortName != null && obj.ShortName.Length > 0 && !this.argumentMap.ContainsKey((object)obj.ShortName))
                        this.argumentMap[(object)obj.ShortName] = (object)obj;
                    else
                        obj.ClearShortName();
                }
            }
        }

        private static ArgumentAttribute GetAttribute(FieldInfo field)
        {
            object[] customAttributes = field.GetCustomAttributes(typeof(ArgumentAttribute), false);
            if (customAttributes.Length == 1)
                return (ArgumentAttribute)customAttributes[0];
            return (ArgumentAttribute)null;
        }

        private void ReportUnrecognizedArgument(string argument)
        {
            this.reporter(string.Format("Unrecognized command line argument '{0}'", (object)argument));
        }

        private bool ParseArgumentList(string[] args, object destination)
        {
            bool flag = false;
            if (args != null)
            {
                foreach (string str1 in args)
                {
                    if (str1.Length > 0)
                    {
                        switch (str1[0])
                        {
                            case '-':
                            case '/':
                                int num = str1.IndexOfAny(new char[3]
                                {
                  ':',
                  '+',
                  '-'
                                }, 1);
                                string str2 = str1.Substring(1, num == -1 ? str1.Length - 1 : num - 1);
                                string str3 = str2.Length + 1 != str1.Length ? (str1.Length <= 1 + str2.Length || (int)str1[1 + str2.Length] != 58 ? str1.Substring(str2.Length + 1) : str1.Substring(str2.Length + 2)) : (string)null;
                                CommandLineParser.Argument obj = (CommandLineParser.Argument)this.argumentMap[(object)str2];
                                if (obj == null)
                                {
                                    this.ReportUnrecognizedArgument(str1);
                                    flag = true;
                                    continue;
                                }
                                flag |= !obj.SetValue(str3, destination);
                                continue;
                            case '@':
                                string[] arguments1;
                                flag = flag | this.LexFileArguments(str1.Substring(1), out arguments1) | this.ParseArgumentList(arguments1, destination);
                                continue;
                            default:
                                if (this.defaultArgument != null)
                                {
                                    flag |= !this.defaultArgument.SetValue(str1, destination);
                                    continue;
                                }
                                this.ReportUnrecognizedArgument(str1);
                                flag = true;
                                continue;
                        }
                    }
                }
            }
            return flag;
        }

        public bool Parse(string[] args, object destination)
        {
            bool argumentList = this.ParseArgumentList(args, destination);
            foreach (CommandLineParser.Argument obj in this.arguments)
                argumentList |= obj.Finish(destination);
            if (this.defaultArgument != null)
                argumentList |= this.defaultArgument.Finish(destination);
            return !argumentList;
        }

        public string GetUsageString(int screenWidth)
        {
            CommandLineParser.ArgumentHelpStrings[] allHelpStrings = this.GetAllHelpStrings();
            int val1 = 0;
            foreach (CommandLineParser.ArgumentHelpStrings argumentHelpStrings in allHelpStrings)
                val1 = Math.Max(val1, argumentHelpStrings.syntax.Length);
            int num1 = val1 + 2;
            screenWidth = Math.Max(screenWidth, 15);
            int num2 = screenWidth >= num1 + 10 ? num1 : 5;
            StringBuilder builder = new StringBuilder();
            builder.AppendLine();
            Assembly entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                builder.AppendFormat("{0} version {1}", (object)CommandLineParser.GetTitle(entryAssembly), (object)entryAssembly.GetName().Version);
                builder.AppendLine();
                AssemblyCopyrightAttribute assemblyAttribute = CommandLineParser.GetAssemblyAttribute<AssemblyCopyrightAttribute>(entryAssembly);
                if (assemblyAttribute != null && !string.IsNullOrEmpty(assemblyAttribute.Copyright))
                    builder.AppendLine(assemblyAttribute.Copyright.Replace("©", "(C)"));
                builder.AppendLine();
                builder.AppendFormat("Usage: {0}", (object)Path.GetFileNameWithoutExtension(entryAssembly.Location));
                foreach (CommandLineParser.ArgumentHelpStrings argumentHelpStrings in allHelpStrings)
                    builder.AppendFormat(" {0}", (object)argumentHelpStrings.syntax);
                builder.AppendLine();
                builder.AppendLine();
            }
            foreach (CommandLineParser.ArgumentHelpStrings argumentHelpStrings in allHelpStrings)
            {
                int length = argumentHelpStrings.syntax.Length;
                builder.Append(argumentHelpStrings.syntax);
                int currentColumn = length;
                if (length >= num2)
                {
                    builder.Append("\n");
                    currentColumn = 0;
                }
                int val2 = screenWidth - num2;
                int startIndex = 0;
                label_21:
                while (startIndex < argumentHelpStrings.help.Length)
                {
                    builder.Append(' ', num2 - currentColumn);
                    currentColumn = num2;
                    int num3 = startIndex + val2;
                    int num4;
                    if (num3 >= argumentHelpStrings.help.Length)
                    {
                        num4 = argumentHelpStrings.help.Length;
                    }
                    else
                    {
                        num4 = argumentHelpStrings.help.LastIndexOf(' ', num3 - 1, Math.Min(num3 - startIndex, val2));
                        if (num4 <= startIndex)
                            num4 = startIndex + val2;
                    }
                    builder.Append(argumentHelpStrings.help, startIndex, num4 - startIndex);
                    startIndex = num4;
                    CommandLineParser.AddNewLine("\n", builder, ref currentColumn);
                    while (true)
                    {
                        if (startIndex < argumentHelpStrings.help.Length && (int)argumentHelpStrings.help[startIndex] == 32)
                            ++startIndex;
                        else
                            goto label_21;
                    }
                }
                if (argumentHelpStrings.help.Length == 0)
                    builder.Append("\n");
            }
            return builder.ToString();
        }

        private static string GetTitle(Assembly assembly)
        {
            string str = "";
            AssemblyCompanyAttribute assemblyAttribute1 = CommandLineParser.GetAssemblyAttribute<AssemblyCompanyAttribute>(assembly);
            if (assemblyAttribute1 != null && !string.IsNullOrEmpty(assemblyAttribute1.Company))
                str = assemblyAttribute1.Company + " ";
            AssemblyTitleAttribute assemblyAttribute2 = CommandLineParser.GetAssemblyAttribute<AssemblyTitleAttribute>(assembly);
            if (assemblyAttribute2 != null && !string.IsNullOrEmpty(assemblyAttribute2.Title))
            {
                if (!assemblyAttribute2.Title.StartsWith(str))
                    return str + assemblyAttribute2.Title;
                return assemblyAttribute2.Title;
            }
            AssemblyProductAttribute assemblyAttribute3 = CommandLineParser.GetAssemblyAttribute<AssemblyProductAttribute>(assembly);
            if (assemblyAttribute2 != null && !string.IsNullOrEmpty(assemblyAttribute3.Product))
            {
                if (!assemblyAttribute3.Product.StartsWith(str))
                    return str + assemblyAttribute3.Product;
                return assemblyAttribute3.Product;
            }
            AssemblyDescriptionAttribute assemblyAttribute4 = CommandLineParser.GetAssemblyAttribute<AssemblyDescriptionAttribute>(assembly);
            if (assemblyAttribute2 == null || string.IsNullOrEmpty(assemblyAttribute4.Description))
                return str + assembly.GetName().Name;
            if (!assemblyAttribute4.Description.StartsWith(str))
                return str + assemblyAttribute4.Description;
            return assemblyAttribute4.Description;
        }

        private static T GetAssemblyAttribute<T>(Assembly assembly) where T : Attribute
        {
            object[] customAttributes = assembly.GetCustomAttributes(typeof(T), true);
            if (customAttributes != null && customAttributes.Length > 0)
                return (T)customAttributes[0];
            return default(T);
        }

        private static void AddNewLine(string newLine, StringBuilder builder, ref int currentColumn)
        {
            builder.Append(newLine);
            currentColumn = 0;
        }

        private CommandLineParser.ArgumentHelpStrings[] GetAllHelpStrings()
        {
            CommandLineParser.ArgumentHelpStrings[] argumentHelpStringsArray1 = new CommandLineParser.ArgumentHelpStrings[this.NumberOfParametersToDisplay()];
            int num1 = 0;
            foreach (CommandLineParser.Argument obj in this.arguments)
                argumentHelpStringsArray1[num1++] = CommandLineParser.GetHelpStrings(obj);
            if (this.defaultArgument != null)
                argumentHelpStringsArray1[num1++] = CommandLineParser.GetHelpStrings(this.defaultArgument);
            CommandLineParser.ArgumentHelpStrings[] argumentHelpStringsArray2 = argumentHelpStringsArray1;
            int index = num1;
            int num2 = 1;
            int num3 = index + num2;
            argumentHelpStringsArray2[index] = new CommandLineParser.ArgumentHelpStrings("@<file>", "Read response file for more options.");
            return argumentHelpStringsArray1;
        }

        private static CommandLineParser.ArgumentHelpStrings GetHelpStrings(CommandLineParser.Argument arg)
        {
            return new CommandLineParser.ArgumentHelpStrings(arg.SyntaxHelp, arg.FullHelpText);
        }

        private int NumberOfParametersToDisplay()
        {
            int num = this.arguments.Count + 1;
            if (this.HasDefaultArgument)
                ++num;
            return num;
        }

        public bool HasDefaultArgument
        {
            get
            {
                return this.defaultArgument != null;
            }
        }

        private bool LexFileArguments(string fileName, out string[] arguments1)
        {
            string str = (string)null;
            try
            {
                using (FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
                    str = new StreamReader((Stream)fileStream).ReadToEnd();
            }
            catch (Exception ex)
            {
                this.reporter(string.Format("Error: Can't open command line argument file '{0}' : '{1}'", (object)fileName, (object)ex.Message));
                arguments1 = (string[])null;
                return true;
            }
            bool flag1 = false;
            ArrayList arrayList = new ArrayList();
            StringBuilder stringBuilder = new StringBuilder();
            bool flag2 = false;
            int index = 0;
            try
            {
                label_10:
                while (true)
                {
                    while (char.IsWhiteSpace(str[index]))
                        ++index;
                    if ((int)str[index] != 35)
                    {
                        do
                        {
                            if ((int)str[index] == 92)
                            {
                                int repeatCount = 1;
                                ++index;
                                while (index == str.Length && (int)str[index] == 92)
                                    ++repeatCount;
                                if (index == str.Length || (int)str[index] != 34)
                                {
                                    stringBuilder.Append('\\', repeatCount);
                                }
                                else
                                {
                                    stringBuilder.Append('\\', repeatCount >> 1);
                                    if ((repeatCount & 1) != 0)
                                        stringBuilder.Append('"');
                                    else
                                        flag2 = !flag2;
                                }
                            }
                            else if ((int)str[index] == 34)
                            {
                                flag2 = !flag2;
                                ++index;
                            }
                            else
                            {
                                stringBuilder.Append(str[index]);
                                ++index;
                            }
                        }
                        while (!char.IsWhiteSpace(str[index]) || flag2);
                        arrayList.Add((object)stringBuilder.ToString());
                        stringBuilder.Length = 0;
                    }
                    else
                        break;
                }
                ++index;
                while ((int)str[index] != 10)
                    ++index;
                goto label_10;
            }
            catch (IndexOutOfRangeException)
            {
                if (flag2)
                {
                    this.reporter(string.Format("Error: Unbalanced '\"' in command line argument file '{0}'", (object)fileName));
                    flag1 = true;
                }
                else if (stringBuilder.Length > 0)
                    arrayList.Add((object)stringBuilder.ToString());
            }
            arguments1 = (string[])arrayList.ToArray(typeof(string));
            return flag1;
        }

        private static string LongName(ArgumentAttribute attribute, FieldInfo field)
        {
            if (attribute != null && !attribute.DefaultLongName)
                return attribute.LongName;
            return field.Name;
        }

        private static string ShortName(ArgumentAttribute attribute, FieldInfo field)
        {
            if (attribute is DefaultArgumentAttribute)
                return (string)null;
            if (!CommandLineParser.ExplicitShortName(attribute))
                return CommandLineParser.LongName(attribute, field).Substring(0, 1);
            return attribute.ShortName;
        }

        private static string HelpText(ArgumentAttribute attribute)
        {
            if (attribute == null)
                return (string)null;
            return attribute.HelpText;
        }

        private static bool HasHelpText(ArgumentAttribute attribute)
        {
            if (attribute != null)
                return attribute.HasHelpText;
            return false;
        }

        private static bool ExplicitShortName(ArgumentAttribute attribute)
        {
            if (attribute != null)
                return !attribute.DefaultShortName;
            return false;
        }

        private static object DefaultValue(ArgumentAttribute attribute)
        {
            if (attribute != null && attribute.HasDefaultValue)
                return attribute.DefaultValue;
            return (object)null;
        }

        private static Type ElementType(FieldInfo field)
        {
            if (CommandLineParser.IsCollectionType(field.FieldType))
                return field.FieldType.GetElementType();
            return (Type)null;
        }

        private static ArgumentType Flags(ArgumentAttribute attribute, FieldInfo field)
        {
            if (attribute != null)
                return attribute.Type;
            return CommandLineParser.IsCollectionType(field.FieldType) ? ArgumentType.MultipleUnique : ArgumentType.AtMostOnce;
        }

        private static bool IsCollectionType(Type type)
        {
            return type.IsArray;
        }

        private static bool IsValidElementType(Type type)
        {
            if (type == null)
                return false;
            if (type != typeof(int) && type != typeof(uint) && (type != typeof(string) && type != typeof(bool)))
                return type.IsEnum;
            return true;
        }

        private class HelpArgument
        {
            [Argument(ArgumentType.AtMostOnce, ShortName = "?")]
#pragma warning disable 649
            public bool help;
#pragma warning restore 649
        }

        private struct ArgumentHelpStrings
        {
            public string syntax;
            public string help;

            public ArgumentHelpStrings(string syntax, string help)
            {
                this.syntax = syntax;
                this.help = help;
            }
        }

        private class Argument
        {
            private string longName;
            private string shortName;
            private string helpText;
            private bool hasHelpText;
            private bool explicitShortName;
            private object defaultValue;
            private bool seenValue;
            private FieldInfo field;
            private Type elementType;
            private ArgumentType flags;
            private ArrayList collectionValues;
            private ErrorReporter reporter;
            private bool isDefault;

            public Argument(ArgumentAttribute attribute, FieldInfo field, ErrorReporter reporter)
            {
                this.longName = CommandLineParser.LongName(attribute, field);
                this.explicitShortName = CommandLineParser.ExplicitShortName(attribute);
                this.shortName = CommandLineParser.ShortName(attribute, field);
                this.hasHelpText = CommandLineParser.HasHelpText(attribute);
                this.helpText = CommandLineParser.HelpText(attribute);
                this.defaultValue = CommandLineParser.DefaultValue(attribute);
                this.elementType = CommandLineParser.ElementType(field);
                this.flags = CommandLineParser.Flags(attribute, field);
                this.field = field;
                this.seenValue = false;
                this.reporter = reporter;
                this.isDefault = attribute != null && attribute is DefaultArgumentAttribute;
                if (!this.IsCollection)
                    return;
                this.collectionValues = new ArrayList();
            }

            public bool Finish(object destination)
            {
                if (!this.SeenValue && this.HasDefaultValue)
                    this.field.SetValue(destination, this.DefaultValue);
                if (this.IsCollection)
                    this.field.SetValue(destination, (object)this.collectionValues.ToArray(this.elementType));
                return this.ReportMissingRequiredArgument();
            }

            private bool ReportMissingRequiredArgument()
            {
                if (!this.IsRequired || this.SeenValue)
                    return false;
                if (this.IsDefault)
                    this.reporter(string.Format("Missing required argument '<{0}>'.", (object)this.LongName));
                else
                    this.reporter(string.Format("Missing required argument '/{0}'.", (object)this.LongName));
                return true;
            }

            private void ReportDuplicateArgumentValue(string value)
            {
                this.reporter(string.Format("Duplicate '{0}' argument '{1}'", (object)this.LongName, (object)value));
            }

            public bool SetValue(string value, object destination)
            {
                if (this.SeenValue && !this.AllowMultiple)
                {
                    this.reporter(string.Format("Duplicate '{0}' argument", (object)this.LongName));
                    return false;
                }
                this.seenValue = true;
                object obj;
                if (!this.ParseValue(this.ValueType, value, out obj))
                    return false;
                if (this.IsCollection)
                {
                    if (this.Unique && this.collectionValues.Contains(obj))
                    {
                        this.ReportDuplicateArgumentValue(value);
                        return false;
                    }
                    this.collectionValues.Add(obj);
                }
                else
                    this.field.SetValue(destination, obj);
                return true;
            }

            public Type ValueType
            {
                get
                {
                    if (!this.IsCollection)
                        return this.Type;
                    return this.elementType;
                }
            }

            private void ReportBadArgumentValue(string value)
            {
                this.reporter(string.Format("'{0}' is not a valid value for the '{1}' command line option", (object)value, (object)this.LongName));
            }

            private bool ParseValue(Type type, string stringData, out object value)
            {
                if (stringData != null || type == typeof(bool))
                {
                    if (stringData != null)
                    {
                        if (stringData.Length <= 0)
                            goto label_16;
                    }
                    try
                    {
                        if (type == typeof(string))
                        {
                            value = (object)stringData;
                            return true;
                        }
                        if (type == typeof(bool))
                        {
                            if (stringData == null || stringData == "+")
                            {
                                value = (object)true;
                                return true;
                            }
                            if (stringData == "-")
                            {
                                value = (object)false;
                                return true;
                            }
                        }
                        else
                        {
                            if (type == typeof(int))
                            {
                                value = (object)int.Parse(stringData);
                                return true;
                            }
                            if (type == typeof(uint))
                            {
                                value = (object)uint.Parse(stringData);
                                return true;
                            }
                            value = Enum.Parse(type, stringData, true);
                            return true;
                        }
                    }
                    catch
                    {
                    }
                }
                label_16:
                this.ReportBadArgumentValue(stringData);
                value = (object)null;
                return false;
            }

            private void AppendValue(StringBuilder builder, object value)
            {
                if (value is string || value is int || (value is uint || value.GetType().IsEnum))
                    builder.Append(value.ToString());
                else if (value is bool)
                {
                    builder.Append((bool)value ? "+" : "-");
                }
                else
                {
                    bool flag = true;
                    foreach (object obj in (Array)value)
                    {
                        if (!flag)
                            builder.Append(", ");
                        this.AppendValue(builder, obj);
                        flag = false;
                    }
                }
            }

            public string LongName
            {
                get
                {
                    return this.longName;
                }
            }

            public bool ExplicitShortName
            {
                get
                {
                    return this.explicitShortName;
                }
            }

            public string ShortName
            {
                get
                {
                    return this.shortName;
                }
            }

            public bool HasShortName
            {
                get
                {
                    return this.shortName != null;
                }
            }

            public void ClearShortName()
            {
                this.shortName = (string)null;
            }

            public bool HasHelpText
            {
                get
                {
                    return this.hasHelpText;
                }
            }

            public string HelpText
            {
                get
                {
                    return this.helpText;
                }
            }

            public object DefaultValue
            {
                get
                {
                    return this.defaultValue;
                }
            }

            public bool HasDefaultValue
            {
                get
                {
                    return null != this.defaultValue;
                }
            }

            public string FullHelpText
            {
                get
                {
                    StringBuilder builder = new StringBuilder();
                    if (this.HasHelpText)
                        builder.Append(this.HelpText);
                    if (this.HasDefaultValue)
                    {
                        if (builder.Length > 0)
                            builder.Append(" ");
                        builder.Append("Default value: '");
                        this.AppendValue(builder, this.DefaultValue);
                        builder.Append('\'');
                    }
                    if (this.HasShortName)
                    {
                        if (builder.Length > 0)
                            builder.Append(" ");
                        builder.Append("(Short form: /");
                        builder.Append(this.ShortName);
                        builder.Append(")");
                    }
                    return builder.ToString();
                }
            }

            public string SyntaxHelp
            {
                get
                {
                    StringBuilder stringBuilder = new StringBuilder();
                    if (this.IsDefault)
                    {
                        stringBuilder.Append("<");
                        stringBuilder.Append(this.LongName);
                        stringBuilder.Append(">");
                    }
                    else
                    {
                        if (!this.IsRequired)
                            stringBuilder.Append("[");
                        stringBuilder.Append("/");
                        stringBuilder.Append(this.LongName);
                        Type valueType = this.ValueType;
                        if (valueType == typeof(int))
                            stringBuilder.Append(":<int>");
                        else if (valueType == typeof(uint))
                            stringBuilder.Append(":<uint>");
                        else if (valueType == typeof(bool))
                            stringBuilder.Append("[+|-]");
                        else if (valueType == typeof(string))
                        {
                            stringBuilder.Append(":<string>");
                        }
                        else
                        {
                            stringBuilder.Append(":{");
                            bool flag = true;
                            foreach (FieldInfo field in valueType.GetFields())
                            {
                                if (field.IsStatic)
                                {
                                    if (flag)
                                        flag = false;
                                    else
                                        stringBuilder.Append('|');
                                    stringBuilder.Append(field.Name);
                                }
                            }
                            stringBuilder.Append('}');
                        }
                        if (!this.IsRequired)
                            stringBuilder.Append("]");
                    }
                    if (this.AllowMultiple)
                    {
                        if (this.IsRequired)
                            stringBuilder.Append("+");
                        else
                            stringBuilder.Append("*");
                    }
                    return stringBuilder.ToString();
                }
            }

            public bool IsRequired
            {
                get
                {
                    return ArgumentType.AtMostOnce != (this.flags & ArgumentType.Required);
                }
            }

            public bool SeenValue
            {
                get
                {
                    return this.seenValue;
                }
            }

            public bool AllowMultiple
            {
                get
                {
                    return ArgumentType.AtMostOnce != (this.flags & ArgumentType.Multiple);
                }
            }

            public bool Unique
            {
                get
                {
                    return ArgumentType.AtMostOnce != (this.flags & ArgumentType.Unique);
                }
            }

            public Type Type
            {
                get
                {
                    return this.field.FieldType;
                }
            }

            public bool IsCollection
            {
                get
                {
                    return CommandLineParser.IsCollectionType(this.Type);
                }
            }

            public bool IsDefault
            {
                get
                {
                    return this.isDefault;
                }
            }
        }
    }
}
