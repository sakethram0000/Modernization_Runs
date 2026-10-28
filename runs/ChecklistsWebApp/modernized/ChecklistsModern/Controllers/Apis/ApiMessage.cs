using System.Text.Json.Serialization;

namespace ChecklistsModern.Controllers.Apis;

// Web API 2 BadRequest(string) body: {"Message":"..."}.
public record ApiMessage([property: JsonPropertyName("Message")] string Message);
