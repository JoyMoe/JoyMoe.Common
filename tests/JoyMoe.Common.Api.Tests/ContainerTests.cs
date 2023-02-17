using JoyMoe.Common.Api.Filter;
using JoyMoe.Common.Api.Filter.Operands;
using JoyMoe.Common.Api.Filter.Terms;
using Parlot;
using Xunit;

namespace JoyMoe.Common.Api.Tests;

public class ContainerTests
{
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
        var expected   = new Func<long, bool>(a => a < 10 || a >= 100);
        var actual     = (Func<long, bool>)expression.Compile();
        Assert.Equal(expected(10), actual(10));
    }

    [Fact]
    public void TranslateExample4() {
        var position = new TextPosition(0, 0, 0);

        var term = new Accessor(position,
            new Accessor(position,
                new Accessor(position, new Identifier(position, "expr"), new Text(position, "type_map")),
                new Integer(position, 1L)), new Text(position, "type"));

        var expression = Container.Build(term).Bind("expr", typeof(MyVector4)).Build();
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
    }

    public class MyVector4
    {
        public class MyType
        {
            public string type { get; set; } = string.Empty;
        }

        public MyType[] type_map { get; set; } = Array.Empty<MyType>();
    }
}
