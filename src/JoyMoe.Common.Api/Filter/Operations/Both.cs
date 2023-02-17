using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Operations;

public class Both : LogicalOperation
{
    public const string Name = "BOTH";

    public override string DisplayName => Name;

    public Both(TextPosition position, Term left, Term right) : base(position, left, right) { }

    // TODO: Currently, we do not support ranking for both operands.
    public override ExpressionType ExpressionType => ExpressionType.AndAlso;
}
