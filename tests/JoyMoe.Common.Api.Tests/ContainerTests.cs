using JoyMoe.Common.Api.Filter;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;
using Xunit;

namespace JoyMoe.Common.Api.Tests;

public class ContainerTests
{
    [Fact]
    public void TranslateExample1() {
        var position = new TextPosition(0, 0, 0);

        // a b AND c AND d
        var term = (Term.Text(position, "a") + Term.Text(position, "b")) &
                   Term.Text(position, "c") &
                   Term.Text(position, "d");

        var expression = Container.Build(term).Bind("q", typeof(string)).Build();
        var func       = (Func<string, bool>)expression.Compile();

        Assert.True(func("a b c d"));
        Assert.False(func("a b c"));
    }

    [Fact]
    public void TranslateExample2() {
        var position = new TextPosition(0, 0, 0);

        // New York Giants OR Yankees
        var term = (Term.Text(position, "New") + Term.Text(position, "York")) +
                   (Term.Text(position, "Giants") | Term.Text(position, "Yankees"));

        var expression = Container.Build(term).Bind("q", typeof(string)).Build();
        var func       = (Func<string, bool>)expression.Compile();

        Assert.True(func("New York Giants"));
        Assert.False(func("New Giants Yankees"));
    }

    [Fact]
    public void TranslateExample3() {
        var position = new TextPosition(0, 0, 0);

        // a < 10 OR a >= 100
        var term = Term.LessThan(Term.Identifier(position, "a"), Term.Integer(position, 10)) |
                   Term.GreaterThanOrEqual(Term.Identifier(position, "a"), Term.Integer(position, 100));

        var expression = Container.Build(term).Bind("a", typeof(long)).Build();
        var expected   = new Func<long, bool>(a => a is < 10 or >= 100);
        var actual     = (Func<long, bool>)expression.Compile();

        Assert.Equal(expected(10), actual(10));
        Assert.Equal(expected(100), actual(100));
    }

    [Fact]
    public void TranslateExample4() {
        var position = new TextPosition(0, 0, 0);

        // expr.type_map.1.type
        var term = Term.Accessor(
            Term.Accessor(Term.Accessor(Term.Identifier(position, "expr"), Term.Text(position, "type_map")),
                Term.Integer(position, 1L)), Term.Text(position, "type"));

        var expression = Container.Build(term).Bind("expr", typeof(MyVector4)).Build();
        var func       = (Func<MyVector4, string>)expression.Compile();

        var vector = new MyVector4 {
            type_map = { new MyVector4.MyType { type = "foo" }, new MyVector4.MyType { type = "bar" } },
        };
        Assert.Equal("bar", func(vector));
    }

    [Fact]
    public void TranslateExample5() {
        var position = new TextPosition(0, 0, 0);

        // (msg.endsWith('world') AND retries < 10)
        var term = Term.Function(Term.Accessor(Term.Identifier(position, "msg"), Term.Identifier(position, "endsWith")),
                       new List<Term> { Term.Text(position, "world") }) &
                   Term.LessThan(Term.Identifier(position, "retries"), Term.Integer(position, 10L));

        var expression = Container.Build(term).Bind("msg", typeof(string)).Bind("retries", typeof(int)).Build();
        var func       = (Func<string, int, bool>)expression.Compile();

        Assert.True(func("hello world", 9));
        Assert.False(func("hello world", 10));
    }

    [Fact]
    public void TranslateArrayHas() {
        var position = new TextPosition(0, 0, 0);

        var term = Term.Has(Term.Identifier(position, "r"), Term.Integer(position, 42));

        var expression = Container.Build(term).Bind("r", typeof(int[])).Build();
        var func       = (Func<int[], bool>)expression.Compile();

        Assert.True(func(new[] { 42 }));
        Assert.False(func(new[] { 24 }));
    }

    [Fact]
    public void TranslateDictionaryHas() {
        var position = new TextPosition(0, 0, 0);

        var term = Term.Has(Term.Identifier(position, "m"), Term.Integer(position, 42));

        var expression = Container.Build(term).Bind("m", typeof(Dictionary<string, bool>)).Build();
        var func       = (Func<Dictionary<string, bool>, bool>)expression.Compile();

        Assert.True(func(new Dictionary<string, bool> { ["42"]  = true }));
        Assert.False(func(new Dictionary<string, bool> { ["24"] = true }));
    }

    [Fact]
    public void TranslateDictionaryHasWithAccessor() {
        var position = new TextPosition(0, 0, 0);

        var term = Term.Has(                                                           //
            Term.Accessor(Term.Identifier(position, "m"), Term.Text(position, "foo")), //
            Term.Integer(position, 42)                                                 //
        );

        var expression = Container.Build(term).Bind("m", typeof(Dictionary<string, int>)).Build();
        var func       = (Func<Dictionary<string, int>, bool>)expression.Compile();

        Assert.True(func(new Dictionary<string, int> { ["foo"]  = 42 }));
        Assert.False(func(new Dictionary<string, int> { ["foo"] = 24 }));
    }

    [Fact]
    public void TranslateDictionaryHasWithAccessorMatch() {
        var position = new TextPosition(0, 0, 0);

        var term = Term.Has(                                                           //
            Term.Accessor(Term.Identifier(position, "m"), Term.Text(position, "foo")), //
            Term.Text(position, "*")                                                   //
        );

        var expression = Container.Build(term).Bind("m", typeof(Dictionary<string, int>)).Build();
        var func       = (Func<Dictionary<string, int>, bool>)expression.Compile();

        Assert.True(func(new Dictionary<string, int> { ["foo"]  = 42 }));
        Assert.False(func(new Dictionary<string, int> { ["bar"] = 42 }));
    }

    [Fact]
    public void TranslateStringHas() {
        var position = new TextPosition(0, 0, 0);

        var term = Term.Has(Term.Identifier(position, "msg"), Term.Text(position, "hello"));

        var expression = Container.Build(term).Bind("msg", typeof(string)).Build();
        var func       = (Func<string, bool>)expression.Compile();

        Assert.True(func("hello world"));
        Assert.False(func("foo"));
    }

    [Fact]
    public void TranslateMatch() {
        var position = new TextPosition(0, 0, 0);

        var term = Term.Match(Term.Identifier(position, "msg"), Term.Text(position, "oo"));

        var expression = Container.Build(term).Bind("msg", typeof(string)).Build();
        var func       = (Func<string, bool>)expression.Compile();

        Assert.True(func("foobar"));
        Assert.False(func("bar"));
    }
    
    [Fact]
    public void TranslatePrefixMatch() {
        var position = new TextPosition(0, 0, 0);

        var term = Term.Equal(Term.Identifier(position, "msg"), Term.Text(position, "foo*"));

        var expression = Container.Build(term).Bind("msg", typeof(string)).Build();
        var func       = (Func<string, bool>)expression.Compile();

        Assert.True(func("foobar"));
        Assert.False(func("bar"));
    }

    [Fact]
    public void TranslateSuffixMatch() {
        var position = new TextPosition(0, 0, 0);

        var term = Term.Equal(Term.Identifier(position, "msg"), Term.Text(position, "*bar"));

        var expression = Container.Build(term).Bind("msg", typeof(string)).Build();
        var func       = (Func<string, bool>)expression.Compile();

        Assert.True(func("foobar"));
        Assert.False(func("foo"));
    }

    public class MyVector4
    {
        public class MyType
        {
            public string type { get; set; } = string.Empty;
        }

        public List<MyType> type_map { get; } = new();
    }
}
