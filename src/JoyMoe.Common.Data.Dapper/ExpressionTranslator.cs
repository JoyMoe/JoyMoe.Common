using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Dapper;

namespace JoyMoe.Common.Data.Dapper;

public class ExpressionTranslator : ExpressionVisitor
{
    private readonly ISqlAdapter   _adapter;
    private readonly StringBuilder _sb     = new();
    private readonly List<object>  _values = new();

    private string? _stringConstantPrefix;
    private string? _stringConstantSuffix;

    public ExpressionTranslator(ISqlAdapter adapter) {
        _adapter = adapter;
    }

    public (string?, DynamicParameters?) Translate(Expression? expression) {
        if (expression == null) return (null, null);

        Visit(expression);

        var clause = _sb.ToString();

        if (_values.Count == 0) return (clause, null);

        var parameters = new DynamicParameters();
        for (var i = 0; i < _values.Count; i++) parameters.Add($"@__p{i}", _values[i]);

        return (clause, parameters);
    }

    protected override Expression VisitMethodCall(MethodCallExpression m) {
        _sb.Append('(');

        switch (m.Method.Name) {
            case "Contains":
                if (m.Object?.NodeType == ExpressionType.MemberAccess && m.Object?.Type == typeof(string)) {
                    goto case "StringContains";
                }

                if (m.Object != null) {
                    Visit(m.Arguments[0]);

                    // ISSUE: https://github.com/StackExchange/Dapper/issues/150
                    _sb.Append(" = ANY(");
                    Visit(m.Object);
                    _sb.Append(')');

                    break;
                }

                if (m.Object == null) {
                    Visit(m.Arguments[1]);

                    // ISSUE: https://github.com/StackExchange/Dapper/issues/150
                    _sb.Append(" = ANY(");
                    Visit(m.Arguments[0]);
                    _sb.Append(')');

                    break;
                }

                goto default;

            case "StringContains":
                _stringConstantPrefix = "%";
                _stringConstantSuffix = "%";
                goto case "StringLikes";

            case "StartsWith":
                _stringConstantSuffix = "%";
                goto case "StringLikes";

            case "EndsWith":
                _stringConstantPrefix = "%";
                goto case "StringLikes";

            case "StringLikes":
                if (m.Object?.NodeType != ExpressionType.MemberAccess) goto default;

                Visit(m.Object);
                _sb.Append(" LIKE ");
                Visit(m.Arguments[0]);

                break;

            case "CompareString":
            case "Equals":
                if (m.Object?.NodeType == ExpressionType.MemberAccess) {
                    // a.Equals(b)

                    if (m.Arguments.Count == 2 &&
                        m.Arguments[1] is ConstantExpression ce &&
                        ce.Type == typeof(StringComparison) &&
                        ce.Value?.ToString()?.EndsWith("IgnoreCase") == true) {
                        _sb.Append("UPPER(");
                        Visit(m.Object);
                        _sb.Append(") = UPPER(");
                        Visit(m.Arguments[0]);
                        _sb.Append(')');
                    } else if (m.Arguments.Count == 2 || m.Arguments.Count == 1) {
                        Visit(m.Object);
                        _sb.Append(" = ");
                        Visit(m.Arguments[0]);
                    } else {
                        goto default;
                    }

                    break;
                }

                if (m.Object == null) {
                    // string.Equals(a, b)

                    if (m.Arguments.Count == 3 &&
                        m.Arguments[2] is ConstantExpression ce &&
                        ce.Type == typeof(StringComparison) &&
                        ce.Value?.ToString()?.EndsWith("IgnoreCase") == true) {
                        _sb.Append("UPPER(");
                        Visit(m.Arguments[0]);
                        _sb.Append(") = UPPER(");
                        Visit(m.Arguments[1]);
                        _sb.Append(')');
                    } else if (m.Arguments.Count == 3 || m.Arguments.Count == 2) {
                        Visit(m.Arguments[0]);
                        _sb.Append(" = ");
                        Visit(m.Arguments[1]);
                    } else {
                        goto default;
                    }

                    break;
                }

                goto default;

            default:
                _stringConstantPrefix = null;
                _stringConstantSuffix = null;
                throw new NotSupportedException($"'{m.Method.Name}' method is not supported");
        }

        _sb.Append(')');

        return m;
    }

    protected override Expression VisitUnary(UnaryExpression u) {
        switch (u.NodeType) {
            case ExpressionType.Not:
                _sb.Append(" NOT ");
                Visit(u.Operand);
                break;
            case ExpressionType.Convert:
                // TODO: Enum to String
                Visit(u.Operand);
                break;
            default:
                throw new NotSupportedException($"The unary operator '{u.NodeType}' is not supported");
        }

        return u;
    }

    protected override Expression VisitBinary(BinaryExpression b) {
        _sb.Append('(');

        Visit(b.Left);

        switch (b.NodeType) {
            case ExpressionType.And:
            case ExpressionType.AndAlso:
                _sb.Append(" AND ");
                break;

            case ExpressionType.Or:
            case ExpressionType.OrElse:
                _sb.Append(" OR ");
                break;

            case ExpressionType.Equal:
                _sb.Append(IsNullConstant(b.Right) ? " IS " : " = ");

                break;

            case ExpressionType.NotEqual:
                _sb.Append(IsNullConstant(b.Right) ? " IS NOT " : " <> ");

                break;

            case ExpressionType.LessThan:
                _sb.Append(" < ");
                break;

            case ExpressionType.LessThanOrEqual:
                _sb.Append(" <= ");
                break;

            case ExpressionType.GreaterThan:
                _sb.Append(" > ");
                break;

            case ExpressionType.GreaterThanOrEqual:
                _sb.Append(" >= ");
                break;

            default:
                throw new NotSupportedException($"The binary operator '{b.NodeType}' is not supported");
        }

        Visit(b.Right);

        _sb.Append(')');

        return b;
    }

    protected override Expression VisitConstant(ConstantExpression c) {
        if (c.Value == null) {
            _sb.Append("NULL");

            return c;
        }

        switch (Type.GetTypeCode(c.Value.GetType())) {
            case TypeCode.Boolean:
                _sb.Append((bool)c.Value ? "TRUE" : "FALSE");
                break;

            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
            case TypeCode.Single:
            case TypeCode.Double:
            case TypeCode.Decimal:
                _sb.Append(c.Value);
                break;

            default:
                AppendParameter(c.Value);
                break;
        }

        return c;
    }

    protected override Expression VisitMember(MemberExpression m) {
        if (m.Expression == null) {
            throw new NotSupportedException($"The member '{m.Member.Name}' is not supported");
        }

        if (m.NodeType == ExpressionType.MemberAccess && m.Expression.NodeType != ExpressionType.Parameter) {
            var item = Expression.Lambda<Func<object>>(Expression.Convert(m, typeof(object))).Compile().Invoke();

            Visit(Expression.Constant(item));

            return m;
        }

        if (m.Expression.NodeType == ExpressionType.Parameter) {
            _adapter.AppendColumnName(_sb, m.Member.Name);
            return m;
        }

        Visit(m.Expression);

        return m;
    }

    private void AppendParameter(object item) {
        _sb.AppendFormat("@__p{0}", _values.Count);

        if (item is string @string) {
            @string = $"{_stringConstantPrefix}{@string}{_stringConstantSuffix}";
            _values.Add(@string);
        } else {
            _values.Add(item);
        }

        _stringConstantPrefix = null;
        _stringConstantSuffix = null;
    }

    private static bool IsNullConstant(Expression exp) {
        return exp.NodeType == ExpressionType.Constant && ((ConstantExpression)exp).Value == null;
    }
}
