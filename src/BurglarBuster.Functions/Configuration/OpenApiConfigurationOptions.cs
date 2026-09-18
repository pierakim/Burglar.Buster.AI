using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace BurglarBuster.Functions.Configuration;

/// <summary>
/// Replaces the OpenAPI extension's generic default document title/description with
/// project-specific values. Discovered and used automatically by the extension via
/// reflection (WEBSITE_RUN_FROM_PACKAGE-style convention) — not referenced directly.
/// </summary>
public sealed class OpenApiConfigurationOptions : DefaultOpenApiConfigurationOptions
{
    public override OpenApiInfo Info { get; set; } = new()
    {
        Title = "Burglar Buster API",
        Description = "Fictional, educational identity-resolution and record-triage API. "
            + "All data is synthetic; results are candidates for human verification only.",
        Version = "1.0.0",
    };

    public override OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;
}
