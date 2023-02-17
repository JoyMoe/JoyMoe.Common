using System.Collections;
using System.Linq.Expressions;
using System.Reflection;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;

namespace JoyMoe.Common.Api.Filter.Operands;

public class Has : Operand
{
    public const string Name = ":";

    public override string DisplayName => "HAS";

    public Has(TextPosition position, Term left, Term right) : base(position, left, right) { }

    public override Expression ToExpression(Container ctx) {
        var left  = Left!.ToExpression(ctx);
        var right = Right.ToExpression(ctx);

        if (typeof(string).IsAssignableFrom(left.Type)) {
            var contains = left.Type switch {
                _ when right.Type == typeof(char) => ctx.GetMethod(typeof(string), "Contains", new[] { typeof(char) }),
                _ => ctx.GetMethod(typeof(string), "Contains", new[] { typeof(string) }),
            };

            if (right.Type != typeof(char)) {
                right = Expression.Call(right, "ToString", null);
            }

            return Expression.Call(left, contains!, right);
        }

        var instance = left;
        var index    = right;
        var type     = instance.Type.GetElementType() ?? instance.Type.GenericTypeArguments.FirstOrDefault();

        if (Left is Accessor accessor) {
            var inner = accessor.Left!.ToExpression(ctx);
            if (typeof(IDictionary).IsAssignableFrom(inner.Type)) {
                instance = inner;
                index    = accessor.Right.ToExpression(ctx);
                type     = instance.Type.GenericTypeArguments.FirstOrDefault();
            }
        }

        if (type == null) {
            throw new ParseException("Cannot infer type for operand ':'", Position);
        }

        if (type == typeof(string)) {
            index = Expression.Call(index, "ToString", null);
        } else {
            index = Expression.Convert(index, type);
        }

        if (typeof(IDictionary).IsAssignableFrom(instance.Type)) {
            var dictionary = typeof(IDictionary<,>).MakeGenericType(instance.Type.GenericTypeArguments);
            var contains = ctx.GetMethod(dictionary, "ContainsKey",
                new[] { instance.Type.GenericTypeArguments.First() }, BindingFlags.Instance | BindingFlags.Public);

            var expression = Expression.Call(instance, contains!, index);

            if (instance.Equals(left) || Right is Text { Value: "*" }) {
                return expression;
            }

            if (left.Type == typeof(string)) {
                right = Expression.Call(right, "ToString", null);
            } else {
                right = Expression.Convert(right, left.Type);
            }

            var equal = Expression.Equal(left, right);

            return Expression.AndAlso(expression, equal);
        }

        if (typeof(IEnumerable).IsAssignableFrom(instance.Type)) {
            var contains = ctx.GetMethod(typeof(Enumerable), "Contains", new[] { type },
                () => typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public)
                                        .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                                        .MakeGenericMethod(type));

            return Expression.Call(contains!, instance, index);
        }

        throw new ParseException("Unsupported operand ':'", Position);
    }
}
