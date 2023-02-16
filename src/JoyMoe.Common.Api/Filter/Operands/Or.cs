using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operands;

public class Or : Operand
{
    public const string Name = "OR";

    public override string DisplayName => Name;

    public Or(Term left, Term right) : base(left, right) { }
}
