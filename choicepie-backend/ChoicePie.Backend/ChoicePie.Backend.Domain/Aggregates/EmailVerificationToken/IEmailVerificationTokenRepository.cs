using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;

namespace ChoicePie.Backend.Domain.Aggregates.EmailVerificationToken;

public interface IEmailVerificationTokenRepository : IRepository<EmailVerificationToken>;
