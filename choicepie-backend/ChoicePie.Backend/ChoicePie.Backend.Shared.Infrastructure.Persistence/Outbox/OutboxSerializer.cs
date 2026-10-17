using System.Text.Json;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Domain;
using Microsoft.AspNetCore.DataProtection;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Outbox;

/// <summary>
/// 事件 ↔ Outbox 資料列。payload 一律加密：寄信事件帶著 raw token，資料庫原本只存雜湊，
/// 不能因為進了 Outbox 就讓明文落地；全部加密也免得日後新增敏感事件時忘了標記。
/// 型別名稱維持明文，方便查詢與排錯。
/// </summary>
public sealed class OutboxSerializer(IDataProtectionProvider dataProtectionProvider)
{
    private readonly IDataProtector _protector = dataProtectionProvider.CreateProtector("ChoicePie.Outbox.v1");

    public (string Type, string Payload) Serialize(BaseDomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType();
        var json = JsonSerializer.Serialize(domainEvent, eventType);

        return (TypeNameOf(eventType), _protector.Protect(json));
    }

    public BaseDomainEvent Deserialize(string type, string payload)
    {
        var eventType = Type.GetType(type);
        if (eventType is null || !typeof(BaseDomainEvent).IsAssignableFrom(eventType))
        {
            throw new InvalidOperationException($"Outbox 訊息的事件型別 '{type}' 已不存在或不是領域事件。");
        }

        return (BaseDomainEvent)JsonSerializer.Deserialize(_protector.Unprotect(payload), eventType)!;
    }

    // 不用 AssemblyQualifiedName：它帶版本號，組件版本一變，尚未處理的舊資料列就找不回型別。
    public static string TypeNameOf(Type eventType) => $"{eventType.FullName}, {eventType.Assembly.GetName().Name}";
}
