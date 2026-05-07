#nullable enable

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace CityRise.Debug
{
    /// <summary>
    /// Discovers and dispatches <see cref="DebugCommandAttribute"/>-marked methods. Built once at
    /// console open; commands defined after that will not appear until the registry is rebuilt.
    /// </summary>
    public sealed class DebugConsoleRegistry
    {
        private readonly Dictionary<string, DebugCommandInfo> _byName = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>All registered commands, sorted by name.</summary>
        public IReadOnlyList<DebugCommandInfo> All => _byName.Values
            .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        /// <summary>Empty registry — populate via <see cref="Register"/> or <see cref="ScanLoadedAssemblies"/>.</summary>
        public DebugConsoleRegistry() { }

        /// <summary>Register a single discovered command. Last-registered wins on name collision.</summary>
        public void Register(DebugCommandInfo info)
        {
            if (info is null) throw new ArgumentNullException(nameof(info));
            _byName[info.Name] = info;
        }

        /// <summary>Walk every loaded assembly and register every static method tagged with [DebugCommand].</summary>
        public int ScanLoadedAssemblies()
        {
            var added = 0;
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                added += ScanAssembly(asm);
            }
            return added;
        }

        /// <summary>Walk one assembly. Test helper.</summary>
        public int ScanAssembly(Assembly assembly)
        {
            if (assembly is null) throw new ArgumentNullException(nameof(assembly));
            Type[] types;
            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray()!; }

            var added = 0;
            foreach (var type in types)
            {
                if (type is null) continue;
                foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var attr = method.GetCustomAttribute<DebugCommandAttribute>(inherit: false);
                    if (attr is null) continue;
                    Register(new DebugCommandInfo(attr.Name, attr.Description, method));
                    added++;
                }
            }
            return added;
        }

        /// <summary>Look up a command by name (case-insensitive). Returns null if not registered.</summary>
        public DebugCommandInfo? Find(string name)
            => _byName.TryGetValue(name, out var info) ? info : null;

        /// <summary>Names that start with <paramref name="prefix"/>, sorted. Used for autocomplete.</summary>
        public IReadOnlyList<string> Suggest(string prefix)
        {
            if (string.IsNullOrEmpty(prefix)) return Array.Empty<string>();
            return _byName.Keys
                .Where(n => n.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                .OrderBy(n => n, StringComparer.OrdinalIgnoreCase)
                .ToArray();
        }

        /// <summary>
        /// Parse <paramref name="line"/> as a command + args, look up the command, convert args
        /// to the method's parameter types, invoke. Returns the printed output (return value
        /// formatted as string, or a usage/error message).
        /// </summary>
        public string Execute(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return string.Empty;

            var tokens = Tokenize(line);
            if (tokens.Count == 0) return string.Empty;

            var name = tokens[0];
            var info = Find(name);
            if (info is null) return $"Unknown command '{name}'. Try 'help'.";

            if (tokens.Count - 1 != info.Parameters.Length)
            {
                return $"Usage: {info.Usage()}";
            }

            var args = new object?[info.Parameters.Length];
            for (int i = 0; i < info.Parameters.Length; i++)
            {
                var p = info.Parameters[i];
                if (!TryConvert(tokens[i + 1], p.ParameterType, out args[i], out var err))
                {
                    return $"Argument '{p.Name}': {err}";
                }
            }

            try
            {
                var result = info.Method.Invoke(null, args);
                return result switch
                {
                    null => string.Empty,
                    string s => s,
                    _ => result.ToString() ?? string.Empty,
                };
            }
            catch (TargetInvocationException tie) when (tie.InnerException is not null)
            {
                return $"{info.Name} threw: {tie.InnerException.Message}";
            }
            catch (Exception e)
            {
                return $"{info.Name} threw: {e.Message}";
            }
        }

        // --- helpers ---

        /// <summary>Whitespace-split a console input line. Phase 1 doesn't support quoted strings.</summary>
        public static List<string> Tokenize(string line)
        {
            // Multi-word string args need a Phase-2 enhancement (quoting).
            var result = new List<string>();
            var span = line.AsSpan().Trim();
            int start = 0;
            bool inToken = false;
            for (int i = 0; i < span.Length; i++)
            {
                if (char.IsWhiteSpace(span[i]))
                {
                    if (inToken)
                    {
                        result.Add(span.Slice(start, i - start).ToString());
                        inToken = false;
                    }
                }
                else
                {
                    if (!inToken) { start = i; inToken = true; }
                }
            }
            if (inToken)
            {
                result.Add(span.Slice(start).ToString());
            }
            return result;
        }

        internal static bool TryConvert(string token, Type targetType, out object? value, out string error)
        {
            error = string.Empty;
            value = null;
            try
            {
                if (targetType == typeof(string)) { value = token; return true; }
                if (targetType == typeof(int)) { value = int.Parse(token, CultureInfo.InvariantCulture); return true; }
                if (targetType == typeof(uint)) { value = uint.Parse(token, CultureInfo.InvariantCulture); return true; }
                if (targetType == typeof(long)) { value = long.Parse(token, CultureInfo.InvariantCulture); return true; }
                if (targetType == typeof(ulong)) { value = ulong.Parse(token, CultureInfo.InvariantCulture); return true; }
                if (targetType == typeof(float)) { value = float.Parse(token, CultureInfo.InvariantCulture); return true; }
                if (targetType == typeof(double)) { value = double.Parse(token, CultureInfo.InvariantCulture); return true; }
                if (targetType == typeof(bool))
                {
                    if (bool.TryParse(token, out var b)) { value = b; return true; }
                    if (token == "1") { value = true; return true; }
                    if (token == "0") { value = false; return true; }
                    error = $"expected bool, got '{token}'";
                    return false;
                }
                error = $"unsupported parameter type {targetType.Name}";
                return false;
            }
            catch (FormatException)
            {
                error = $"expected {targetType.Name}, got '{token}'";
                return false;
            }
            catch (OverflowException)
            {
                error = $"value '{token}' out of range for {targetType.Name}";
                return false;
            }
        }
    }
}
