using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operations;

public class Both : LogicalOperation
{
    public const string Name = "BOTH";

    public override string DisplayName => Name;

    internal Both(Term left, Term right) : base(left, right) { }

    // TODO: Currently, we do not support ranking for both operands.
    public override ExpressionType ExpressionType => ExpressionType.AndAlso;
}
