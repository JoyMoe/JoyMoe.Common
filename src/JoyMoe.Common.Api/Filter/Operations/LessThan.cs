using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Operations;

public class LessThan : ComparisonOperation
{
    public const string Name = "<";

    public override string DisplayName => "LT";

    public LessThan(TextPosition position, Term left, Term right) : base(position, left, right) { }

    public override ExpressionType ExpressionType => ExpressionType.LessThan;
}
