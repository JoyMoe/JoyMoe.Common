using JoyMoe.Common.Api.Filter.Operations;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;

namespace JoyMoe.Common.Api.Filter;

public class Parser
{
    private readonly Scanner _scanner;

    private Parser(string text) {
        _scanner = new Scanner(text);
    }

    public static Parser Read(string text) => new(text);

    /**
     * filter : [expression];
     */
    public Term? Parse() {
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
                var next     = ParseSequence() ?? throw new ParseException("Expect sequence after 'AND'", position);

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

            expression += next;
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
                var next     = ParseTerm() ?? throw new ParseException("Expect term after 'OR'", position);

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
            (_scanner.ReadText(Not.Name, StringComparison.InvariantCultureIgnoreCase) &&
             _scanner.SkipWhiteSpaceOrNewLine())) {
            var inner = ParseSimple();

            if (inner == null) {
                throw new ParseException("Expect expression after '-'", _scanner.Cursor.Position);
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
                expression = Term.Identifier(expression.Position, text.Value);
            }

            _scanner.SkipWhiteSpaceOrNewLine();

            var current = _scanner.Cursor.Position;
            var next    = ParseArg();
            if (next == null) {
                throw new ParseException($"Expect arg after '{op.GetText()}'", current);
            }

            expression = op.GetText() switch {
                LessThanOrEqual.Name    => Term.LessThanOrEqual(expression, next),
                LessThan.Name           => Term.LessThan(expression, next),
                GreaterThanOrEqual.Name => Term.GreaterThanOrEqual(expression, next),
                GreaterThan.Name        => Term.GreaterThan(expression, next),
                NotEqual.Name           => Term.NotEqual(expression, next),
                Equal.Name              => Term.Equal(expression, next),
                Has.Name                => Term.Has(expression, next),
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
                throw new ParseException("Expect ')'", _scanner.Cursor.Position);
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
            var current = _scanner.Cursor.Position;
            var next    = ParseValue();
            if (next == null) {
                throw new ParseException("Expect value", current);
            }

            expression = Term.Accessor(expression, next);
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

        name = Term.Identifier(name.Position, text.Value);

        while (_scanner.ReadChar(Accessor.Name[0])) {
            var next = ParseText();
            if (next is not Text selector) {
                _scanner.Cursor.ResetPosition(start);
                return null;
            }

            name = Term.Accessor(name, Term.Identifier(next.Position, selector.Value));
        }

        if (_scanner.ReadChar('(')) {
            var expression = ParseList();

            if (!_scanner.ReadChar(')')) {
                throw new ParseException("Expect ')'", _scanner.Cursor.Position);
            }

            return Term.Function(name, expression);
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

        var start = _scanner.Cursor.Position;
        if (_scanner.ReadQuotedString(out var @string)) {
            return Term.Text(start, @string.Span[1..^1]);
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
            if (_scanner.ReadChar('e')) {
                var inverse = _scanner.ReadChar('-');
                if (!_scanner.ReadInteger(out var exponent)) {
                    throw new ParseException("Expect exponent", _scanner.Cursor.Position);
                }

                return Term.Number(start, number.Span, negative, exponent.Span, inverse);
            }

            if (_scanner.ReadChar('s')) {
                if (negative) {
                    throw new ParseException("Expect positive duration", _scanner.Cursor.Position);
                }

                return Term.Duration(start, number.Span);
            }

            if (number.Span.IndexOf('.') == -1) {
                return Term.Integer(start, number.Span, negative);
            }

            return Term.Number(start, number.Span, negative);
        }

        if (_scanner.ReadInteger(out var integer)) {
            return Term.Integer(start, integer.Span, negative);
        }

        _scanner.Cursor.ResetPosition(start);

        if (_scanner.ReadText("true", StringComparison.InvariantCultureIgnoreCase)) {
            return Term.Truth(start, true);
        }

        if (_scanner.ReadText("false", StringComparison.InvariantCultureIgnoreCase)) {
            return Term.Truth(start, false);
        }

        if (_scanner.ReadText("null", StringComparison.InvariantCultureIgnoreCase)) {
            return Term.Null(start);
        }

        if (_scanner.ReadWhile(static x => !new[] {
                                               '-',                                                               //
                                               LessThan.Name[0], GreaterThan.Name[0], Equal.Name[0], Has.Name[0], //
                                               '(', ')',                                                          //
                                               Accessor.Name[0], ',',
                                           }.Contains(x) &&
                                           !Character.IsWhiteSpace(x), out var text)) {
            return Term.Text(start, text.Span);
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
                throw new ParseException("Expect arg", position);
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

            if (_scanner.ReadNonWhiteSpace(out var result) && Term.Timestamp(start, result.Span, out var timestamp)) {
                return timestamp;
            }
        }

        _scanner.Cursor.ResetPosition(start);

        return null;
    }
}
