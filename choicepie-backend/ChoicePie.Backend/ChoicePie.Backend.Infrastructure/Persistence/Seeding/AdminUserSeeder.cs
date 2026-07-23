using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AdminUser;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Enums;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Abstractions;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using Microsoft.Extensions.Options;

namespace ChoicePie.Backend.Infrastructure.Persistence.Seeding;

public sealed class AdminUserSeeder(
    IAdminUserRepository adminUserRepository,
    IAdminAuthAccountRepository adminAuthAccountRepository,
    IPasswordHasher passwordHasher,
    IUnitOfWork unitOfWork,
    IOptions<AdminBootstrapSettings> adminBootstrapSettings)
    : IDataSeeder, IScopedDependency
{
    public int Order => 0;

    public async Task SeedAsync()
    {
        var existingAdmin = await adminUserRepository.GetOneAsync();
        if (existingAdmin is not null)
        {
            return;
        }

        var settings = adminBootstrapSettings.Value;
        if (string.IsNullOrWhiteSpace(settings.Email) ||
            string.IsNullOrWhiteSpace(settings.Name) ||
            string.IsNullOrWhiteSpace(settings.Password))
        {
            return;
        }

        var email = Email.Create(settings.Email);
        var hashedPassword = passwordHasher.Hash(settings.Password);

        var adminUser = AdminUser.Create(settings.Name, AdminRole.SystemAdmin);
        var adminAuthAccount = AdminAuthAccount.Create(email, hashedPassword, adminUser.Id);

        await adminUserRepository.AddAsync(adminUser);
        await adminAuthAccountRepository.AddAsync(adminAuthAccount);
        await unitOfWork.SaveChangesAsync();
    }
}
