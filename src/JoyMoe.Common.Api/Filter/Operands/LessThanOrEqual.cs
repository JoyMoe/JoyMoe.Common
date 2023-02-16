using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operands;

public class LessThanOrEqual : Operand
{
    public const string Name = "<=";

    public override string DisplayName => "LTE";

    public LessThanOrEqual(Term left, Term right) : base(left, right) { }
}
