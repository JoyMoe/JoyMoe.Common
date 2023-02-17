using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Operands;

public class LessThanOrEqual : Comparator
{
    public const string Name = "<=";

    public override string DisplayName => "LTE";

    public LessThanOrEqual(TextPosition position, Term left, Term right) : base(position, left, right) { }

    public override ExpressionType ExpressionType => ExpressionType.LessThanOrEqual;
}
