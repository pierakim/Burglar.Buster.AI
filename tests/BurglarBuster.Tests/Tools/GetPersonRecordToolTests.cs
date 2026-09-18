using System.Text.Json;
using BurglarBuster.Core.Tools;
using BurglarBuster.Infrastructure.Repositories;
using BurglarBuster.Infrastructure.Seeding;
using BurglarBuster.Infrastructure.Tools;
using BurglarBuster.Tests.TestSupport;

namespace BurglarBuster.Tests.Tools;

public class GetPersonRecordToolTests : IDisposable
{
    private readonly SqliteTestDatabase _database = new();
    private readonly GetPersonRecordTool _tool;

    public GetPersonRecordToolTests()
    {
        _tool = new GetPersonRecordTool(new PersonReadRepository(_database.Context));
    }

    [Fact]
    public async Task Known_person_returns_core_record()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var result = await _tool.ExecuteAsync("""{"personId":"BB-0001"}""", CancellationToken.None);

        Assert.Equal(ToolDispatchStatus.Success, result.Status);
        var record = JsonSerializer.Deserialize<PersonRecordResult>(result.ResultJson!, new JsonSerializerOptions(JsonSerializerOptions.Web))!;
        Assert.Equal("Daniel", record.GivenName);
        Assert.Equal("Miller", record.FamilyName);
        Assert.Equal(new DateOnly(1989, 4, 12), record.DateOfBirth);
    }

    [Fact]
    public async Task Unknown_but_well_formed_person_id_is_not_found()
    {
        await DatabaseSeeder.SeedAsync(_database.Context);

        var result = await _tool.ExecuteAsync("""{"personId":"BB-9999"}""", CancellationToken.None);

        Assert.Equal(ToolDispatchStatus.NotFound, result.Status);
    }

    [Fact]
    public async Task Malformed_person_id_is_an_invalid_argument_without_touching_the_database()
    {
        var result = await _tool.ExecuteAsync("""{"personId":"not-an-id"}""", CancellationToken.None);

        Assert.Equal(ToolDispatchStatus.InvalidArguments, result.Status);
    }

    [Fact]
    public async Task Malformed_json_is_an_invalid_argument()
    {
        var result = await _tool.ExecuteAsync("not json", CancellationToken.None);

        Assert.Equal(ToolDispatchStatus.InvalidArguments, result.Status);
    }

    public void Dispose() => _database.Dispose();
}
