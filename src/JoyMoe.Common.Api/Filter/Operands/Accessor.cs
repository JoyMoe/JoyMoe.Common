using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operands;

public class Accessor : Operand
{
    public const string Name = ".";

    public override string DisplayName => Name;

    public Accessor(Term left, Term right) : base(left is Text name ? new Identifier(name.Value) : left, right) { }
}
