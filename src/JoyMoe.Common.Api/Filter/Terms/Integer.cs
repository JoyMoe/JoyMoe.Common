using Parlot;

namespace JoyMoe.Common.Api.Filter.Terms;

public class Integer : Identity<long>
{
    internal Integer(TextPosition position, long value) : base(position, value) { }
}
