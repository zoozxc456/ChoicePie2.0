using ChoicePie.Backend.Application.Account.Dtos;

namespace ChoicePie.Backend.Application.Account.Contracts;

public interface IAccountQueryService
{
    Task<AccountDto> GetByMemberIdAsync(Guid memberId, CancellationToken cancellationToken);
}
