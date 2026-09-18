using Azure.Monitor.OpenTelemetry.Exporter;
using BurglarBuster.Core.Agent;
using BurglarBuster.Core.Matching;
using BurglarBuster.Core.Tools;
using BurglarBuster.Functions.Configuration;
using BurglarBuster.Infrastructure;
using BurglarBuster.Infrastructure.Agent;
using BurglarBuster.Infrastructure.Matching;
using BurglarBuster.Infrastructure.Repositories;
using BurglarBuster.Infrastructure.Seeding;
using BurglarBuster.Infrastructure.Tools;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

builder.Services
    .AddOptions<AgentLoopOptions>()
    .Bind(builder.Configuration.GetSection(AgentLoopOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection(DatabaseOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services
    .AddOptions<AzureOpenAIOptions>()
    .Bind(builder.Configuration.GetSection(AzureOpenAIOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddDbContext<BurglarBusterDbContext>((serviceProvider, options) =>
{
    var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    options.UseSqlite(databaseOptions.ConnectionString);
});

builder.Services.AddScoped<IPersonReadRepository, PersonReadRepository>();
builder.Services.AddScoped<IPersonCandidateRepository, PersonCandidateRepository>();
builder.Services.AddScoped<MatchingService>();
builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<ITool, SearchPeopleTool>();
builder.Services.AddScoped<ITool, GetPersonRecordTool>();
builder.Services.AddScoped<ITool, GetAliasesTool>();
builder.Services.AddScoped<ITool, GetAddressHistoryTool>();
builder.Services.AddScoped<ITool, GetPhysicalDescriptionTool>();
builder.Services.AddScoped<ITool, GetCaseReferencesTool>();
builder.Services.AddScoped<ITool, CompareCandidatesTool>();
builder.Services.AddScoped<IToolDispatcher, ToolDispatcher>();

builder.Services.AddScoped<IInvestigationRepository, InvestigationRepository>();

builder.Services.AddScoped<IChatCompletionClient>(sp =>
{
    var azureOpenAIOptions = sp.GetRequiredService<IOptions<AzureOpenAIOptions>>().Value;
    return new AzureOpenAIChatCompletionClient(azureOpenAIOptions.BaseUrl, azureOpenAIOptions.ApiKey, azureOpenAIOptions.ChatDeployment);
});

builder.Services.AddScoped(sp =>
{
    var azureOpenAIOptions = sp.GetRequiredService<IOptions<AzureOpenAIOptions>>().Value;
    return new InvestigationAgentLoop(
        sp.GetRequiredService<IChatCompletionClient>(),
        sp.GetRequiredService<IToolDispatcher>(),
        sp.GetRequiredService<IInvestigationRepository>(),
        sp.GetRequiredService<TimeProvider>(),
        sp.GetRequiredService<IOptions<AgentLoopOptions>>(),
        azureOpenAIOptions.ChatDeployment,
        sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<InvestigationAgentLoop>>());
});

if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("APPLICATIONINSIGHTS_CONNECTION_STRING")))
{
    builder.Services.AddOpenTelemetry()
        .UseFunctionsWorkerDefaults()
        .UseAzureMonitorExporter();
}

var host = builder.Build();

using (var startupScope = host.Services.CreateScope())
{
    var dbContext = startupScope.ServiceProvider.GetRequiredService<BurglarBusterDbContext>();
    await dbContext.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(dbContext);
}

host.Run();
