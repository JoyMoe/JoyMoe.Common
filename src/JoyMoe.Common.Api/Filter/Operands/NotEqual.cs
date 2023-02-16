using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operands;

public class NotEqual : Operand
{
    public const string Name = "!=";

    public override string DisplayName => "NE";

    public NotEqual(Term left, Term right) : base(left, right) { }
}
