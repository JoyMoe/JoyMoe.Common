namespace JoyMoe.Common.Api.Filter.Terms;

public class Number : Identity<decimal>
{
    public Number(decimal value) : base(value) { }

    public override string ToString() {
        return Value.ToString("G29");
    }
}
