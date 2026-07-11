using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChoicePie.Backend.Shared.Hosting.Serialization.Json.Convertors;

public class EnumMemberConverter<T> : JsonConverter<T> where T : struct, Enum
{
    private static readonly ConcurrentDictionary<T, string> EnumToStringCache = new();
    private static readonly ConcurrentDictionary<string, T> StringToEnumCache = new();

    static EnumMemberConverter()
    {
        foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var enumValue = (T)field.GetValue(null)!;
            var enumMemberAttr = field.GetCustomAttribute<EnumMemberAttribute>();
            var enumString = enumMemberAttr?.Value ?? field.Name; // 預設使用 Enum 成員名稱

            EnumToStringCache.TryAdd(enumValue, enumString);
            StringToEnumCache.TryAdd(enumString, enumValue);
        }
    }

    public override T Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var enumString = reader.GetString();
        return StringToEnumCache.TryGetValue(enumString!, out var value)
            ? value
            : throw new JsonException($"Invalid enum value: {enumString}");
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        if (EnumToStringCache.TryGetValue(value, out var enumString))
        {
            writer.WriteStringValue(enumString);
        }
        else
        {
            throw new JsonException($"Unknown enum value: {value}");
        }
    }
}