using System.Text.Json;

namespace BurglarBuster.Infrastructure.Tools;

internal static class ToolJson
{
    public static readonly JsonSerializerOptions Options = new(JsonSerializerOptions.Web);
}
