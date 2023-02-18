using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operations;

public class NotEqual : ComparisonOperation
{
    public const string Name = "!=";

    public override string DisplayName => "NE";

    internal NotEqual(Term left, Term right) : base(left, right) { }

    public override ExpressionType ExpressionType => ExpressionType.NotEqual;
}
