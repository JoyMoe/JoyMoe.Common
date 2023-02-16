using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operands;

public class Both : Operand
{
    public const string Name = "BOTH";

    public override string DisplayName => Name;

    public Both(Term left, Term right) : base(left, right) { }
}
