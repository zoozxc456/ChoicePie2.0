using ChoicePie.Backend.Domain.Aggregates.RefreshToken;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Specifications;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;

namespace ChoicePie.Backend.Application.AdminUsers.Commands;

public sealed class AdminLogoutCommandHandler(
    IRefreshTokenRepository refreshTokenRepository,
    IRefreshTokenGenerator refreshTokenGenerator,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider)
    : IRequestHandler<AdminLogoutCommand>
{
    public async Task Handle(AdminLogoutCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = refreshTokenGenerator.Hash(request.RefreshToken);
        var existingToken =
            await refreshTokenRepository.FirstOrDefaultAsync(new RefreshTokenByTokenHashSpecification(tokenHash),
                cancellationToken);

        if (existingToken is null || !existingToken.IsActive)
        {
            return;
        }

        existingToken.Revoke(timeProvider.GetUtcNow().UtcDateTime);
        await refreshTokenRepository.UpdateAsync(existingToken, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
