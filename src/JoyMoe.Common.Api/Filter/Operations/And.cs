using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operations;

public class And : LogicalOperation
{
    public const string Name = "AND";

    public override string DisplayName => Name;

    internal And(Term left, Term right) : base(left, right) { }

    public override ExpressionType ExpressionType => ExpressionType.AndAlso;
}
