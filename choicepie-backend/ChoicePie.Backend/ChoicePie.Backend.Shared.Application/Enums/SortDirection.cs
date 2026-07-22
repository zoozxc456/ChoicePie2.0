using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ChoicePie.Backend.Shared.Application.Enums;

[JsonConverter(typeof(StringEnumConverter))]
public enum SortDirection
{
    [Description("asc")] [EnumMember(Value = "asc")]
    Asc,
    
    [Description("desc")] [EnumMember(Value = "desc")]
    Desc
}