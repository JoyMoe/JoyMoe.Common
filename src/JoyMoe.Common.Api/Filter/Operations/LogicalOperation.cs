using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operations;

public abstract class LogicalOperation : Operation
{
    protected LogicalOperation(Term left, Term right) : base(left.Position, left, right) { }

    public abstract ExpressionType ExpressionType { get; }

    public override Expression ToExpression(Container ctx) {
        var left  = Left!.ToExpression(ctx);
        var right = Right.ToExpression(ctx);

        if (left.Type != typeof(bool)) {
            left = Match(Identifier(Left.Position, "q"), Left).ToExpression(ctx);
        }

        if (right.Type != typeof(bool)) {
            right = Match(Identifier(Right.Position, "q"), Right).ToExpression(ctx);
        }

        return Expression.MakeBinary(ExpressionType, left, right);
    }
}
