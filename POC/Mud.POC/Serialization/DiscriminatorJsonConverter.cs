using System.Reflection;
using System.Text;
using System.Text.Json;

namespace Mud.POC.Serialization;

// https://stackoverflow.com/questions/78405973/serializing-and-deserializing-json-to-typed-objects-at-runtime-without-storing-t
public class DiscriminatorJsonConverter
{
    private const string DiscriminatorTag = "$discriminator";

    private readonly Dictionary<string, Type> _discriminatorTypeMap;
    private readonly HashSet<Type> _types;
    private readonly JsonSerializerOptions _options;

    public DiscriminatorJsonConverter()
    {
        _discriminatorTypeMap = [];

        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                var attribute = type.GetCustomAttribute<DiscriminatorAttribute>();
                if (attribute != null)
                    _discriminatorTypeMap.Add(attribute.Discriminator ?? type.Name, type);
            }
        }

        _types = [.. _discriminatorTypeMap.Values.Distinct()];

        _options = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public object Deserialize(string json)
    {
        var obj = JsonSerializer.Deserialize<JsonDocument>(json, _options);

        if (obj is not null && obj.RootElement.TryGetProperty(DiscriminatorTag, out JsonElement property) && TryGetType(property.GetString() ?? "", out var type) && type is not null && CanConvert(type))
            return Deserialize(json, type);
        else
            throw new JsonException("Invalid Notification JSON document.");
    }

    public T Deserialize<T>(string json)
        => (T)Deserialize(json, GetDiscriminator(typeof(T)));

    public object Deserialize(string json, Type typeToConvert)
        => Deserialize(json, GetDiscriminator(typeToConvert));

    public object Deserialize(string json, string discriminator)
    {
        if (TryGetType(discriminator, out var typeToConvert) && CanConvert(typeToConvert!))
            return JsonSerializer.Deserialize(json, typeToConvert!, _options)!;
        else
            throw new JsonException("Type not supported.");
    }

    public string Serialize(object value)
    {
        var type = value.GetType();

        if (CanConvert(type))
        {
            var discriminator = GetDiscriminator(type);
            using MemoryStream memoryStream = new();
            using Utf8JsonWriter writer = new(memoryStream, new JsonWriterOptions { Indented = true });
            writer.WriteStartObject();
            writer.WriteString(DiscriminatorTag, discriminator);
            var json = JsonSerializer.Serialize(value, type, _options);
            var document = JsonDocument.Parse(json);
            var root = document.RootElement;
            foreach (var property in root.EnumerateObject())
                property.WriteTo(writer);
            writer.WriteEndObject();
            writer.Flush();
            return Encoding.UTF8.GetString(memoryStream.ToArray());
        }
        else
        {
            throw new JsonException("Discriminator not found on object.");
        }
    }

    public string Serialize<T>(T value)
        where T : notnull
        => Serialize((object)value);

    private bool CanConvert(Type typeToConvert)
        => _types.Contains(typeToConvert);

    private bool TryGetType(string discriminator, out Type? type)
    {
        var gotValue = _discriminatorTypeMap.TryGetValue(discriminator, out var t);
        type = t;
        return gotValue;
    }

    private static string GetDiscriminator(Type type)
    {
        var attribute = type.GetCustomAttribute<DiscriminatorAttribute>() ?? throw new ArgumentException("Discriminator not found on the type.");
        return attribute.Discriminator ?? type.Name;
    }

}
