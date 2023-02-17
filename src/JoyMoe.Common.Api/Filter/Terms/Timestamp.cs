using Parlot;

namespace JoyMoe.Common.Api.Filter.Terms;

public class Timestamp : Identity<DateTimeOffset>
{
    public Timestamp(TextPosition position, DateTimeOffset value) : base(position, value) { }

    public override string ToString() {
        return Value.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFFZ");
    }
}
