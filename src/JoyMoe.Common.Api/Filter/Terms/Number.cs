using Parlot;

namespace JoyMoe.Common.Api.Filter.Terms;

public class Number : Identity<decimal>
{
    internal Number(TextPosition position, decimal value) : base(position, value) { }

    public override string ToString() {
        return Value.ToString("G29");
    }
}
