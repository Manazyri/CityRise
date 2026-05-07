#nullable enable

using System;

namespace CityRise.Debug
{
    /// <summary>
    /// Marks a static method as a console command discoverable by <see cref="DebugConsoleRegistry"/>.
    /// </summary>
    /// <remarks>
    /// Phase 1 supports static methods only. Parameters of type int, uint, long, ulong, float,
    /// double, bool, and string are auto-converted from console tokens. Methods may return
    /// <c>string</c> (printed to console output) or <c>void</c> (no output). Argument count must
    /// match exactly; missing or extra args report a usage error.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class DebugCommandAttribute : Attribute
    {
        public string Name { get; }
        public string Description { get; }

        public DebugCommandAttribute(string name, string description = "")
        {
            if (string.IsNullOrEmpty(name)) throw new ArgumentException("name must be non-empty.", nameof(name));
            Name = name;
            Description = description ?? string.Empty;
        }
    }
}
