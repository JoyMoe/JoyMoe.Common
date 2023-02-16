using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operands;

public class Function : Operand
{
    public List<Term>? Parameters { get; }

    public Function(Term left, List<Term>? right) : base(left, left) {
        Parameters = right;
    }

    public override string ToString() {
        return $"{Left}({string.Join(", ", Parameters?.Select(p => p.ToString()) ?? Array.Empty<string>())})";
    }
}
