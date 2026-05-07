#nullable enable

using System.Reflection;

namespace CityRise.Debug
{
    /// <summary>One discovered console command — the attribute metadata plus its MethodInfo for invocation.</summary>
    public sealed class DebugCommandInfo
    {
        public string Name { get; }
        public string Description { get; }
        public MethodInfo Method { get; }
        public ParameterInfo[] Parameters { get; }

        public DebugCommandInfo(string name, string description, MethodInfo method)
        {
            Name = name;
            Description = description;
            Method = method;
            Parameters = method.GetParameters();
        }

        public string Usage()
        {
            if (Parameters.Length == 0) return Name;
            var sb = new System.Text.StringBuilder(Name);
            foreach (var p in Parameters)
            {
                sb.Append(' ').Append('<').Append(p.Name).Append(':').Append(PrettyTypeName(p.ParameterType)).Append('>');
            }
            return sb.ToString();
        }

        private static string PrettyTypeName(System.Type t)
        {
            if (t == typeof(int)) return "int";
            if (t == typeof(uint)) return "uint";
            if (t == typeof(long)) return "long";
            if (t == typeof(ulong)) return "ulong";
            if (t == typeof(float)) return "float";
            if (t == typeof(double)) return "double";
            if (t == typeof(bool)) return "bool";
            if (t == typeof(string)) return "string";
            return t.Name;
        }
    }
}
