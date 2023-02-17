using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Operands;

public class Both : Operand
{
    public const string Name = "BOTH";

    public override string DisplayName => Name;

    public Both(TextPosition position, Term left, Term right) : base(position, left, right) { }

    public override Expression ToExpression(Container container) {
        // TODO: Currently, we do not support ranking for both operands.

        return Expression.AndAlso(Left!.ToExpression(container), Right.ToExpression(container));
    }
}
