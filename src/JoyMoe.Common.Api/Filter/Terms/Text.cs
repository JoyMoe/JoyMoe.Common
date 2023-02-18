using Parlot;

namespace JoyMoe.Common.Api.Filter.Terms;

public class Text : Identity<string>
{
    internal Text(TextPosition position, string? value) : base(position, value) { }

    public bool EndsWith(string @string) {
        return Value?.EndsWith(@string) ?? false;
    }

    public bool EndsWith(char @char) {
        return Value?.EndsWith(@char) ?? false;
    }

    public bool StartsWith(string @string) {
        return Value?.StartsWith(@string) ?? false;
    }

    public bool StartsWith(char @char) {
        return Value?.StartsWith(@char) ?? false;
    }

    public Text Substring(int start, int length) {
        return Text(Position, Value?.Substring(start, length));
    }

    public Text Substring(Range range) {
        return Text(Position, Value?[range]);
    }

    public override string ToString() {
        return Value == null ? "NULL" : $"\"{Value}\"";
    }

    public Term this[Range range] => Substring(range);
}
