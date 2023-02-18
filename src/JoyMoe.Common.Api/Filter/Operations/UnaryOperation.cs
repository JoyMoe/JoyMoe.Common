using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter.Operations;

public abstract class UnaryOperation : Operation
{
    protected UnaryOperation(Term right) : base(right.Position, null, right) { }

    public override string ToString() {
        var name = DisplayName ?? GetType().ToString();

        return $"{name}({Right})";
    }
}
