using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operations;

public class Or : LogicalOperation
{
    public const string Name = "OR";

    public override string DisplayName => Name;

    internal Or(Term left, Term right) : base(left, right) { }

    public override ExpressionType ExpressionType => ExpressionType.OrElse;
}
