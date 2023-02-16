using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operands;

public abstract class UnaryOperand : Operand
{
    public UnaryOperand(Term right) : base(right, right) { }

    public override string ToString() {
        var name = DisplayName ?? GetType().ToString();

        return $"{name}({Right})";
    }
}
