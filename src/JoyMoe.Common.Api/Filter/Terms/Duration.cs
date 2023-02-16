namespace JoyMoe.Common.Api.Filter.Terms;

public class Duration : Identity<TimeSpan>
{
    public Duration(TimeSpan value) : base(value) { }

    public override string ToString() {
        return $"{Value.TotalSeconds:G}s";
    }
}
