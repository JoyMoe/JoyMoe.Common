using JoyMoe.Common.Api.Filter;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;
using Xunit;

namespace JoyMoe.Common.Api.Tests;

public class ParserTests
{
    [Fact]
    public void ParseExample1() {
        var parser = new Parser();

        var expression = parser.Parse("a b AND c AND d");

        Assert.Equal("AND(AND(BOTH(\"a\", \"b\"), \"c\"), \"d\")", expression?.ToString());
    }

    [Fact]
    public void ParseExample2() {
        var parser = new Parser();

        var expression = parser.Parse("New York Giants OR Yankees");

        Assert.Equal("BOTH(BOTH(\"New\", \"York\"), OR(\"Giants\", \"Yankees\"))", expression?.ToString());
    }

    [Fact]
    public void ParseExample3() {
        var parser = new Parser();

        var expression = parser.Parse("a < 10 OR a >= 100");

        Assert.Equal("OR(LT(a, 10), GTE(a, 100))", expression?.ToString());
    }

    [Fact]
    public void ParseExample4() {
        var parser = new Parser();

        var expression = parser.Parse("expr.type_map.1.type");

        Assert.Equal(".(.(.(expr, \"type_map\"), 1), \"type\")", expression?.ToString());
    }

    [Fact]
    public void ParseExample5() {
        var parser = new Parser();

        var expression = parser.Parse("(msg.endsWith('world') AND retries < 10)");

        Assert.Equal("AND(.(msg, endsWith)(\"world\"), LT(retries, 10))", expression?.ToString());
    }

    [Fact]
    public void ParseValues() {
        var parser = new Parser();

        var timestamp = DateTimeOffset.Parse("2012-04-21T15:30:00Z");

        var position = new TextPosition(0, 0, 0);
        Assert.Equal(Term.Text(position, "String"), parser.Parse("String"));
        Assert.Equal(Term.Truth(position, true), parser.Parse("true"));
        Assert.Equal(Term.Integer(position, 30), parser.Parse("30"));
        Assert.Equal(Term.Number(position, 2997000000), parser.Parse("2.997e9"));
        Assert.Equal(Term.Duration(position, TimeSpan.FromSeconds(20)), parser.Parse("20s"));
        Assert.Equal(Term.Duration(position, TimeSpan.FromSeconds(1.2)), parser.Parse("1.2s"));
        Assert.Equal(Term.Timestamp(position, timestamp), parser.Parse("2012-04-21T15:30:00Z"));
        Assert.Equal(Term.Timestamp(position, timestamp), parser.Parse("2012-04-21T11:30:00-04:00"));
    }

    [Theory]
    [InlineData("New York (Giants OR Yankees)", "BOTH(BOTH(\"New\", \"York\"), OR(\"Giants\", \"Yankees\"))")]
    [InlineData("(a b) AND c AND d", "AND(AND(BOTH(\"a\", \"b\"), \"c\"), \"d\")")]
    [InlineData("a OR b OR c", "OR(OR(\"a\", \"b\"), \"c\")")]
    [InlineData("NOT (a OR b)", "NOT(OR(\"a\", \"b\"))")]
    [InlineData("-file:\".java\"", "NOT(HAS(file, \".java\"))")]
    [InlineData("package=com.google", "EQ(package, .(com, \"google\"))")]
    [InlineData("msg != 'hello'", "NE(msg, \"hello\")")]
    [InlineData("1 > 0", "GT(1, 0)")]
    [InlineData("2.5 >= 2.4", "GTE(2.5, 2.4)")]
    [InlineData("foo >= -2.4", "GTE(foo, -2.4)")]
    [InlineData("foo >= (-2.4)", "GTE(foo, -2.4)")]
    [InlineData("yesterday < request.time", "LT(yesterday, .(request, \"time\"))")]
    [InlineData("experiment.rollout <= cohort(request.user)",
        "LTE(.(experiment, \"rollout\"), cohort(.(request, \"user\")))")]
    [InlineData("prod", "\"prod\"")]
    [InlineData("regex(m.key, '^.*prod.*$')", "regex(.(m, \"key\"), \"^.*prod.*$\")")]
    [InlineData("math.mem('30mb')", ".(math, mem)(\"30mb\")")]
    [InlineData("(msg.endsWith('world') AND retries < 10)", "AND(.(msg, endsWith)(\"world\"), LT(retries, 10))")]
    [InlineData("(endsWith(msg, 'world') AND retries < 10)", "AND(endsWith(\"msg\", \"world\"), LT(retries, 10))")]
    [InlineData("time.now()", ".(time, now)()")]
    [InlineData("timestamp(\"2012-04-21T11:30:00-04:00\")", "timestamp(\"2012-04-21T11:30:00-04:00\")")]
    [InlineData("duration(\"32s\")", "duration(\"32s\")")]
    [InlineData("duration(\"4h0m0s\")", "duration(\"4h0m0s\")")]
    [InlineData(@"start_time > timestamp(""2006-01-02T15:04:05+07:00"") AND
        (driver = ""driver1"" OR start_driver = ""driver1"" OR end_driver = ""driver1"")",
        @"AND(GT(start_time, timestamp(""2006-01-02T15:04:05+07:00"")), OR(OR(EQ(driver, ""driver1""), EQ(start_driver, ""driver1"")), EQ(end_driver, ""driver1"")))")]
    [InlineData("annotations:schedule", "HAS(annotations, \"schedule\")")]
    [InlineData("annotations.schedule = \"test\"", "EQ(.(annotations, \"schedule\"), \"test\")")]
    public void ParseMoreVectors(string input, string expected) {
        var parser = new Parser();

        var expression = parser.Parse(input);

        Assert.Equal(expected, expression?.ToString());
    }
}
