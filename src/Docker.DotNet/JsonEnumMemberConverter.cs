using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Docker.DotNet;

// Maps enum values to their [EnumMember] names. This deliberately does not build on
// JsonStringEnumConverter with a naming policy: since System.Text.Json 10 that rejects
// empty names, which Docker uses (e.g. RestartPolicyKind.Undefined => "").
internal sealed class JsonEnumMemberConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
{
    private static readonly Dictionary<TEnum, string> ValueToName = new Dictionary<TEnum, string>();
    private static readonly Dictionary<string, TEnum> NameToValue = new Dictionary<string, TEnum>(StringComparer.OrdinalIgnoreCase);

    static JsonEnumMemberConverter()
    {
        foreach (var field in typeof(TEnum).GetFields(BindingFlags.Public | BindingFlags.Static))
        {
            var value = (TEnum)field.GetValue(null);
            var name = field.GetCustomAttribute<EnumMemberAttribute>()?.Value ?? field.Name;

            ValueToName[value] = name;

            if (!NameToValue.ContainsKey(name))
            {
                NameToValue[name] = value;
            }
        }
    }

    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        switch (reader.TokenType)
        {
            case JsonTokenType.String:
                var name = reader.GetString();

                if (NameToValue.TryGetValue(name, out var value) || Enum.TryParse(name, true, out value))
                {
                    return value;
                }

                throw new JsonException($"The JSON value '{name}' could not be converted to {typeof(TEnum)}.");

            case JsonTokenType.Number:
                return (TEnum)Enum.ToObject(typeof(TEnum), reader.GetInt64());

            default:
                throw new JsonException($"Unexpected token {reader.TokenType} when parsing {typeof(TEnum)}.");
        }
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        if (ValueToName.TryGetValue(value, out var name))
        {
            writer.WriteStringValue(name);
        }
        else
        {
            writer.WriteNumberValue(Convert.ToInt64(value));
        }
    }
}
