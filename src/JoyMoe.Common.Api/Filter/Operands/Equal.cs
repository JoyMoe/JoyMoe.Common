using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operands;

public class Equal : Operand
{
    public const string Name = "=";

    public override string DisplayName => "EQ";

    public Equal(Term left, Term right) : base(left, right) { }
}
