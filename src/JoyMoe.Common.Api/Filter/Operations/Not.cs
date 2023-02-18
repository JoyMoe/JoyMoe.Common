using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operations;

public class Not : UnaryOperation
{
    public const string Name = "NOT";

    public override string DisplayName => Name;

    internal Not(Term right) : base(right) { }

    public override Expression ToExpression(Container ctx) {
        return Expression.Not(Right.ToExpression(ctx));
    }
}
