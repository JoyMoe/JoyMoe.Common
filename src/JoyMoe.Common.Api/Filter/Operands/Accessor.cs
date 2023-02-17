using System.Collections;
using System.Linq.Expressions;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Operands;

public class Accessor : Operand
{
    public const string Name = ".";

    public override string DisplayName => Name;

    public Accessor(TextPosition position, Term left, Term right) : base(position,
        left is Text name ? new Identifier(left.Position, name.Value) : left, right) { }

    public override Expression ToExpression(Container container) {
        var expression = Left!.ToExpression(container);

        if (typeof(IList).IsAssignableFrom(expression.Type)) {
            if (Right is not Integer { Value: < int.MaxValue } integer) {
                throw new ParseException("Expect array index", Right.Position);
            }

            return Expression.ArrayIndex(expression, Expression.Constant((int)integer.Value));
        }

        var property = Right switch {
            Identifier identifier => identifier.Value,
            Text text             => text.Value,
            _                     => throw new ParseException("Invalid field", Right.Position),
        };
        if (string.IsNullOrWhiteSpace(property)) {
            throw new ParseException("Expect field name", Right.Position);
        }

        if (typeof(IDictionary).IsAssignableFrom(expression.Type)) {
            return Expression.Property(expression, "Item", Expression.Constant(property));
        }

        try {
            return Expression.PropertyOrField(expression, property);
        } catch (ArgumentException) {
            throw new ParseException($"Invalid field name '{property}'", Right.Position);
        }
    }
}
