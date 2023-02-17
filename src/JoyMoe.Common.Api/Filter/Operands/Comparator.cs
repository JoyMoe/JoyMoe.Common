using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Operands;

public abstract class Comparator : Operand
{
    protected Comparator(TextPosition position, Term left, Term right) : base(position, left, right) { }

    public abstract ExpressionType ExpressionType { get; }

    public override Expression ToExpression(Container container) {
        var left  = Left!.ToExpression(container);
        var right = Right.ToExpression(container);

        return Expression.MakeBinary(ExpressionType, left, Expression.Convert(right, left.Type));
    }
}
