using System.Text.Json;
using System.Text.Json.Serialization;

namespace MinGo.Qap.Shared.Models;

/// <summary>
/// 强类型 JobKey 标识符，包含 Name（必填）和 Group（默认 "DEFAULT"）。
/// 值类型、不可变、按值比较相等。
/// 在 JSON 中序列化为 { "name": "...", "group": "..." }。
/// </summary>
[JsonConverter(typeof(JobKeyDtoJsonConverter))]
public readonly record struct JobKeyDto(string Name, string Group = "DEFAULT")
{
    public override string ToString() => $"{Group}.{Name}";
}

/// <summary>
/// 自定义 JSON 转换器：确保 Group 为 null/empty 时使用 "DEFAULT"，
/// Name 为 null/empty 时抛出异常。
/// </summary>
public class JobKeyDtoJsonConverter : JsonConverter<JobKeyDto>
{
    public override JobKeyDto Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected JSON object for JobKeyDto");

        string? name = null;
        string? group = null;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                break;

            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();
                reader.Read();

                switch (propertyName?.ToLowerInvariant())
                {
                    case "name":
                        name = reader.GetString();
                        break;
                    case "group":
                        group = reader.GetString();
                        break;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(name))
            throw new JsonException("JobKeyDto.Name is required");

        return new JobKeyDto(name, string.IsNullOrWhiteSpace(group) ? "DEFAULT" : group);
    }

    public override void Write(Utf8JsonWriter writer, JobKeyDto value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("name", value.Name);
        writer.WriteString("group", value.Group);
        writer.WriteEndObject();
    }
}
