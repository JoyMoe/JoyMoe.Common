using Parlot;

namespace JoyMoe.Common.Api.Filter.Terms;

public abstract class Operand : Term
{
    public virtual string? DisplayName { get; }

    public virtual Term? Left { get; }

    public virtual Term Right { get; }

    public Operand(TextPosition position, Term? left, Term right) : base(position) {
        Left  = left;
        Right = right;
    }

    public override bool Equals(object? obj) {
        if (ReferenceEquals(null, obj)) {
            return false;
        }

        return obj is Operand other &&
               DisplayName == other.DisplayName &&
               Equals(Left, other.Left) &&
               Equals(Right, other.Right);
    }

    public override int GetHashCode() {
        return DisplayName?.GetHashCode() ?? 0 + Left?.GetHashCode() ?? 0 + Right.GetHashCode();
    }

    public override string ToString() {
        var name = DisplayName ?? GetType().ToString();

        return $"{name}({Left}, {Right})";
    }
}
