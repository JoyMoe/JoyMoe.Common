using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Operands;

public class Or : Operand
{
    public const string Name = "OR";

    public override string DisplayName => Name;

    public Or(TextPosition position, Term left, Term right) : base(position, left, right) { }

    public override Expression ToExpression(Container ctx) {
        return Expression.OrElse(Left!.ToExpression(ctx), Right.ToExpression(ctx));
    }
}
