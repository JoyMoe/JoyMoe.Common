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

    public override Expression ToExpression(Container container) {
        var expression = Left!.ToExpression(container);
        var element    = expression.Type.GetElementType();

        if (typeof(IEnumerable).IsAssignableFrom(expression.Type)) {
            var contains = typeof(IEnumerable<>).MakeGenericType(element!)
                                                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                .Single(x => x.Name == "Contains" && x.GetParameters().Length == 2)
                                                .MakeGenericMethod(element!);

            return Expression.Call(contains, expression, Right.ToExpression(container));
        }

        if (typeof(IDictionary).IsAssignableFrom(expression.Type)) {
            var contains = typeof(IEnumerable<>).MakeGenericType(element!)
                                                .GetMethods(BindingFlags.Static | BindingFlags.Public)
                                                .Single(x => x.Name == "ContainsKey" && x.GetParameters().Length == 2)
                                                .MakeGenericMethod(element!);

            return Expression.Call(contains, expression, Right.ToExpression(container));
        }

        if (typeof(string).IsAssignableFrom(expression.Type)) {
            var contains = typeof(string).GetMethod("Contains", new[] { typeof(string) });

            return Expression.Call(expression, contains!, Right.ToExpression(container));
        }

        throw new ParseException("Unsupported operand ':'", Position);
    }
}
