using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operands;

public class Has : Operand
{
    public const string Name = ":";

    public override string DisplayName => "HAS";

    public Has(Term left, Term right) : base(left, right) { }
}
