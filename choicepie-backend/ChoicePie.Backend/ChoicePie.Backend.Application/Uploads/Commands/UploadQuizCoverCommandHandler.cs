using ChoicePie.Backend.Application.Uploads.Dtos;
using ChoicePie.Backend.Application.Uploads.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using MediatR;

namespace ChoicePie.Backend.Application.Uploads.Commands;

public sealed class UploadQuizCoverCommandHandler(
    IFileStorageService fileStorageService,
    ICurrentUserService currentUserService)
    : IRequestHandler<UploadQuizCoverCommand, UploadQuizCoverResultDto>
{
    private static readonly string[] AllowedContentTypes = ["image/jpeg", "image/png", "image/webp"];
    private const long MaxFileSizeBytes = 5 * 1024 * 1024;

    public async Task<UploadQuizCoverResultDto> Handle(UploadQuizCoverCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthenticatedException();

        if (!AllowedContentTypes.Contains(request.File.ContentType))
        {
            throw new InvalidFileTypeException(request.File.ContentType);
        }

        if (request.File.Length > MaxFileSizeBytes)
        {
            throw new FileTooLargeException(request.File.Length, MaxFileSizeBytes);
        }

        var ext = Path.GetExtension(request.File.FileName);
        var destinationPath = $"quiz-covers/{userId}/{Guid.NewGuid()}{ext}";

        var url = await fileStorageService.UploadFileAsync(request.File, destinationPath, cancellationToken);

        return new UploadQuizCoverResultDto(url);
    }
}
