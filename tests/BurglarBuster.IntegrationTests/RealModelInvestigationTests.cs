using BurglarBuster.Core.Agent;
using BurglarBuster.Core.Matching;
using BurglarBuster.Infrastructure.Agent;
using BurglarBuster.Infrastructure.Repositories;
using BurglarBuster.Infrastructure.Seeding;
using BurglarBuster.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace BurglarBuster.IntegrationTests;

/// <summary>
/// The one explicit, optional real-model check (spec §18, §20 Milestone 5). Skips
/// itself automatically when Azure OpenAI configuration isn't present in the
/// environment, so it's harmless if run accidentally. To actually exercise it (this
/// calls the real model and may incur cost):
///
///   $env:AzureOpenAI__BaseUrl = "https://your-resource.openai.azure.com/openai/v1"
///   $env:AzureOpenAI__ApiKey = "..."
///   $env:AzureOpenAI__ChatDeployment = "gpt-4.1-mini"
///   dotnet test tests/BurglarBuster.IntegrationTests --filter Category=RealModel
///
/// Asserts contract/behavioural properties only — never exact model wording.
/// </summary>
public class RealModelInvestigationTests
{
    [Fact]
    [Trait("Category", "RealModel")]
    public async Task Real_model_investigates_a_known_fixture_and_returns_a_contract_valid_result()
    {
        var baseUrl = Environment.GetEnvironmentVariable("AzureOpenAI__BaseUrl");
        var apiKey = Environment.GetEnvironmentVariable("AzureOpenAI__ApiKey");
        var deployment = Environment.GetEnvironmentVariable("AzureOpenAI__ChatDeployment");

        if (string.IsNullOrWhiteSpace(baseUrl) || string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(deployment))
        {
            return; // Skipped: no Azure OpenAI configuration in the environment.
        }

        using var database = new SqliteTestDatabase();
        await DatabaseSeeder.SeedAsync(database.Context);

        var chatClient = new AzureOpenAIChatCompletionClient(baseUrl, apiKey, deployment);
        var toolDispatcher = TestToolDispatcherFactory.Create(database);
        var investigationRepository = new InvestigationRepository(database.Context);

        var loop = new InvestigationAgentLoop(
            chatClient,
            toolDispatcher,
            investigationRepository,
            TimeProvider.System,
            Options.Create(new AgentLoopOptions()),
            deployment,
            NullLogger<InvestigationAgentLoop>.Instance);

        var result = await loop.RunAsync(
            new InvestigationRequest { Description = "A man who said his name was Daniel Miller, born 12 April 1989." },
            CancellationToken.None);

        Assert.NotNull(result.InvestigationId);
        Assert.NotEmpty(result.Summary);
        Assert.True(Enum.IsDefined(result.Outcome));
        Assert.True(Enum.IsDefined(result.TerminationReason));
        Assert.Equal(OutcomePolicy.RequiresHumanReview(result.Outcome), result.RequiresHumanReview);
        Assert.True(result.ModelTurnCount > 0);

        var persisted = await investigationRepository.GetByIdAsync(result.InvestigationId, CancellationToken.None);
        Assert.NotNull(persisted);
    }
}
