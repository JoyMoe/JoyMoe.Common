using System.Linq.Expressions;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Terms;

public class Null : Identity<bool>
{
    internal Null(TextPosition position) : base(position, false) { }

    public override Expression ToExpression(Container ctx) {
        return Expression.Constant(null);
    }

    public override bool Equals(object? obj) {
        return obj is Null;
    }

    public override int GetHashCode() {
        return 0;
    }

    public override string ToString() {
        return "NULL";
    }
}
