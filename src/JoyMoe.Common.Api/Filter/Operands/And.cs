using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operands;

public class And : Operand
{
    public const string Name = "AND";

    public override string DisplayName => Name;

    public And(Term left, Term right) : base(left, right) { }
}
