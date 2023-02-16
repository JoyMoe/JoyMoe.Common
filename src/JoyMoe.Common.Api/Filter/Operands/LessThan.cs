using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operands;

public class LessThan : Operand
{
    public const string Name = "<";

    public override string DisplayName => "LT";

    public LessThan(Term left, Term right) : base(left, right) { }
}
