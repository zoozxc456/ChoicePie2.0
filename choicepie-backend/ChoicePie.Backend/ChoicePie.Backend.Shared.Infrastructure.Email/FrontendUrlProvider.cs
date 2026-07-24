using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Dependencies;
using Microsoft.Extensions.Options;

namespace ChoicePie.Backend.Shared.Infrastructure.Email;

public sealed class FrontendUrlProvider(IOptions<SmtpSettings> settings) : IFrontendUrlProvider, ISingletonDependency
{
    public string BaseUrl => settings.Value.FrontendBaseUrl;
}
