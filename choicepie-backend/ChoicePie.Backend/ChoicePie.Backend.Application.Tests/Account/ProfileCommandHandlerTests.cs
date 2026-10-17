using ChoicePie.Backend.Application.Account.Commands;
using ChoicePie.Backend.Application.Uploads.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Member.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Account;

[TestFixture]
public class ProfileCommandHandlerTests
{
    private IMemberRepository _memberRepository = null!;
    private IFileStorageReader _fileStorageReader = null!;
    private IFileStorageWriter _fileStorageWriter = null!;
    private ICurrentUserService _currentUserService = null!;
    private IUnitOfWork _unitOfWork = null!;
    private Member _member = null!;

    [SetUp]
    public void SetUp()
    {
        _memberRepository = Substitute.For<IMemberRepository>();
        _fileStorageReader = Substitute.For<IFileStorageReader>();
        _fileStorageWriter = Substitute.For<IFileStorageWriter>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _member = Member.Create("Host Name");
        _currentUserService.UserId.Returns(_member.Id);
        _memberRepository.GetByIdAsync(_member.Id, Arg.Any<CancellationToken>()).Returns(_member);
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    private UpdateProfileCommandHandler ProfileSut() => new(_memberRepository, _currentUserService, _unitOfWork);

    private UploadAvatarCommandHandler AvatarSut() =>
        new(_memberRepository, _fileStorageReader, _fileStorageWriter, _currentUserService, _unitOfWork,
            NullLogger<UploadAvatarCommandHandler>.Instance);

    private static IFormFile File(string contentType, long length, string name = "me.png")
    {
        var file = Substitute.For<IFormFile>();
        file.ContentType.Returns(contentType);
        file.Length.Returns(length);
        file.FileName.Returns(name);
        return file;
    }

    [Test]
    public async Task UpdateProfile_GivenValidName_WhenCalled_ThenRenamesAndSaves()
    {
        var result = await ProfileSut().Handle(new UpdateProfileCommand { Name = "New Name" }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo("New Name"));
            Assert.That(_member.Name, Is.EqualTo("New Name"));
        });
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void UpdateProfile_GivenInvalidName_WhenCalled_ThenThrowsInvalidMemberNameException()
    {
        Assert.ThrowsAsync<InvalidMemberNameException>(() =>
            ProfileSut().Handle(new UpdateProfileCommand { Name = "x" }, CancellationToken.None));
    }

    [Test]
    public void UpdateProfile_GivenUnauthenticatedUser_WhenCalled_ThenThrowsUnauthenticatedException()
    {
        _currentUserService.UserId.Returns((Guid?)null);

        Assert.ThrowsAsync<UnauthenticatedException>(() =>
            ProfileSut().Handle(new UpdateProfileCommand { Name = "New Name" }, CancellationToken.None));
    }

    [Test]
    public async Task UploadAvatar_GivenValidImage_WhenCalled_ThenStoresUnderAvatarPrefixAndSetsAvatar()
    {
        var file = File("image/png", 1000);
        _fileStorageWriter.UploadFileAsync(file, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("https://cdn/avatars/x.png");

        var result = await AvatarSut().Handle(new UploadAvatarCommand { File = file }, CancellationToken.None);

        Assert.That(result.Avatar, Is.EqualTo("https://cdn/avatars/x.png"));
        await _fileStorageWriter.Received(1).UploadFileAsync(file,
            Arg.Is<string>(p => p.StartsWith($"avatars/{_member.Id}/") && p.EndsWith(".png")),
            Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Test]
    public void UploadAvatar_GivenUnsupportedType_WhenCalled_ThenThrowsInvalidFileTypeException()
    {
        Assert.ThrowsAsync<InvalidFileTypeException>(() => AvatarSut().Handle(
            new UploadAvatarCommand { File = File("image/gif", 1000) }, CancellationToken.None));
    }

    [Test]
    public void UploadAvatar_GivenTooLargeFile_WhenCalled_ThenThrowsFileTooLargeException()
    {
        Assert.ThrowsAsync<FileTooLargeException>(() => AvatarSut().Handle(
            new UploadAvatarCommand { File = File("image/png", 6 * 1024 * 1024) }, CancellationToken.None));
    }

    [Test]
    public async Task UploadAvatar_GivenPreviousOwnAvatar_WhenCalled_ThenDeletesOldFile()
    {
        var oldUrl = $"https://cdn/avatars/{_member.Id}/old.png";
        _member.ChangeAvatar(oldUrl);
        var file = File("image/png", 1000);
        _fileStorageWriter.UploadFileAsync(file, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("https://cdn/new.png");
        _fileStorageReader.ExtractObjectNameFromUrl(oldUrl).Returns($"avatars/{_member.Id}/old.png");

        await AvatarSut().Handle(new UploadAvatarCommand { File = file }, CancellationToken.None);

        await _fileStorageWriter.Received(1).DeleteFileAsync(oldUrl, Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task UploadAvatar_GivenPreviousForeignAvatar_WhenCalled_ThenDoesNotDeleteIt()
    {
        var oldUrl = "https://lh3.googleusercontent.com/pic";
        _member.ChangeAvatar(oldUrl);
        var file = File("image/png", 1000);
        _fileStorageWriter.UploadFileAsync(file, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("https://cdn/new.png");
        _fileStorageReader.ExtractObjectNameFromUrl(oldUrl).Returns("pic");

        await AvatarSut().Handle(new UploadAvatarCommand { File = file }, CancellationToken.None);

        await _fileStorageWriter.DidNotReceive().DeleteFileAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task UploadAvatar_GivenDeleteFailure_WhenCalled_ThenStillSucceeds()
    {
        var oldUrl = $"https://cdn/avatars/{_member.Id}/old.png";
        _member.ChangeAvatar(oldUrl);
        var file = File("image/png", 1000);
        _fileStorageWriter.UploadFileAsync(file, Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns("https://cdn/new.png");
        _fileStorageReader.ExtractObjectNameFromUrl(oldUrl).Returns($"avatars/{_member.Id}/old.png");
        _fileStorageWriter.DeleteFileAsync(oldUrl, Arg.Any<CancellationToken>())
            .Returns(Task.FromException(new InvalidOperationException("r2 down")));

        var result = await AvatarSut().Handle(new UploadAvatarCommand { File = file }, CancellationToken.None);

        Assert.That(result.Avatar, Is.EqualTo("https://cdn/new.png"));
    }
}
