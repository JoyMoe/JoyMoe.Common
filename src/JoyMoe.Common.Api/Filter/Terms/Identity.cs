namespace JoyMoe.Common.Api.Filter.Terms;

public abstract class Identity<T> : Term
{
    public virtual T? Value { get; }

    public Identity(T? value) {
        Value = value;
    }

    public override bool Equals(object? obj) {
        if (ReferenceEquals(null, obj)) {
            return false;
        }

        return obj is Identity<T> other && Equals(Value, other.Value);
    }

    public override int GetHashCode() {
        return Value?.GetHashCode() ?? 0;
    }

    public override string ToString() {
        return Value?.ToString() ?? string.Empty;
    }
}
