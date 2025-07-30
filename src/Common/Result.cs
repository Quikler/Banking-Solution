namespace Common;

public readonly struct Result<TValue, TError>
{
    private readonly TValue? _value;
    private readonly TError? _error;

    private Result(TValue value)
    {
        IsError = false;
        _value = value;
        _error = default;
    }

    private Result(TError error)
    {
        IsError = true;
        _value = default;
        _error = error;
    }

    public bool IsError { get; }
    public bool IsSuccess => !IsError;

    public static implicit operator Result<TValue, TError>(TValue value) => new(value);
    public static implicit operator Result<TValue, TError>(TError error) => new(error);

    public TResult Match<TResult>(Func<TValue, TResult> success, Func<TError, TResult> failure)
        => IsSuccess ? success(_value!) : failure(_error!);

    public Task<TResult> MatchAsync<TResult>(Func<TValue, Task<TResult>> success, Func<TError, TResult> failure)
        => IsSuccess ? success(_value!) : Task.FromResult(failure(_error!));
}
