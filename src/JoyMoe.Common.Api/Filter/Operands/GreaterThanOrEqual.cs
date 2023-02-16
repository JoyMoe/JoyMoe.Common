using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operands;

public class GreaterThanOrEqual : Operand
{
    public const string Name = ">=";

    public override string DisplayName => "GTE";

    public GreaterThanOrEqual(Term left, Term right) : base(left, right) { }
}
