using JoyMoe.Common.Api.Filter.Terms;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Operands;

public abstract class UnaryOperand : Operand
{
    public UnaryOperand(TextPosition position, Term right) : base(position, null, right) { }

    public override string ToString() {
        var name = DisplayName ?? GetType().ToString();

        return $"{name}({Right})";
    }
}
