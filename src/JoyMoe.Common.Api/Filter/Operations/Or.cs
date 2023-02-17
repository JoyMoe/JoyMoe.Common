using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Operations;

public class Or : LogicalOperation
{
    public const string Name = "OR";

    public override string DisplayName => Name;

    public Or(TextPosition position, Term left, Term right) : base(position, left, right) { }

    public override ExpressionType ExpressionType => ExpressionType.OrElse;
}
