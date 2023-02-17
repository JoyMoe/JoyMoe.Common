using Parlot;

namespace JoyMoe.Common.Api.Filter.Terms;

public class Text : Identity<string>
{
    public Text(TextPosition position, string? value) : base(position, value) { }

    public override string ToString() {
        return Value == null ? "NULL" : $"\"{Value}\"";
    }
}
