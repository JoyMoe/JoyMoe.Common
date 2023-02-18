using Parlot;

namespace JoyMoe.Common.Api.Filter.Terms;

public class Duration : Identity<TimeSpan>
{
    internal Duration(TextPosition position, TimeSpan value) : base(position, value) { }

    public override string ToString() {
        return $"{Value.TotalSeconds:G}s";
    }
}
