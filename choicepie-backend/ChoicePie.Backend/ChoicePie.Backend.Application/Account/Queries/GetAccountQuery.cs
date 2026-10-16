using ChoicePie.Backend.Application.Account.Dtos;
using MediatR;

namespace ChoicePie.Backend.Application.Account.Queries;

public sealed class GetAccountQuery : IRequest<AccountDto>;
