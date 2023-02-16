using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operands;

public class Not : UnaryOperand
{
    public const string Name = "NOT";

    public override string DisplayName => Name;

    public Not(Term right) : base(right) { }
}
