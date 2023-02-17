using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Operands;

public class Function : Operand
{
    public List<Term>? Parameters { get; }

    public Function(TextPosition position, Term left, List<Term>? right) : base(position, left, left) {
        Parameters = right;
    }

    public override Expression ToExpression(Container ctx) {
        if (Left is Identifier identifier && ctx.TryGetExpression(identifier.Value, out var expression)) {
            return expression;
        }

        if (Left is not Accessor { Right: Identifier text } accessor) {
            throw new ParseException("Invalid call", Position);
        }

        var instance = accessor.Left!.ToExpression(ctx);
        var method   = text.Value!;

        return Expression.Call(instance, method, null, Parameters?.Select(p => p.ToExpression(ctx)).ToArray());
    }

    public override string ToString() {
        return $"{Left}({string.Join(", ", Parameters?.Select(p => p.ToString()) ?? Array.Empty<string>())})";
    }
}
