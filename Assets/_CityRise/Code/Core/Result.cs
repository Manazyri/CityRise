#nullable enable

using System;

namespace CityRise.Core;

/// <summary>
/// A success value or an error message. Used in place of exceptions in sim code per ADR-0005.
/// </summary>
/// <remarks>
/// Phase 0 carries the error as a string for simplicity. Phase 1+ may upgrade to a typed
/// CommandRejectionReason (Tech Roadmap section 4.3). The Map/Bind shape doesn't depend on
/// that, so call-site changes will be local.
/// </remarks>
public readonly struct Result<T>
{
    private readonly T _value;
    private readonly string? _error;

    private Result(T value, string? error)
    {
        _value = value;
        _error = error;
    }

    public bool IsOk => _error is null;
    public bool IsErr => _error is not null;

    /// <summary>Throws if <see cref="IsErr"/>. Use <see cref="TryGet"/> when you don't want to throw.</summary>
    public T Value => IsOk
        ? _value
        : throw new InvalidOperationException($"Result is Err: {_error}");

    /// <summary>Throws if <see cref="IsOk"/>. Use <see cref="TryGet"/> when you don't want to throw.</summary>
    public string Error => _error
        ?? throw new InvalidOperationException("Result is Ok; no error to read.");

    public static Result<T> Ok(T value) => new(value, null);

    public static Result<T> Err(string error)
    {
        if (string.IsNullOrEmpty(error))
            throw new ArgumentException("Error message must be non-empty.", nameof(error));
        return new Result<T>(default!, error);
    }

    /// <summary>Pattern-match form. <paramref name="value"/> is meaningful only when this returns true.</summary>
    public bool TryGet(out T value, out string error)
    {
        value = _value;
        error = _error ?? string.Empty;
        return IsOk;
    }

    /// <summary>Apply <paramref name="f"/> to the success value; pass through the error.</summary>
    public Result<U> Map<U>(Func<T, U> f)
    {
        if (f is null) throw new ArgumentNullException(nameof(f));
        return IsOk ? Result<U>.Ok(f(_value)) : Result<U>.Err(_error!);
    }

    /// <summary>Chain a fallible step on the success value; pass through the error.</summary>
    public Result<U> Bind<U>(Func<T, Result<U>> f)
    {
        if (f is null) throw new ArgumentNullException(nameof(f));
        return IsOk ? f(_value) : Result<U>.Err(_error!);
    }

    public override string ToString() => IsOk ? $"Ok({_value})" : $"Err({_error})";
}
