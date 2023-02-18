using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operations;

public class LessThanOrEqual : ComparisonOperation
{
    public const string Name = "<=";

    public override string DisplayName => "LTE";

    internal LessThanOrEqual(Term left, Term right) : base(left, right) { }

    public override ExpressionType ExpressionType => ExpressionType.LessThanOrEqual;
}
