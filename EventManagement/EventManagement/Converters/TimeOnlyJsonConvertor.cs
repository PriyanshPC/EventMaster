using System.Text.Json;
using System.Text.Json.Serialization;
/// <summary> /// This converter allows ASP.NET to properly handle TimeOnly values in JSON. /// Since JSON doesn't support TimeOnly by default, we define how to read and write it. /// </summary>
public class TimeOnlyJsonConverter : JsonConverter<TimeOnly>
{
    public override TimeOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => TimeOnly.Parse(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, TimeOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("HH:mm:ss"));
}
