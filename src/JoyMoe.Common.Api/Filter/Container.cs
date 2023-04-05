using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Reflection;
using JoyMoe.Common.Api.Filter.Terms;

namespace JoyMoe.Common.Api.Filter;

public class Container
{
    private static readonly Dictionary<string, MethodInfo> MethodCache = new();

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

        expression = null;
        return false;
    }

    public bool TryGetParameter(string? name, [MaybeNullWhen(false)] out ParameterExpression parameter) {
        if (!string.IsNullOrWhiteSpace(name)) {
            return Parameters.TryGetValue(name, out parameter);
        }

        parameter = null;
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
}
