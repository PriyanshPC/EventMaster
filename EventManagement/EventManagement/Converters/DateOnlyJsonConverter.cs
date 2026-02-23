using System.Text.Json;
using System.Text.Json.Serialization;

/// <summary> /// This converter helps ASP.NET handle DateOnly values when sending or receiving JSON. /// By default, JSON doesn't know how to read or write DateOnly, so we manually tell it /// how to convert the value. /// </summary>
public class DateOnlyJsonConverter : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => DateOnly.Parse(reader.GetString()!);

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
        => writer.WriteStringValue(value.ToString("yyyy-MM-dd"));
}
