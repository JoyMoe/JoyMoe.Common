using Parlot;

namespace JoyMoe.Common.Api.Filter.Terms;

public class Truth : Identity<bool>
{
    internal Truth(TextPosition position, bool value) : base(position, value) { }
}
