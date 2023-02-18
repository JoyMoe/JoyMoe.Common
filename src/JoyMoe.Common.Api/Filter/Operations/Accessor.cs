using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Operations;

public class Accessor : Operation
{
    public const string Name = ".";

    public override string DisplayName => Name;

    internal Accessor(Term left, Term right) : base(left.Position,
        left is Text name ? Identifier(left.Position, name.Value) : left, right) { }

    public override Expression ToExpression(Container ctx) {
        var expression = Left!.ToExpression(ctx);

        var property = Right switch {
            Identifier identifier => identifier.Value,
            Text text             => text.Value,
            Integer integer       => integer.Value.ToString(),
            _                     => throw new ParseException("Invalid field", Right.Position),
        };
        if (string.IsNullOrWhiteSpace(property)) {
            throw new ParseException("Expect field name or indice", Right.Position);
        }

        if (typeof(IDictionary).IsAssignableFrom(expression.Type)) {
            return Expression.Property(expression, "Item", Expression.Constant(property));
        }

        if (typeof(IEnumerable).IsAssignableFrom(expression.Type)) {
            if (Right is not Integer { Value: < int.MaxValue } integer) {
                throw new ParseException("Expect array index", Right.Position);
            }

            var type = expression.Type.GetElementType() ?? expression.Type.GenericTypeArguments.FirstOrDefault();

            var at = ctx.GetMethod(typeof(Enumerable), "ElementAt", new[] { type! },
                () => typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).Single(x =>
                    x.Name == "ElementAt" &&
                    x.GetParameters().Length == 2 &&
                    x.GetParameters().Last().ParameterType == typeof(int)).MakeGenericMethod(type!));

            return Expression.Call(at!, expression, Expression.Constant((int)integer.Value));
        }

        try {
            return Expression.PropertyOrField(expression, property);
        } catch (ArgumentException) {
            throw new ParseException($"Invalid field name '{property}'", Right.Position);
        }
    }
}
