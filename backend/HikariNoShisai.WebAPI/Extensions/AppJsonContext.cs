using HikariNoShisai.Common.DTO;
using System.Text.Json.Serialization;

namespace HikariNoShisai.WebAPI.Extensions
{
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default)]
    [JsonSerializable(typeof(AgentTerminalRequest))]
    [JsonSerializable(typeof(AgentTerminalStatusPatch))]
    [JsonSerializable(typeof(AgentStatusLogRequest))]
    public partial class AppJsonContext : JsonSerializerContext { }
}