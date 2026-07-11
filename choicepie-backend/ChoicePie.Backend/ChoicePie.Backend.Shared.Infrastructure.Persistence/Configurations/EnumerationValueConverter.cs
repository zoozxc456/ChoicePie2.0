using ChoicePie.Backend.Shared.Kernel.Primitives;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Configurations;

public sealed class EnumerationValueConverter<TEnum>()
    : ValueConverter<TEnum, int>(e => e.Id, id => Enumeration<TEnum>.FromValue(id)!)
    where TEnum : Enumeration<TEnum>;
