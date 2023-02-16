namespace JoyMoe.Common.Api.Filter.Terms;

public class Timestamp : Identity<DateTimeOffset>
{
    public Timestamp(DateTimeOffset value) : base(value) { }

    public override string ToString() {
        return Value.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFZ");
    }
}
