using System.Globalization;
using JoyMoe.Common.Api.Filter.Operands;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;

namespace JoyMoe.Common.Api.Filter;

public class Parser
{
    private Scanner _scanner = null!;

    /**
     * filter : [expression];
     */
    public Term? Parse(string text) {
        _scanner = new Scanner(text);

        return ParseExpression();
    }

    /**
     * expression : sequence {WS AND WS sequence};
     */
    private Term? ParseExpression() {
        var expression = ParseSequence();
        if (expression == null) return null;

        while (true) {
            _scanner.SkipWhiteSpaceOrNewLine();

            if (_scanner.ReadText(And.Name)) {
                _scanner.SkipWhiteSpaceOrNewLine();

                var position = _scanner.Cursor.Position;
                var next     = ParseSequence() ?? throw new ParseException("Expected sequence after 'AND'", position);

                expression &= next;
            } else {
                break;
            }
        }

        return expression;
    }

    /**
     * sequence : factor {WS factor};
     */
    private Term? ParseSequence() {
        var expression = ParseFactor();
        if (expression == null) return null;

        while (!_scanner.Cursor.Eof) {
            _scanner.SkipWhiteSpaceOrNewLine();

            var next = ParseFactor();
            if (next == null) {
                return expression;
            }

            expression = new Both(expression, next);
        }

        return expression;
    }

    /**
     * factor : term {WS OR WS term};
     */
    private Term? ParseFactor() {
        var expression = ParseTerm();
        if (expression == null) return null;

        while (true) {
            _scanner.SkipWhiteSpaceOrNewLine();

            if (_scanner.ReadText(Or.Name)) {
                _scanner.SkipWhiteSpaceOrNewLine();

                var position = _scanner.Cursor.Position;
                var next     = ParseTerm() ?? throw new ParseException("Expected term after 'OR'", position);

                expression |= next;
            } else {
                break;
            }
        }

        return expression;
    }

    /**
     * term : [(NOT WS | MINUS)] simple;
     */
    private Term? ParseTerm() {
        if (_scanner.ReadChar('-') ||
            (_scanner.ReadText(Not.Name, StringComparison.InvariantCultureIgnoreCase) && _scanner.SkipWhiteSpaceOrNewLine())) {
            var inner = ParseSimple();

            if (inner == null) {
                throw new ParseException("Expected expression after '-'", _scanner.Cursor.Position);
            }

            return !inner;
        }

        return ParseSimple();
    }

    /**
     * simple : restriction | composite;
     */
    private Term? ParseSimple() {
        var expression = ParseComposite();
        return expression ?? ParseRestriction();
    }

    /**
     * restriction : comparable [comparator arg];
     * comparator
     *  : LESS_EQUALS
     *  | LESS_THAN
     *  | GREATER_EQUALS
     *  | GREATER_THAN
     *  | NOT_EQUALS
     *  | EQUALS
     *  | HAS
     *  ;
     */
    private Term? ParseRestriction() {
        var expression = ParseComparable();
        if (expression == null) return null;

        _scanner.SkipWhiteSpaceOrNewLine();

        if (_scanner.ReadText(LessThanOrEqual.Name, out var op) ||
            _scanner.ReadChar(LessThan.Name[0], out op) ||
            _scanner.ReadText(GreaterThanOrEqual.Name, out op) ||
            _scanner.ReadChar(GreaterThan.Name[0], out op) ||
            _scanner.ReadText(NotEqual.Name, out op) ||
            _scanner.ReadChar(Equal.Name[0], out op) ||
            _scanner.ReadChar(Has.Name[0], out op)) {
            if (expression is Text text) {
                expression = new Identifier(text.Value);
            }

            _scanner.SkipWhiteSpaceOrNewLine();

            var position = _scanner.Cursor.Position;
            var next     = ParseArg();
            if (next == null) {
                throw new ParseException($"Expected arg after '{op.GetText()}'", position);
            }

            expression = op.GetText() switch {
                LessThanOrEqual.Name    => new LessThanOrEqual(expression, next),
                LessThan.Name           => new LessThan(expression, next),
                GreaterThanOrEqual.Name => new GreaterThanOrEqual(expression, next),
                GreaterThan.Name        => new GreaterThan(expression, next),
                NotEqual.Name           => new NotEqual(expression, next),
                Equal.Name              => new Equal(expression, next),
                Has.Name                => new Has(expression, next),
                _                       => throw new NotSupportedException(),
            };
        }

        return expression;
    }

    /**
     * composite : LPAREN expression RPAREN;
     */
    private Term? ParseComposite() {
        if (_scanner.ReadChar('(')) {
            var expression = ParseExpression();

            if (!_scanner.ReadChar(')')) {
                throw new ParseException("Expected ')'", _scanner.Cursor.Position);
            }

            return expression;
        }

        return null;
    }

    /**
     * comparable : member | function;
     */
    private Term? ParseComparable() {
        var expression = ParseFunction();
        return expression ?? ParseMember();
    }

    /**
     * member : value {DOT field};
     */
    private Term? ParseMember() {
        var expression = ParseValue();
        if (expression == null) return null;

        while (_scanner.ReadChar(Accessor.Name[0])) {
            var position = _scanner.Cursor.Position;
            var next     = ParseValue();
            if (next == null) {
                throw new ParseException("Expected value", position);
            }

            expression = new Accessor(expression, next);
        }

        return expression;
    }

    /**
     * function : name {DOT name} LPAREN [argList] RPAREN;
     */
    private Term? ParseFunction() {
        var start = _scanner.Cursor.Position;

        var name = ParseText();
        if (name is not Text text) {
            _scanner.Cursor.ResetPosition(start);
            return null;
        }

        name = new Identifier(text.Value);

        while (_scanner.ReadChar(Accessor.Name[0])) {
            var next = ParseText();
            if (next is not Text selector) {
                _scanner.Cursor.ResetPosition(start);
                return null;
            }

            name = new Accessor(name, new Identifier(selector.Value));
        }

        if (_scanner.ReadChar('(')) {
            var expression = ParseList();

            if (!_scanner.ReadChar(')')) {
                throw new ParseException("Expected ')'", _scanner.Cursor.Position);
            }

            return new Function(name, expression);
        }

        _scanner.Cursor.ResetPosition(start);

        return null;
    }

    /**
     * value : TEXT | STRING;
     * field : value | keyword;
     */
    private Term? ParseValue() {
        if (MeetKeyword()) return null;

        if (_scanner.ReadQuotedString(out var @string)) {
            return new Text(@string.GetText()[1..^1]);
        }

        return ParseText();
    }

    /**
     * name : TEXT | keyword;
     */
    private Term? ParseText() {
        if (MeetKeyword()) return null;

        var timestamp = ParseTimestamp();
        if (timestamp != null) {
            return timestamp;
        }

        var start = _scanner.Cursor.Position;

        var negative = _scanner.ReadChar('-');
        if (_scanner.ReadDecimal(out var number)) {
            var @decimal = decimal.Parse(number.Span, provider: CultureInfo.InvariantCulture);
            if (negative) {
                @decimal = -@decimal;
            }

            if (_scanner.ReadChar('e')) {
                if (!_scanner.ReadInteger(out var exponent)) {
                    throw new ParseException("Expected exponent", _scanner.Cursor.Position);
                }

                return new Number(@decimal * (decimal)Math.Pow(10, double.Parse(exponent.Span)));
            }

            if (_scanner.ReadChar('s')) {
                if (negative) {
                    throw new ParseException("Expected positive duration", _scanner.Cursor.Position);
                }

                return new Duration(TimeSpan.FromSeconds((double)@decimal));
            }

            if (number.Span.IndexOf('.') == -1) {
                var @long = long.Parse(number.Span);
                if (negative) {
                    @long = -@long;
                }

                return new Integer(@long);
            }

            return new Number(@decimal);
        }

        if (_scanner.ReadInteger(out var integer)) {
            var @long = long.Parse(integer.Span);
            if (negative) {
                @long = -@long;
            }

            return new Integer(@long);
        }

        _scanner.Cursor.ResetPosition(start);

        if (_scanner.ReadText("true", StringComparison.InvariantCultureIgnoreCase)) {
            return new Truth(true);
        }

        if (_scanner.ReadText("false", StringComparison.InvariantCultureIgnoreCase)) {
            return new Truth(false);
        }

        if (_scanner.ReadText("null", StringComparison.InvariantCultureIgnoreCase)) {
            return new Text(null);
        }

        if (_scanner.ReadWhile(static x => !new[] {
                                               '-',                                                               //
                                               LessThan.Name[0], GreaterThan.Name[0], Equal.Name[0], Has.Name[0], //
                                               '(', ')',                                                          //
                                               Accessor.Name[0], ',',
                                           }.Contains(x) &&
                                           !Character.IsWhiteSpace(x), out var text)) {
            return new Text(text.GetText());
        }

        _scanner.Cursor.ResetPosition(start);

        return null;
    }

    /**
     * argList : arg {COMMA arg};
     */
    private List<Term>? ParseList() {
        var expression = ParseArg();
        if (expression == null) return null;

        var list = new List<Term> { expression };

        while (_scanner.ReadChar(',')) {
            _scanner.SkipWhiteSpaceOrNewLine();

            var position = _scanner.Cursor.Position;
            var next     = ParseArg();
            if (next == null) {
                throw new ParseException("Expected arg", position);
            }

            list.Add(next);
        }

        return list;
    }

    /**
     * arg : comparable | composite;
     */
    private Term? ParseArg() {
        var expression = ParseComposite();
        return expression ?? ParseComparable();
    }

    /**
     * keyword : NOT | AND | OR;
     */
    private bool MeetKeyword() {
        var start = _scanner.Cursor.Position;

        var result = _scanner.ReadText(Not.Name) || _scanner.ReadText(And.Name) || _scanner.ReadText(Or.Name);

        _scanner.Cursor.ResetPosition(start);

        return result;
    }

    private Timestamp? ParseTimestamp() {
        var start = _scanner.Cursor.Position;

        if (_scanner.ReadInteger() &&
            _scanner.ReadChar('-') &&
            _scanner.ReadInteger() &&
            _scanner.ReadChar('-') &&
            _scanner.ReadInteger() &&
            _scanner.ReadChar('T')) {
            _scanner.Cursor.ResetPosition(start);

            if (_scanner.ReadNonWhiteSpace(out var result)) {
                if (DateTimeOffset.TryParseExact(result.Span, "yyyy-MM-dd'T'HH:mm:ss.FFFFFFFK", null,
                        DateTimeStyles.AdjustToUniversal, out var date)) {
                    return new Timestamp(date);
                }
            }
        }

        _scanner.Cursor.ResetPosition(start);

        return null;
    }
}
