using HikariNoShisai.Common.DTO;
using System.Text.Json.Serialization;

namespace HikariNoShisai.WebAPI
{
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default)]
    [JsonSerializable(typeof(sbyte))]
    [JsonSerializable(typeof(bool))]
    [JsonSerializable(typeof(AgentTerminalRequest))]
    [JsonSerializable(typeof(AgentTerminalStatusPatch))]
    [JsonSerializable(typeof(AgentStatusLogRequest))]
    public partial class AppJsonContext : JsonSerializerContext {}
}
