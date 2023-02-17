using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Operands;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Terms;

public abstract class Term
{
    public TextPosition Position { get; }

    public Term(TextPosition position) {
        Position = position;
    }

    public abstract Expression ToExpression(Container ctx);

    public static And And(Term left, Term right) {
        return new And(left.Position, left, right);
    }

    public static Or Or(Term left, Term right) {
        return new Or(left.Position, left, right);
    }

    public static Term Not(Term right) {
        return right switch {
            Number number   => new Number(right.Position, -number.Value),
            Integer integer => new Integer(right.Position, -integer.Value),
            Not not         => not.Right,
            _               => new Not(right.Position, right),
        };
    }

    public abstract override bool Equals(object? obj);

    public abstract override int GetHashCode();

    public abstract override string ToString();

    public static And operator &(Term left, Term right) {
        return And(left, right);
    }

    public static Or operator |(Term left, Term right) {
        return Or(left, right);
    }

    public static Term operator !(Term right) {
        return Not(right);
    }
}
