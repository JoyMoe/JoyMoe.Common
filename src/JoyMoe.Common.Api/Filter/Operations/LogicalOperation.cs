using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Operations;

public abstract class LogicalOperation : Operation
{
    public LogicalOperation(TextPosition position, Term left, Term right) : base(position, left, right) { }

    public abstract ExpressionType ExpressionType { get; }

    public override Expression ToExpression(Container ctx) {
        var left  = Left!.ToExpression(ctx);
        var right = Right.ToExpression(ctx);

        if (left.Type != typeof(bool)) {
            left = new Match(Left.Position, new Identifier(Left.Position, "q"), Left).ToExpression(ctx);
        }

        if (right.Type != typeof(bool)) {
            right = new Match(Right.Position, new Identifier(Right.Position, "q"), Right).ToExpression(ctx);
        }

        return Expression.MakeBinary(ExpressionType, left, right);
    }
}
