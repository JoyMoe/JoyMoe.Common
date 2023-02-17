using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Operations;

public abstract class ComparisonOperation : Operation
{
    protected ComparisonOperation(TextPosition position, Term left, Term right) : base(position, left, right) { }

    public abstract ExpressionType ExpressionType { get; }

    public override Expression ToExpression(Container ctx) {
        var left  = Left!.ToExpression(ctx);
        var right = Right.ToExpression(ctx);

        return Expression.MakeBinary(ExpressionType, left, Convert(right, left.Type));
    }
}
