#nullable enable

using System;
using System.Collections.Generic;

namespace CityRise.Core;

/// <summary>
/// Trivial typed service registry for Bootstrap-time wiring. Not a DI framework — Bootstrap
/// constructs services explicitly and registers them here so other startup code can resolve
/// shared instances. Systems and tools receive their dependencies via constructor injection
/// rather than looking them up here.
/// </summary>
public sealed class ServiceContainer
{
    private readonly Dictionary<Type, object> _services = new();

    /// <summary>Register a service under its declared type <typeparamref name="T"/>. Replaces any prior registration.</summary>
    public void Register<T>(T service) where T : class
    {
        if (service is null) throw new ArgumentNullException(nameof(service));
        _services[typeof(T)] = service;
    }

    /// <summary>Resolve a service by type. Throws if not registered.</summary>
    public T Get<T>() where T : class
    {
        if (_services.TryGetValue(typeof(T), out var instance)) return (T)instance;
        throw new InvalidOperationException($"Service of type {typeof(T)} is not registered.");
    }

    /// <summary>Resolve a service by type. Returns false if not registered.</summary>
    public bool TryGet<T>(out T? service) where T : class
    {
        if (_services.TryGetValue(typeof(T), out var instance))
        {
            service = (T)instance;
            return true;
        }
        service = null;
        return false;
    }

    /// <summary>True if a service of type <typeparamref name="T"/> is registered.</summary>
    public bool IsRegistered<T>() where T : class => _services.ContainsKey(typeof(T));

    /// <summary>Number of registered services. Test-only.</summary>
    public int Count => _services.Count;
}
