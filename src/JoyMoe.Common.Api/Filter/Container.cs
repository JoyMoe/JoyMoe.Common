using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;

namespace JoyMoe.Common.Api.Filter;

public class Container
{
    private static Dictionary<string, MethodInfo> MethodCache = new();

    private Term Term { get; }

    private Dictionary<string, Expression> Expressions { get; } = new();

    private Dictionary<string, ParameterExpression> Parameters { get; } = new();

    public Container(Term term) {
        Term = term;
    }

    public static Container Build(Term term) {
        return new Container(term);
    }

    public Container Bind(string name, Type type) {
        var parameter = Expression.Parameter(type, name);

        Parameters.Add(name, parameter);

        return this;
    }

    public Container Bind(string name, Expression expression) {
        Expressions.Add(name, expression);

        return this;
    }

    public bool TryGetExpression(string? name, [MaybeNullWhen(false)] out Expression expression) {
        if (!string.IsNullOrWhiteSpace(name)) {
            return Expressions.TryGetValue(name, out expression);
        }

        expression = default;
        return false;
    }

    public bool TryGetParameter(string? name, [MaybeNullWhen(false)] out ParameterExpression parameter) {
        if (!string.IsNullOrWhiteSpace(name)) {
            return Parameters.TryGetValue(name, out parameter);
        }

        parameter = default;
        return false;
    }

    public MethodInfo? GetMethod(
        Type          type,
        string        name,
        Type[]?       types,
        BindingFlags? flag = null) {
        return GetMethod(type, name, types, () => {
            flag ??= BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;

            return types == null
                ? type.GetMethod(name, flag.Value)
                : type.GetMethod(name, flag.Value, null, types, null);
        });
    }

    public MethodInfo? GetMethod(
        Type               type,
        string             name,
        IEnumerable<Type>? types,
        Func<MethodInfo?>  getter) {
        var typ       = types?.Aggregate("", (s, i) => s = $"{s}{i.Name},");
        var qualified = $"{type.FullName}.{name}({typ})";

        if (MethodCache.TryGetValue(qualified, out var method)) {
            return method;
        }

        method = getter();

        if (method == null) return null;

        MethodCache.Add(qualified, method);

        return method;
    }

    public LambdaExpression Build() {
        var body = Term.ToExpression(this);

        return Expression.Lambda(body, Parameters.Values);
    }

    internal class Replacer : ExpressionVisitor
    {
        public Dictionary<string, ParameterExpression> Parameters { get; }

        public static Expression? Replace(Expression? expression, Dictionary<string, ParameterExpression> parameters) {
            if (expression == null) return null;

            var visitor = new Replacer(parameters);
            return visitor.Visit(expression);
        }

        private Replacer(Dictionary<string, ParameterExpression> parameters) {
            Parameters = parameters;
        }

        public override Expression? Visit(Expression? node) {
            if (node is not ParameterExpression parameter) {
                return base.Visit(node);
            }

            if (string.IsNullOrWhiteSpace(parameter.Name)) {
                return base.Visit(node);
            }

            if (!Parameters.TryGetValue(parameter.Name, out var value)) {
                throw new ParseException($"Unknown parameter: {parameter.Name}", new TextPosition());
            }

            return value;
        }
    }
}
