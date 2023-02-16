using JoyMoe.Common.Api.Filter.Operands;

namespace JoyMoe.Common.Api.Filter.Terms;

public abstract class Term
{
    public static And And(Term left, Term right) {
        return new And(left, right);
    }

    public static Or Or(Term left, Term right) {
        return new Or(left, right);
    }

    public static Term Not(Term right) {
        return right switch {
            Number number   => new Number(-number.Value),
            Integer integer => new Integer(-integer.Value),
            Not not         => not.Right,
            _               => new Not(right),
        };
    }

    public abstract override bool Equals(object? obj);

    public abstract override int GetHashCode();

    public abstract override string ToString();

    public static And operator &(Term left, Term right) {
        return new And(left, right);
    }

    public static Or operator |(Term left, Term right) {
        return new Or(left, right);
    }

    public static Term operator !(Term right) {
        return Not(right);
    }
}
