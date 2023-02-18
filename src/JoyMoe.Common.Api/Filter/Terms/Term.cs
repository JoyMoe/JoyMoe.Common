using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Operations;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Terms;

public abstract class Term
{
    public TextPosition Position { get; }

    protected Term(TextPosition position) {
        Position = position;
    }

    public abstract Expression ToExpression(Container ctx);

    #region Operation Factory

    public static Accessor Accessor(Term left, Term right) {
        return new Accessor(left, right);
    }

    public static Function Function(Term left, List<Term>? right) {
        return new Function(left, right);
    }

    public static Both Both(Term left, Term right) {
        return new Both(left, right);
    }

    public static And And(Term left, Term right) {
        return new And(left, right);
    }

    public static Or Or(Term left, Term right) {
        return new Or(left, right);
    }

    public static Term Not(Term right) {
        return right switch {
            Number number   => Number(right.Position, -number.Value),
            Integer integer => Integer(right.Position, -integer.Value),
            Not not         => not.Right,
            _               => new Not(right),
        };
    }

    public static Term Equal(Term left, Term right) {
        return right switch {
            Text text when text.StartsWith('*') => SuffixMatch(left, text[1..]),
            Text text when text.EndsWith('*')   => PrefixMatch(left, text[..^1]),
            _                                   => new Equal(left, right),
        };
    }

    public static NotEqual NotEqual(Term left, Term right) {
        return new NotEqual(left, right);
    }

    public static LessThan LessThan(Term left, Term right) {
        return new LessThan(left, right);
    }

    public static LessThanOrEqual LessThanOrEqual(Term left, Term right) {
        return new LessThanOrEqual(left, right);
    }

    public static GreaterThan GreaterThan(Term left, Term right) {
        return new GreaterThan(left, right);
    }

    public static GreaterThanOrEqual GreaterThanOrEqual(Term left, Term right) {
        return new GreaterThanOrEqual(left, right);
    }

    public static PrefixMatch PrefixMatch(Term left, Term right) {
        return new PrefixMatch(left, right);
    }

    public static Match Match(Term left, Term right) {
        return new Match(left, right);
    }

    public static SuffixMatch SuffixMatch(Term left, Term right) {
        return new SuffixMatch(left, right);
    }

    public static Has Has(Term left, Term right) {
        return new Has(left, right);
    }

    #endregion

    #region Identity Factory

    public static Text Text(TextPosition position, ReadOnlySpan<char> value) {
        return new Text(position, value.ToString());
    }

    public static Identifier Identifier(TextPosition position, ReadOnlySpan<char> value) {
        return new Identifier(position, value.ToString());
    }

    public static Null Null(TextPosition position) {
        return new Null(position);
    }

    public static Truth Truth(TextPosition position, bool value) {
        return new Truth(position, value);
    }

    public static Integer Integer(TextPosition position, long value) {
        return new Integer(position, value);
    }

    public static Integer Integer(TextPosition position, ReadOnlySpan<char> value, bool negative = false) {
        var @long = long.Parse(value, provider: CultureInfo.InvariantCulture);
        if (negative) {
            @long = -@long;
        }

        return new Integer(position, @long);
    }

    public static Number Number(TextPosition position, decimal value) {
        return new Number(position, value);
    }

    public static Number Number(TextPosition position, ReadOnlySpan<char> value, bool negative = false) {
        var @decimal = decimal.Parse(value, provider: CultureInfo.InvariantCulture);
        if (negative) {
            @decimal = -@decimal;
        }

        return new Number(position, @decimal);
    }

    public static Number Number(
        TextPosition       position,
        ReadOnlySpan<char> value,
        bool               negative,
        ReadOnlySpan<char> exponent,
        bool               inverse = false) {
        var @decimal = decimal.Parse(value, provider: CultureInfo.InvariantCulture);
        if (negative) {
            @decimal = -@decimal;
        }

        if (exponent.Length > 0) {
            var exp = int.Parse(exponent, provider: CultureInfo.InvariantCulture);
            if (inverse) {
                exp = -exp;
            }

            @decimal *= (decimal)Math.Pow(10, exp);
        }

        return new Number(position, @decimal);
    }

    public static Duration Duration(TextPosition position, TimeSpan value) {
        return new Duration(position, value);
    }

    public static Duration Duration(TextPosition position, double value) {
        return new Duration(position, TimeSpan.FromSeconds(value));
    }

    public static Duration Duration(TextPosition position, ReadOnlySpan<char> value) {
        var @double = double.Parse(value, provider: CultureInfo.InvariantCulture);
        return new Duration(position, TimeSpan.FromSeconds(@double));
    }

    public static Timestamp Timestamp(TextPosition position, DateTimeOffset value) {
        return new Timestamp(position, value);
    }

    public static bool Timestamp(
        TextPosition                         position,
        ReadOnlySpan<char>                   value,
        [MaybeNullWhen(false)] out Timestamp timestamp) {
        if (DateTimeOffset.TryParseExact(value, "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK", null,
                DateTimeStyles.AdjustToUniversal, out var date)) {
            timestamp = new Timestamp(position, date);
            return true;
        }

        timestamp = null;
        return false;
    }

    #endregion

    public abstract override bool Equals(object? obj);

    public abstract override int GetHashCode();

    public abstract override string ToString();

    public static Both operator +(Term left, Term right) {
        return Both(left, right);
    }

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
