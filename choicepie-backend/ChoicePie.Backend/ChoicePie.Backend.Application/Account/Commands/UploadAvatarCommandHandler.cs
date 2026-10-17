using ChoicePie.Backend.Application.Account.Dtos;
using ChoicePie.Backend.Application.Uploads.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ChoicePie.Backend.Application.Account.Commands;

public sealed class UploadAvatarCommandHandler(
    IMemberRepository memberRepository,
    IFileStorageReader fileStorageReader,
    IFileStorageWriter fileStorageWriter,
    ICurrentUserService currentUserService,
    IUnitOfWork unitOfWork,
    ILogger<UploadAvatarCommandHandler> logger)
    : IRequestHandler<UploadAvatarCommand, ProfileDto>
{
    // 與題庫封面上傳（UploadQuizCoverCommandHandler）相同的型別與大小限制與例外。
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;
    private const string AvatarKeyPrefix = "avatars";

    public async Task<ProfileDto> Handle(UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        var memberId = currentUserService.UserId ?? throw new UnauthenticatedException();

        if (!AllowedContentTypes.Contains(request.File.ContentType))
        {
            throw new InvalidFileTypeException(request.File.ContentType);
        }

        if (request.File.Length > MaxFileSizeBytes)
        {
            throw new FileTooLargeException(request.File.Length, MaxFileSizeBytes);
        }

        var member = await memberRepository.GetByIdAsync(memberId, cancellationToken)
                     ?? throw new MemberNotFoundException(memberId);

        var extension = Path.GetExtension(request.File.FileName);
        var destinationPath = $"{AvatarKeyPrefix}/{memberId}/{Guid.NewGuid()}{extension}";
        var avatarUrl = await fileStorageWriter.UploadFileAsync(request.File, destinationPath, cancellationToken);

        var previousAvatar = member.ChangeAvatar(avatarUrl);

        await memberRepository.UpdateAsync(member, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        await TryDeletePreviousAvatarAsync(memberId, previousAvatar, cancellationToken);

        return new ProfileDto(member.Name, member.Avatar);
    }

    // 舊頭像 best-effort 清除：只刪自己名下 avatars/{memberId}/ 底下的檔案（避免誤刪外部網址），
    // 失敗只記 log——孤兒檔案不影響使用者，也不該讓已成功的更新回報失敗。
    private async Task TryDeletePreviousAvatarAsync(Guid memberId, string? previousAvatar, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(previousAvatar))
        {
            return;
        }

        try
        {
            var objectName = fileStorageReader.ExtractObjectNameFromUrl(previousAvatar);
            if (objectName.StartsWith($"{AvatarKeyPrefix}/{memberId}/", StringComparison.Ordinal))
            {
                await fileStorageWriter.DeleteFileAsync(previousAvatar, ct);
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to delete previous avatar {Avatar} of member {MemberId}", previousAvatar,
                memberId);
        }
    }
}
