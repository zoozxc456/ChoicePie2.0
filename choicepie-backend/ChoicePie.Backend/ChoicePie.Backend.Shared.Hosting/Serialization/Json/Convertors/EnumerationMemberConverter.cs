using System.Collections.Concurrent;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Shared.Hosting.Serialization.Json.Convertors;

public class EnumerationMemberConverter<T> : JsonConverter<T> 
    where T : Enumeration<T>
{
    private static readonly ConcurrentDictionary<T, string> EnumToStringCache = new();
    private static readonly ConcurrentDictionary<string, T> StringToEnumCache = new();

    static EnumerationMemberConverter()
    {
        foreach (var field in typeof(T).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var enumValue = (T)field.GetValue(null)!;
            var enumString = enumValue.Name; // 預設使用 Enum 成員名稱

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