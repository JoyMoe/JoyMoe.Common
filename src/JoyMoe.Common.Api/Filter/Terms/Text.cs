namespace JoyMoe.Common.Api.Filter.Terms;

public class Text : Identity<string>
{
    public Text(string? value) : base(value) { }

    public override string ToString() {
        return Value == null ? "NULL" : $"\"{Value}\"";
    }
}
