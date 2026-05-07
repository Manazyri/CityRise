#nullable enable

using System;
using System.IO;
using System.Text;
using CityRise.App;
using CityRise.Persistence;
using CityRise.Presentation.Camera;
using CityRise.Simulation.Infrastructure;
using UnityEngine;

namespace CityRise.Debug
{
    /// <summary>
    /// Phase 1 starter command set. Each method is a static [DebugCommand] entry that
    /// <see cref="DebugConsoleRegistry"/> picks up at scan time. Looks up runtime services via
    /// <see cref="Bootstrap.Instance"/> rather than holding references — registry scan happens
    /// once but commands run repeatedly during a session.
    /// </summary>
    /// <remarks>
    /// Phase-1 starters per Tech Roadmap §6.3: <c>help</c>, <c>set_tick_rate</c>,
    /// <c>teleport_camera</c>, <c>dump_state</c>, <c>load_state</c>.
    /// <c>run_commands_from_file</c> follows in Phase 2.
    /// </remarks>
    public static class CityRiseDebugCommands
    {
        [DebugCommand("help", "List all commands or describe one: help [name]")]
        public static string Help(string name = "")
        {
            var registry = DebugConsole.ActiveRegistry;
            if (registry is null) return "Console registry not active.";

            if (!string.IsNullOrEmpty(name))
            {
                var info = registry.Find(name);
                if (info is null) return $"Unknown command '{name}'.";
                return string.IsNullOrEmpty(info.Description)
                    ? info.Usage()
                    : $"{info.Usage()}\n  {info.Description}";
            }

            var sb = new StringBuilder("Commands:");
            foreach (var info in registry.All)
            {
                sb.Append('\n').Append("  ").Append(info.Name);
                if (!string.IsNullOrEmpty(info.Description))
                    sb.Append(" — ").Append(info.Description);
            }
            return sb.ToString();
        }

        [DebugCommand("set_tick_rate", "Set sim speed: paused / normal / fast / faster (or 0/1/2/3)")]
        public static string SetTickRate(string speed)
        {
            var services = Bootstrap.Instance?.Services;
            if (services is null || !services.TryGet<TickScheduler>(out var scheduler) || scheduler is null)
                return "TickScheduler not registered. Is Bootstrap loaded?";

            if (TryParseSpeed(speed, out var multiplier))
            {
                scheduler.Speed = multiplier;
                return $"Tick speed: {multiplier} ({(int)multiplier}x).";
            }
            return $"Unknown speed '{speed}'. Try: paused, normal, fast, faster (or 0..3).";
        }

        [DebugCommand("teleport_camera", "Move the RTS camera rig to (x y z) world coordinates.")]
        public static string TeleportCamera(float x, float y, float z)
        {
            var rig = UnityEngine.Object.FindFirstObjectByType<RtsCameraController>(FindObjectsInactive.Include);
            if (rig is null) return "No RtsCameraController found in active scene.";
            rig.transform.position = new Vector3(x, y, z);
            return $"Camera rig at ({x:F2}, {y:F2}, {z:F2}).";
        }

        [DebugCommand("dump_state", "Save current state to a debug JSON file. Filename optional; defaults to debug_dump.json.")]
        public static string DumpState(string filename = "debug_dump.json")
        {
            if (!TryGetSaveService(out var svc, out var error)) return error;
            var path = ResolvePath(filename);
            var result = svc.Save(path);
            return result.IsOk ? $"Saved to {path}" : $"Save failed: {result.Error}";
        }

        [DebugCommand("load_state", "Load state from a debug JSON file. Filename optional; defaults to debug_dump.json.")]
        public static string LoadState(string filename = "debug_dump.json")
        {
            if (!TryGetSaveService(out var svc, out var error)) return error;
            var path = ResolvePath(filename);
            if (!File.Exists(path)) return $"Save file not found: {path}";
            var result = svc.Load(path);
            return result.IsOk ? $"Loaded from {path}" : $"Load failed: {result.Error}";
        }

        private static bool TryGetSaveService(out SaveService svc, out string error)
        {
            svc = null!;
            error = string.Empty;
            var services = Bootstrap.Instance?.Services;
            if (services is null) { error = "Bootstrap not loaded; SaveService unavailable."; return false; }
            if (!services.TryGet<SaveService>(out var found) || found is null)
            {
                error = "SaveService not registered.";
                return false;
            }
            svc = found;
            return true;
        }

        private static string ResolvePath(string filename)
        {
            if (Path.IsPathRooted(filename)) return filename;
            return Path.Combine(Application.persistentDataPath, filename);
        }

        private static bool TryParseSpeed(string token, out SpeedMultiplier multiplier)
        {
            switch (token.Trim().ToLowerInvariant())
            {
                case "0": case "pause": case "paused":   multiplier = SpeedMultiplier.Paused; return true;
                case "1": case "normal":                  multiplier = SpeedMultiplier.Normal; return true;
                case "2": case "fast":                    multiplier = SpeedMultiplier.Fast; return true;
                case "3": case "faster":                  multiplier = SpeedMultiplier.Faster; return true;
                default:
                    multiplier = SpeedMultiplier.Normal;
                    return false;
            }
        }
    }
}
