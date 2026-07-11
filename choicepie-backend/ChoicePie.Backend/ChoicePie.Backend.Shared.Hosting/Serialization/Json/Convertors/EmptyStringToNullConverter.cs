using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ChoicePie.Backend.Shared.Hosting.Serialization.Json.Convertors;

public class EmptyStringToNullConverter<T> : JsonConverter<T?> where T : struct
{
    private static readonly Dictionary<Type, Func<string, object?>> Parsers = InitializeParsers();

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();

            if (string.IsNullOrWhiteSpace(stringValue))
            {
                return null;
            }

            if (Parsers.TryGetValue(typeof(T), out var parser))
            {
                var resultObject = parser(stringValue);

                if (resultObject != null)
                {
                    return (T)resultObject;
                }
            }

            throw new JsonException($"Cannot parse string value '{stringValue}' into required type {typeof(T).Name}.");
        }

        // 3. 如果傳入的是 JSON null 或是其他非字串類型，使用預設邏輯
        if (reader.TokenType == JsonTokenType.Null)
        {
            return null;
        }


        return JsonSerializer.Deserialize<T>(ref reader, options);
    }

    public override void Write(Utf8JsonWriter writer, T? value, JsonSerializerOptions options)
    {
        if (value == null)
        {
            writer.WriteNullValue();
        }
        else
        {
            writer.WriteStringValue(value.Value.ToString());
        }
    }

    private static Dictionary<Type, Func<string, object?>> InitializeParsers()
    {
        var parsers = new Dictionary<Type, Func<string, object?>>
        {
            // --- GUID 解析策略 ---
            {
                typeof(Guid), value => Guid.TryParse(value, out var result) ? result : null
            },

            // --- DATETIME 解析策略 (ISO 8601) ---
            {
                typeof(DateTime), value =>
                    DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var result)
                        ? result
                        : null
            },

            // --- INT32 (int) 解析策略 ---
            {
                typeof(int), value =>
                    int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                        ? result
                        : null
            },

            // --- DECIMAL 解析策略 ---
            {
                typeof(decimal), value =>
                    decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                        ? result
                        : null
            },
            {
                typeof(double), value =>
                    double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                        ? result
                        : null
            },
            {
                typeof(long), value =>
                    long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result)
                        ? result
                        : null
            }
        };
        return parsers;
    }
}