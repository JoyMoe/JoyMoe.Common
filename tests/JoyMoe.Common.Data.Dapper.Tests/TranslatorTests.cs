using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Dapper;
using Xunit;

namespace JoyMoe.Common.Data.Dapper.Tests;

public class TranslatorTests
{
    [Fact]
    public void TranslateNumberEquals() {
        var (clause, parameters) = Translate(s => s.Id == 1);
        Assert.Equal("(\"Id\" = 1)", clause);
        Assert.Null(parameters);

        var student = new Student { Id = 1 };
        (clause, parameters) = Translate(s => s.Id == student.Id);
        Assert.Equal("(\"Id\" = 1)", clause);
        Assert.Null(parameters);
    }

    [Fact]
    public void TranslateStringEquals() {
        var (clause, parameters) = Translate(s => s.LastName == "Sophia");
        Assert.Equal("(\"LastName\" = @__p0)", clause);
        Assert.NotNull(parameters);
        Assert.Equal("Sophia", parameters!.Get<string>("@__p0"));

        (clause, parameters) = Translate(s => s.LastName.Equals("Sophia"));
        Assert.Equal("(\"LastName\" = @__p0)", clause);
        Assert.NotNull(parameters);
        Assert.Equal("Sophia", parameters!.Get<string>("@__p0"));

        (clause, parameters) = Translate(s => s.LastName.Equals("Sophia", StringComparison.InvariantCultureIgnoreCase));
        Assert.Equal("(UPPER(\"LastName\") = UPPER(@__p0))", clause);
        Assert.NotNull(parameters);
        Assert.Equal("Sophia", parameters!.Get<string>("@__p0"));

        (clause, parameters) = Translate(s => string.Equals(s.LastName, "Sophia"));
        Assert.Equal("(\"LastName\" = @__p0)", clause);
        Assert.NotNull(parameters);
        Assert.Equal("Sophia", parameters!.Get<string>("@__p0"));

        (clause, parameters) =
            Translate(s => string.Equals(s.LastName, "Sophia", StringComparison.InvariantCultureIgnoreCase));
        Assert.Equal("(UPPER(\"LastName\") = UPPER(@__p0))", clause);
        Assert.NotNull(parameters);
        Assert.Equal("Sophia", parameters!.Get<string>("@__p0"));
    }

    [Fact]
    public void TranslateContains() {
        var values = new List<long> { 1L, 2L, 3L };

        var (clause, parameters) = Translate(s => values.Contains(s.Id));
        Assert.Equal("(\"Id\" = ANY(@__p0))", clause);
        Assert.NotNull(parameters);
        Assert.Single(parameters!.ParameterNames);

        (clause, parameters) = Translate(s => !values.Contains(s.Id));
        Assert.Equal(" NOT (\"Id\" = ANY(@__p0))", clause);
        Assert.NotNull(parameters);
        Assert.Single(parameters!.ParameterNames);
    }

    [Fact]
    public void TranslateStringLikes() {
        // Caroline Sophia

        var (clause, parameters) = Translate(s => s.LastName.StartsWith("Sop"));
        Assert.Equal("(\"LastName\" LIKE @__p0)", clause);
        Assert.NotNull(parameters);
        Assert.Equal("Sop%", parameters!.Get<string>("@__p0"));

        (clause, parameters) = Translate(s => s.LastName.EndsWith("hia"));
        Assert.Equal("(\"LastName\" LIKE @__p0)", clause);
        Assert.NotNull(parameters);
        Assert.Equal("%hia", parameters!.Get<string>("@__p0"));

        (clause, parameters) = Translate(s => s.LastName.Contains("ph"));
        Assert.Equal("(\"LastName\" LIKE @__p0)", clause);
        Assert.NotNull(parameters);
        Assert.Equal("%ph%", parameters!.Get<string>("@__p0"));
    }

    [Fact]
    public void TranslateLogicality() {
        var values = new List<long> { 1L, 2L, 3L };

        var (clause, parameters) = Translate(s => !values.Contains(s.Id) && s.LastName == "Sophia");
        Assert.Equal("( NOT (\"Id\" = ANY(@__p0)) AND (\"LastName\" = @__p1))", clause);
        Assert.NotNull(parameters);
        Assert.Equal("Sophia", parameters!.Get<string>("@__p1"));

        (clause, parameters) =
            Translate(s => !values.Contains(s.Id) && (s.FirstName.StartsWith("Caro") || s.LastName == "Sophia"));
        Assert.Equal("( NOT (\"Id\" = ANY(@__p0)) AND ((\"FirstName\" LIKE @__p1) OR (\"LastName\" = @__p2)))", clause);
        Assert.NotNull(parameters);
        Assert.Equal("Caro%", parameters!.Get<string>("@__p1"));
        Assert.Equal("Sophia", parameters.Get<string>("@__p2"));
    }

    private static (string?, DynamicParameters?) Translate(Expression<Func<Student, bool>> expression) {
        var translator = new ExpressionTranslator(new PostgresAdapter());
        return translator.Translate(expression);
    }
}
