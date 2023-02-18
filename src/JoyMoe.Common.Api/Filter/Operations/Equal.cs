using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operations;

public class Equal : ComparisonOperation
{
    public const string Name = "=";

    public override string DisplayName => "EQ";

    internal Equal(Term left, Term right) : base(left, right) { }

    public override ExpressionType ExpressionType => ExpressionType.Equal;
}
