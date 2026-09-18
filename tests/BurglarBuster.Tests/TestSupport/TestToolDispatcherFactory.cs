using BurglarBuster.Core.Tools;
using BurglarBuster.Infrastructure.Matching;
using BurglarBuster.Infrastructure.Repositories;
using BurglarBuster.Infrastructure.Tools;

namespace BurglarBuster.Tests.TestSupport;

/// <summary>Builds a real, fully-wired ToolDispatcher (all 7 tools) against a test
/// database — used by agent-loop tests, which exercise real tool execution end to end
/// while only the model itself is faked (spec §18).</summary>
internal static class TestToolDispatcherFactory
{
    public static IToolDispatcher Create(SqliteTestDatabase database)
    {
        var personReadRepository = new PersonReadRepository(database.Context);
        var matchingService = new MatchingService(new PersonCandidateRepository(database.Context));
        var timeProvider = TimeProvider.System;

        ITool[] tools =
        [
            new SearchPeopleTool(matchingService, timeProvider),
            new GetPersonRecordTool(personReadRepository),
            new GetAliasesTool(personReadRepository),
            new GetAddressHistoryTool(personReadRepository),
            new GetPhysicalDescriptionTool(personReadRepository),
            new GetCaseReferencesTool(personReadRepository),
            new CompareCandidatesTool(personReadRepository, timeProvider),
        ];

        return new ToolDispatcher(tools);
    }
}
