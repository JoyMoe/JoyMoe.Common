using System;
using System.Linq;
using System.Threading.Tasks;
using Apps72.Dev.Data.DbMocker;
using Dapper.Contrib;
using Xunit;

namespace JoyMoe.Common.Data.Dapper.Tests;

public class SqlMapperTests
{
    private static readonly Student Student = new()
    {
        Id               = 1,
        FirstName        = "Carolyne",
        LastName         = "Sophia",
        Timestamp        = Guid.NewGuid(),
        CreationDate     = DateTime.UtcNow,
        ModificationDate = DateTime.UtcNow
    };

    [Fact]
    public async Task QueryRecords() {
        var conn = new MockDbConnection();

        const string sql = "SELECT * FROM [Students] WHERE ([Id] = 1)";

        conn.Mocks.When(cmd => cmd.CommandText == sql)
            .ReturnsTable(CreateMockTable()
                             .AddRow(Student.Id,
                                     Student.FirstName,
                                     Student.LastName,
                                     Student.Grade,
                                     Student.Timestamp,
                                     Student.CreationDate,
                                     Student.ModificationDate,
                                     Student.DeletionDate));

        var result = (await conn.QueryAsync<Student>(s => s.Id == 1)).ToArray();
        Assert.Single(result);
        Assert.Equal(Student.FirstName, result.First().FirstName);
    }

    [Fact]
    public async Task InsertRecord() {
        var conn = new MockDbConnection();

        const string sql =
            "INSERT INTO [Students] ([Grade], [Id], [FirstName], [LastName], [Timestamp], [CreationDate], [ModificationDate], [DeletionDate]) VALUES (@Grade, @Id, @FirstName, @LastName, @Timestamp, @CreationDate, @ModificationDate, @DeletionDate)";

        conn.Mocks.When(cmd => cmd.CommandText == sql && cmd.Parameters.Count() == 8).ReturnsRow(1);

        var result = await conn.InsertAsync(Student);
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task UpdateRecord() {
        var conn = new MockDbConnection();

        const string sql =
            "UPDATE [Students] SET [Grade] = @Grade, [FirstName] = @FirstName, [LastName] = @LastName, [Timestamp] = @Timestamp, [CreationDate] = @CreationDate, [ModificationDate] = @ModificationDate, [DeletionDate] = @DeletionDate WHERE [Id] = @Id";

        conn.Mocks.When(cmd => cmd.CommandText == sql && cmd.Parameters.Count() == 8).ReturnsRow(1);

        var result = await conn.UpdateAsync(Student);
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task DeleteRecord() {
        var conn = new MockDbConnection();

        const string sql = "DELETE FROM [Students] WHERE [Id] = @Id";

        conn.Mocks.When(cmd => cmd.CommandText == sql && cmd.Parameters.Count() == 1).ReturnsRow(1);

        var result = await conn.DeleteAsync(Student);
        Assert.Equal(1, result);
    }

    private MockTable CreateMockTable() {
        return MockTable.WithColumns(nameof(Tests.Student.Id),
                                     nameof(Tests.Student.FirstName),
                                     nameof(Tests.Student.LastName),
                                     nameof(Tests.Student.Grade),
                                     nameof(Tests.Student.Timestamp),
                                     nameof(Tests.Student.CreationDate),
                                     nameof(Tests.Student.ModificationDate),
                                     nameof(Tests.Student.DeletionDate));
    }
}
