using System.Linq.Expressions;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Terms;

public class Identifier : Identity<string>
{
    public Identifier(TextPosition position, string? value) : base(position, value) { }

    public override Expression ToExpression(Container container) {
        if (!container.TryGetParameter(Value, out var value)) {
            throw new ParseException($"Unknown parameter: {Value}", Position);
        }

        return value;
    }
}
