using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operations;

public class Match : Operation
{
    public override string DisplayName => "MATCH";

    internal Match(Term left, Term right) : base(left.Position, left, right) { }

    public override Expression ToExpression(Container ctx) {
        var left  = Left!.ToExpression(ctx);
        var right = Right.ToExpression(ctx);

        var contains = left.Type switch {
            _ when right.Type == typeof(char) => ctx.GetMethod(typeof(string), "Contains", new[] { typeof(char) }),
            _                                 => ctx.GetMethod(typeof(string), "Contains", new[] { typeof(string) }),
        };

        if (right.Type != typeof(char)) {
            right = Convert(right, typeof(string));
        }

        return Expression.Call(left, contains!, right);
    }
}
