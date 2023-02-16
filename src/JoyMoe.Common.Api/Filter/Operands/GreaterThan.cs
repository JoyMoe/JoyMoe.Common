using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operands;

public class GreaterThan : Operand
{
    public const string Name = ">";

    public override string DisplayName => "GT";

    public GreaterThan(Term left, Term right) : base(left, right) { }
}
