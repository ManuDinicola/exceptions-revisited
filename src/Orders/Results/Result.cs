namespace Orders.Results;

/// <summary>A classic hand-rolled Result: a success value OR an <see cref="Error"/>.</summary>
/// <remarks>
/// Success is tracked with an explicit flag rather than "error is null", so that
/// <c>default(Result&lt;T&gt;)</c> is NOT silently a success carrying a null value.
/// </remarks>
public readonly struct Result<T>
{
    private readonly T? _value;
    private readonly Error? _error;
    private readonly bool _isSuccess;

    private Result(T value)
    {
        _value = value;
        _error = null;
        _isSuccess = true;
    }

    private Result(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        _value = default;
        _error = error;
        _isSuccess = false;
    }

    public bool IsSuccess => _isSuccess;

    public T Value => _isSuccess ? _value! : throw new InvalidOperationException("Result is a failure.");

    public Error Error => _error ?? throw new InvalidOperationException(
        _isSuccess ? "Result is a success." : "Result is uninitialized (default).");

#pragma warning disable CA2225 // The implicit conversions ARE the API; named alternatives would only add noise here.
    public static implicit operator Result<T>(T value) => new(value);

    public static implicit operator Result<T>(Error error) => new(error);
#pragma warning restore CA2225

    public TOut Match<TOut>(Func<T, TOut> ok, Func<Error, TOut> fail)
    {
        ArgumentNullException.ThrowIfNull(ok);
        ArgumentNullException.ThrowIfNull(fail);
        return _isSuccess ? ok(_value!) : fail(Error);
    }
}
