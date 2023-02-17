using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Operands;

public class And : Operand
{
    public const string Name = "AND";

    public override string DisplayName => Name;

    public And(TextPosition position, Term left, Term right) : base(position, left, right) { }

    public override Expression ToExpression(Container ctx) {
        return Expression.AndAlso(Left!.ToExpression(ctx), Right.ToExpression(ctx));
    }
}
