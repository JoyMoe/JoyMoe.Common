using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Operations;

public class Not : UnaryOperation
{
    public const string Name = "NOT";

    public override string DisplayName => Name;

    public Not(TextPosition position, Term right) : base(position, right) { }

    public override Expression ToExpression(Container ctx) {
        return Expression.Not(Right.ToExpression(ctx));
    }
}
