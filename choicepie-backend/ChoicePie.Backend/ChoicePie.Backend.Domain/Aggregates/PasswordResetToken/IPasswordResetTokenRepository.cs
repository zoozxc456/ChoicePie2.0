using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;

namespace ChoicePie.Backend.Domain.Aggregates.PasswordResetToken;

public interface IPasswordResetTokenRepository : IRepository<PasswordResetToken>;
