using JoyMoe.Common.Api.Filter;
using JoyMoe.Common.Api.Filter.Operations;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;
using Xunit;

namespace JoyMoe.Common.Api.Tests;

public class ContainerTests
{
    [Fact]
    public void TranslateExample1() {
        var position = new TextPosition(0, 0, 0);

        var term = new And(position,
            new And(position, new Both(position, new Text(position, "a"), new Text(position, "b")),
                new Text(position, "c")), new Text(position, "d"));

        var expression = Container.Build(term).Bind("q", typeof(string)).Build();
        var func       = (Func<string, bool>)expression.Compile();

        Assert.True(func("a b c d"));
        Assert.False(func("a b c"));
    }

    [Fact]
    public void TranslateExample2() {
        var position = new TextPosition(0, 0, 0);

        var term = new Both(position, new Both(position, new Text(position, "New"), new Text(position, "York")),
            new Or(position, new Text(position, "Giants"), new Text(position, "Yankees")));

        var expression = Container.Build(term).Bind("q", typeof(string)).Build();
        var func       = (Func<string, bool>)expression.Compile();

        Assert.True(func("New York Giants"));
        Assert.False(func("New Giants Yankees"));
    }

    [Fact]
    public void TranslateExample3() {
        var position = new TextPosition(0, 0, 0);

        var term = new Or(position, new LessThan(position, //
            new Identifier(position, "a"),                 //
            new Integer(position, 10)                      //
        ), new GreaterThanOrEqual(position,                //
            new Identifier(position, "a"),                 //
            new Integer(position, 100)                     //
        ));

        var expression = Container.Build(term).Bind("a", typeof(long)).Build();
        var expected   = new Func<long, bool>(a => a is < 10 or >= 100);
        var actual     = (Func<long, bool>)expression.Compile();

        Assert.Equal(expected(10), actual(10));
        Assert.Equal(expected(100), actual(100));
    }

    [Fact]
    public void TranslateExample4() {
        var position = new TextPosition(0, 0, 0);

        var term = new Accessor(position,
            new Accessor(position,
                new Accessor(position, new Identifier(position, "expr"), new Text(position, "type_map")),
                new Integer(position, 1L)), new Text(position, "type"));

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

        var term = new And(position,
            new Function(position,
                new Accessor(position, new Identifier(position, "msg"), new Identifier(position, "endsWith")),
                new List<Term> { new Text(position, "world") }),
            new LessThan(position, new Identifier(position, "retries"), new Integer(position, 10L)));

        var expression = Container.Build(term).Bind("msg", typeof(string)).Bind("retries", typeof(int)).Build();
        var func       = (Func<string, int, bool>)expression.Compile();

        Assert.True(func("hello world", 9));
        Assert.False(func("hello world", 10));
    }

    [Fact]
    public void TranslateArrayHas() {
        var position = new TextPosition(0, 0, 0);

        var term = new Has(position, new Identifier(position, "r"), new Integer(position, 42));

        var expression = Container.Build(term).Bind("r", typeof(int[])).Build();
        var func       = (Func<int[], bool>)expression.Compile();

        Assert.True(func(new[] { 42 }));
        Assert.False(func(new[] { 24 }));
    }

    [Fact]
    public void TranslateDictionaryHas() {
        var position = new TextPosition(0, 0, 0);

        var term = new Has(position, new Identifier(position, "m"), new Integer(position, 42));

        var expression = Container.Build(term).Bind("m", typeof(Dictionary<string, bool>)).Build();
        var func       = (Func<Dictionary<string, bool>, bool>)expression.Compile();

        Assert.True(func(new Dictionary<string, bool> { ["42"]  = true }));
        Assert.False(func(new Dictionary<string, bool> { ["24"] = true }));
    }

    [Fact]
    public void TranslateDictionaryHasWithAccessor() {
        var position = new TextPosition(0, 0, 0);

        var term = new Has(position,                                                          //
            new Accessor(position, new Identifier(position, "m"), new Text(position, "foo")), //
            new Integer(position, 42)                                                         //
        );

        var expression = Container.Build(term).Bind("m", typeof(Dictionary<string, int>)).Build();
        var func       = (Func<Dictionary<string, int>, bool>)expression.Compile();

        Assert.True(func(new Dictionary<string, int> { ["foo"]  = 42 }));
        Assert.False(func(new Dictionary<string, int> { ["foo"] = 24 }));
    }

    [Fact]
    public void TranslateDictionaryHasWithAccessorMatch() {
        var position = new TextPosition(0, 0, 0);

        var term = new Has(position,                                                          //
            new Accessor(position, new Identifier(position, "m"), new Text(position, "foo")), //
            new Text(position, "*")                                                           //
        );

        var expression = Container.Build(term).Bind("m", typeof(Dictionary<string, int>)).Build();
        var func       = (Func<Dictionary<string, int>, bool>)expression.Compile();

        Assert.True(func(new Dictionary<string, int> { ["foo"]  = 42 }));
        Assert.False(func(new Dictionary<string, int> { ["bar"] = 42 }));
    }

    [Fact]
    public void TranslateStringHas() {
        var position = new TextPosition(0, 0, 0);

        var term = new Has(position, new Identifier(position, "msg"), new Text(position, "hello"));

        var expression = Container.Build(term).Bind("msg", typeof(string)).Build();
        var func       = (Func<string, bool>)expression.Compile();

        Assert.True(func("hello world"));
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
