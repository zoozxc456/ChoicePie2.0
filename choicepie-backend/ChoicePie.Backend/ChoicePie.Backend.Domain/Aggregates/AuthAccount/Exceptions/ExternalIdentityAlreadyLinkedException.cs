using System.Net;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;
using ChoicePie.Backend.Shared.Kernel.Exceptions;

namespace ChoicePie.Backend.Domain.Aggregates.AuthAccount.Exceptions;

public sealed class ExternalIdentityAlreadyLinkedException(LoginProvider provider)
    : DomainException(
        internalLogMessage: $"External identity of provider {provider.Name} is already linked to another account.",
        presentationMessage: "此 Google 帳號已連結到其他 ChoicePie 帳號。",
        errorCode: "EXTERNAL_IDENTITY_ALREADY_LINKED",
        statusCode: HttpStatusCode.Conflict);
