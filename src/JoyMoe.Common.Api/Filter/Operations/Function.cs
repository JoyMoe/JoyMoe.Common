using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Operations;

public class Function : Operation
{
    public List<Term>? Parameters { get; }

    internal Function(Term left, List<Term>? right) : base(left.Position, null, left) {
        Parameters = right;
    }

    public override Expression ToExpression(Container ctx) {
        if (Right is Identifier identifier && ctx.TryGetExpression(identifier.Value, out var expression)) {
            return expression;
        }

        if (Right is not Accessor { Right: Identifier text } accessor) {
            throw new ParseException("Invalid call", Position);
        }

        var instance = accessor.Left!.ToExpression(ctx);
        var method   = text.Value!;

        return Expression.Call(instance, method, null, Parameters?.Select(p => p.ToExpression(ctx)).ToArray());
    }

    public override string ToString() {
        return $"{Right}({string.Join(", ", Parameters?.Select(p => p.ToString()) ?? Array.Empty<string>())})";
    }
}
