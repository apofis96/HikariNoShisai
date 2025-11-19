using HikariNoShisai.Common.DTO;
using System.Text.Json.Serialization;

namespace HikariNoShisai.WebAPI
{
    [JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Default)]
    [JsonSerializable(typeof(sbyte))]
    [JsonSerializable(typeof(AgentTerminalRequest))]
    public partial class AppJsonContext : JsonSerializerContext
    {
    }
}
