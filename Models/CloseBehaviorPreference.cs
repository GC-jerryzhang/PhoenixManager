using System.Text.Json;
using System.Text.Json.Serialization;

namespace PhoenixToolkit.Models;

[JsonConverter(typeof(CloseBehaviorPreferenceJsonConverter))]
public enum CloseBehaviorPreference
{
    AskEveryTime,
    ExitApplication,
    MinimizeToTray
}

public sealed class CloseBehaviorPreferenceJsonConverter : JsonConverter<CloseBehaviorPreference>
{
    public override CloseBehaviorPreference Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var value = reader.GetString();
            return Enum.TryParse<CloseBehaviorPreference>(value, ignoreCase: true, out var parsed)
                ? parsed
                : CloseBehaviorPreference.AskEveryTime;
        }

        if (reader.TokenType == JsonTokenType.Number
            && reader.TryGetInt32(out var numericValue)
            && Enum.IsDefined(typeof(CloseBehaviorPreference), numericValue))
        {
            return (CloseBehaviorPreference)numericValue;
        }

        reader.Skip();
        return CloseBehaviorPreference.AskEveryTime;
    }

    public override void Write(
        Utf8JsonWriter writer,
        CloseBehaviorPreference value,
        JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString());
    }
}
