using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Operands;

public class NotEqual : Comparator
{
    public const string Name = "!=";

    public override string DisplayName => "NE";

    public NotEqual(TextPosition position, Term left, Term right) : base(position, left, right) { }

    public override ExpressionType ExpressionType => ExpressionType.NotEqual;
}
