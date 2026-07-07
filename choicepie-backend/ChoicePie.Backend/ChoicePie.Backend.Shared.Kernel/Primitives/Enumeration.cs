using System.Reflection;

namespace ChoicePie.Backend.Shared.Kernel.Primitives;

public abstract record Enumeration<TEnum>(int Id, string Name) where TEnum : Enumeration<TEnum>
{
    private static readonly Lazy<Dictionary<int, TEnum>> _enumerations =
        new(CreateEnumerations);

    public static IReadOnlyDictionary<int, TEnum> Enumerations => _enumerations.Value;
    public static TEnum? FromValue(int value) => Enumerations.GetValueOrDefault(value);

    public static TEnum? FromName(string name) => Enumerations.Values.SingleOrDefault(s =>
        string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));

    public override string ToString() => Name;
    public override int GetHashCode() => Id.GetHashCode();

    private static Dictionary<int, TEnum> CreateEnumerations()
    {
        var enumerationType = typeof(TEnum);
        var fields = enumerationType
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Where(f => enumerationType.IsAssignableFrom(f.FieldType))
            .Select(f => (TEnum)f.GetValue(null)!);

        return fields.ToDictionary(k => k.Id);
    }
}