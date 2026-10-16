using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;
using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Domain.Aggregates.RefreshToken.Specifications;

// 尚未被撤銷且未過期的 refresh token（不含「已輪替但仍在寬限期內」的舊 token，它們的下一顆會一併被撤銷）。
public sealed class ActiveRefreshTokensByOwnerSpecification(
    Guid ownerId, RefreshTokenOwnerType ownerType, DateTime utcNow)
    : Specification<RefreshToken>(t =>
        t.OwnerId == ownerId && t.OwnerType == ownerType && t.RevokedAt == null && t.ExpiresAt > utcNow);
