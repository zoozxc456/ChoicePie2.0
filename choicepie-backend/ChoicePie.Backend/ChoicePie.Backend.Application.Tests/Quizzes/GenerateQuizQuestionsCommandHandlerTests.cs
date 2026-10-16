using ChoicePie.Backend.Application.AiUsage.Contracts;
using ChoicePie.Backend.Application.AiUsage.Dtos;
using ChoicePie.Backend.Application.Quizzes;
using ChoicePie.Backend.Application.Quizzes.Commands;
using ChoicePie.Backend.Application.Quizzes.Contracts;
using ChoicePie.Backend.Domain.Aggregates.AiUsageLog;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.MembershipTier;
using ChoicePie.Backend.Domain.Aggregates.MembershipTier.Exceptions;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Exceptions;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Data;
using ChoicePie.Backend.Shared.Kernel.Abstractions.Settings;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace ChoicePie.Backend.Application.Tests.Quizzes;

[TestFixture]
public class GenerateQuizQuestionsCommandHandlerTests
{
    private IMemberRepository _memberRepository = null!;
    private IMembershipTierRepository _membershipTierRepository = null!;
    private IAiUsageLogRepository _aiUsageLogRepository = null!;
    private IAiUsageLogQueryService _aiUsageLogQueryService = null!;
    private ICurrentUserService _currentUserService = null!;
    private IQuizGenerationService _generationService = null!;
    private IWebPageContentFetcher _webPageContentFetcher = null!;
    private IOptions<AiQuizGenerationSettings> _aiSettings = null!;
    private IUnitOfWork _unitOfWork = null!;
    private TimeProvider _timeProvider = null!;
    private GenerateQuizQuestionsCommandHandler _sut = null!;
    private Member _member = null!;
    private MembershipTier _tier = null!;

    [SetUp]
    public void SetUp()
    {
        _memberRepository = Substitute.For<IMemberRepository>();
        _membershipTierRepository = Substitute.For<IMembershipTierRepository>();
        _aiUsageLogRepository = Substitute.For<IAiUsageLogRepository>();
        _aiUsageLogQueryService = Substitute.For<IAiUsageLogQueryService>();
        _currentUserService = Substitute.For<ICurrentUserService>();
        _generationService = Substitute.For<IQuizGenerationService>();
        _webPageContentFetcher = Substitute.For<IWebPageContentFetcher>();
        _aiSettings = Options.Create(new AiQuizGenerationSettings
        {
            Provider = "Anthropic", ApiKey = "test-key", Model = "claude-sonnet-5"
        });
        _unitOfWork = Substitute.For<IUnitOfWork>();
        _timeProvider = Substitute.For<TimeProvider>();
        _timeProvider.GetUtcNow().Returns(DateTimeOffset.UtcNow);
        _sut = new GenerateQuizQuestionsCommandHandler(
            _memberRepository, _membershipTierRepository, _aiUsageLogRepository, _aiUsageLogQueryService,
            _currentUserService, _generationService, _webPageContentFetcher, _aiSettings, _unitOfWork, _timeProvider);

        _member = Member.Create("Host Name");
        _tier = MembershipTier.Create("Free", dailyGenerationLimit: 3, dailyTokenBudget: 10_000, isDefault: true);
        _member.AssignTier(_tier.Id);

        _currentUserService.UserId.Returns(_member.Id);
        _memberRepository.GetByIdAsync(_member.Id, Arg.Any<CancellationToken>()).Returns(_member);
        _membershipTierRepository.GetByIdAsync(_tier.Id, Arg.Any<CancellationToken>()).Returns(_tier);
        _aiUsageLogQueryService.GetTodayUsageAsync(_member.Id, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new DailyAiUsageDto(GenerationCount: 0, TokensUsed: 0));
    }

    [TearDown]
    public void TearDown() => _unitOfWork.Dispose();

    private static GenerateQuizQuestionsCommand ValidCommand() => new()
    {
        Content = new string('x', 30),
        QuestionCount = 5,
        Difficulty = "beginner"
    };

    [Test]
    public async Task Handle_GivenValidRequest_WhenCalled_ThenReturnsGeneratedQuestionsAndRecordsUsage()
    {
        _generationService.GenerateAsync(Arg.Any<string>(), 5, Difficulty.Beginner, Arg.Any<CancellationToken>(), Arg.Any<WebPageSource?>())
            .Returns(new GeneratedQuestionsResult(
                [new GeneratedQuestion("Q1?", ["A", "B", "C", "D"], 0, "because")], 42));

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result.TokensUsed, Is.EqualTo(42));
            Assert.That(result.Questions, Has.Count.EqualTo(1));
        });
        Assert.That(_member.LastAiGenerationAt, Is.Not.Null);
        await _unitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
        await _aiUsageLogRepository.Received(1).AddAsync(
            Arg.Is<AiUsageLog>(l =>
                l.MemberId == _member.Id && l.TokensUsed == 42 && l.Provider == "Anthropic" && l.Model == "claude-sonnet-5"),
            Arg.Any<CancellationToken>());
    }

    [Test]
    public void Handle_GivenTodayGenerationCountReachedTierLimit_WhenCalled_ThenThrowsAiGenerationQuotaExceededException()
    {
        _aiUsageLogQueryService.GetTodayUsageAsync(_member.Id, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new DailyAiUsageDto(GenerationCount: 3, TokensUsed: 100));

        Assert.ThrowsAsync<AiGenerationQuotaExceededException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [Test]
    public void Handle_GivenTodayTokensReachedTierBudget_WhenCalled_ThenThrowsAiTokenBudgetExceededException()
    {
        _aiUsageLogQueryService.GetTodayUsageAsync(_member.Id, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new DailyAiUsageDto(GenerationCount: 1, TokensUsed: 10_000));

        Assert.ThrowsAsync<AiTokenBudgetExceededException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [Test]
    public async Task Handle_GivenTodayUsageJustUnderTierBudget_WhenCalled_ThenAllowsThisCallThroughEvenIfItPushesOverBudget()
    {
        _aiUsageLogQueryService.GetTodayUsageAsync(_member.Id, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new DailyAiUsageDto(GenerationCount: 1, TokensUsed: 9_999));
        _generationService.GenerateAsync(Arg.Any<string>(), 5, Difficulty.Beginner, Arg.Any<CancellationToken>(), Arg.Any<WebPageSource?>())
            .Returns(new GeneratedQuestionsResult(
                [new GeneratedQuestion("Q1?", ["A", "B", "C", "D"], 0, "because")], 500));

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.That(result.TokensUsed, Is.EqualTo(500));
    }

    [Test]
    public void Handle_GivenMemberTierNotFound_WhenCalled_ThenThrowsMembershipTierNotFoundException()
    {
        _membershipTierRepository.GetByIdAsync(_tier.Id, Arg.Any<CancellationToken>()).Returns((MembershipTier?)null);

        Assert.ThrowsAsync<MembershipTierNotFoundException>(() => _sut.Handle(ValidCommand(), CancellationToken.None));
    }

    [Test]
    public async Task Handle_GivenMemberWithNoTierAssigned_WhenCalled_ThenSkipsQuotaCheckAndGenerates()
    {
        var memberWithoutTier = Member.Create("No Tier Member");
        _currentUserService.UserId.Returns(memberWithoutTier.Id);
        _memberRepository.GetByIdAsync(memberWithoutTier.Id, Arg.Any<CancellationToken>()).Returns(memberWithoutTier);
        _generationService.GenerateAsync(Arg.Any<string>(), 5, Difficulty.Beginner, Arg.Any<CancellationToken>(), Arg.Any<WebPageSource?>())
            .Returns(new GeneratedQuestionsResult(
                [new GeneratedQuestion("Q1?", ["A", "B", "C", "D"], 0, "because")], 10));

        var result = await _sut.Handle(ValidCommand(), CancellationToken.None);

        Assert.That(result.TokensUsed, Is.EqualTo(10));
    }

    [TestCase(1)]
    [TestCase(4)]
    [TestCase(7)]
    public void Handle_GivenDisallowedQuestionCount_WhenCalled_ThenThrowsInvalidQuizException(int count)
    {
        var command = new GenerateQuizQuestionsCommand
        {
            Content = new string('x', 30), QuestionCount = count, Difficulty = "beginner"
        };

        Assert.ThrowsAsync<InvalidQuizException>(() => _sut.Handle(command, CancellationToken.None));
    }

    [Test]
    public void Handle_GivenUnknownDifficulty_WhenCalled_ThenThrowsInvalidQuizException()
    {
        var command = new GenerateQuizQuestionsCommand
        {
            Content = new string('x', 30), QuestionCount = 5, Difficulty = "legendary"
        };

        Assert.ThrowsAsync<InvalidQuizException>(() => _sut.Handle(command, CancellationToken.None));
    }

    private const string PageUrl = "https://example.com/article";

    private static GenerateQuizQuestionsCommand UrlCommand(string content = PageUrl) => new()
    {
        Content = content, QuestionCount = 5, Difficulty = "beginner"
    };

    private void GivenGenerationSucceeds() =>
        _generationService.GenerateAsync(
                Arg.Any<string>(), 5, Difficulty.Beginner, Arg.Any<CancellationToken>(), Arg.Any<WebPageSource?>())
            .Returns(new GeneratedQuestionsResult(
                [new GeneratedQuestion("Q1?", ["A", "B", "C", "D"], 0, "because")], 42));

    [Test]
    public async Task Handle_GivenUrlInput_WhenCalled_ThenFetchesPageAndGeneratesFromExtractedTextWithSource()
    {
        var text = new string('字', 100);
        _webPageContentFetcher.FetchAsync(Arg.Any<Uri>(), Arg.Any<CancellationToken>())
            .Returns(new WebPageContent(new Uri("https://example.com/final"), "標題", text));
        GivenGenerationSucceeds();

        await _sut.Handle(UrlCommand("  " + PageUrl + "  "), CancellationToken.None);

        await _webPageContentFetcher.Received(1).FetchAsync(new Uri(PageUrl), Arg.Any<CancellationToken>());
        await _generationService.Received(1).GenerateAsync(
            text, 5, Difficulty.Beginner, Arg.Any<CancellationToken>(),
            Arg.Is<WebPageSource?>(s => s != null && s.Url == new Uri("https://example.com/final") && s.Title == "標題"));
    }

    [Test]
    public async Task Handle_GivenUrlInputWithVeryLongPage_WhenCalled_ThenTruncatesToMaxContentLength()
    {
        _webPageContentFetcher.FetchAsync(Arg.Any<Uri>(), Arg.Any<CancellationToken>())
            .Returns(new WebPageContent(new Uri(PageUrl), null, new string('a', 20_000)));
        GivenGenerationSucceeds();

        await _sut.Handle(UrlCommand(), CancellationToken.None);

        await _generationService.Received(1).GenerateAsync(
            Arg.Is<string>(c => c.Length == QuizContentInput.MaxContentLength), 5, Difficulty.Beginner,
            Arg.Any<CancellationToken>(), Arg.Any<WebPageSource?>());
    }

    [Test]
    public async Task Handle_GivenPlainTextInput_WhenCalled_ThenDoesNotFetchAndPassesNoSource()
    {
        GivenGenerationSucceeds();

        await _sut.Handle(ValidCommand(), CancellationToken.None);

        await _webPageContentFetcher.DidNotReceiveWithAnyArgs().FetchAsync(default!, default);
        await _generationService.Received(1).GenerateAsync(
            ValidCommand().Content, 5, Difficulty.Beginner, Arg.Any<CancellationToken>(), null);
    }

    [Test]
    public void Handle_GivenUrlWhoseExtractedTextIsTooShort_WhenCalled_ThenThrowsContentInsufficientAndConsumesNothing()
    {
        _webPageContentFetcher.FetchAsync(Arg.Any<Uri>(), Arg.Any<CancellationToken>())
            .Returns(new WebPageContent(new Uri(PageUrl), "t", "太短"));

        Assert.ThrowsAsync<WebPageContentInsufficientException>(() => _sut.Handle(UrlCommand(), CancellationToken.None));

        AssertNoQuotaConsumed();
    }

    [Test]
    public void Handle_GivenFetchFails_WhenCalled_ThenPropagatesErrorAndConsumesNoQuota()
    {
        _webPageContentFetcher.FetchAsync(Arg.Any<Uri>(), Arg.Any<CancellationToken>())
            .Returns<WebPageContent>(_ => throw new WebPageFetchFailedException("boom"));

        Assert.ThrowsAsync<WebPageFetchFailedException>(() => _sut.Handle(UrlCommand(), CancellationToken.None));

        AssertNoQuotaConsumed();
    }

    [TestCase("ftp://example.com/file.txt")]
    [TestCase("http://")]
    [TestCase("file:///etc/passwd")]
    public void Handle_GivenUnsupportedUrlInput_WhenCalled_ThenThrowsUnsupportedWithoutFetching(string input)
    {
        Assert.ThrowsAsync<UnsupportedWebPageUrlException>(() => _sut.Handle(UrlCommand(input), CancellationToken.None));

        _webPageContentFetcher.DidNotReceiveWithAnyArgs().FetchAsync(default!, default);
    }

    [Test]
    public void Handle_GivenUrlInputAndQuotaExhausted_WhenCalled_ThenThrowsQuotaExceededWithoutFetching()
    {
        _aiUsageLogQueryService.GetTodayUsageAsync(_member.Id, Arg.Any<DateTime>(), Arg.Any<CancellationToken>())
            .Returns(new DailyAiUsageDto(GenerationCount: 3, TokensUsed: 0));

        Assert.ThrowsAsync<AiGenerationQuotaExceededException>(() => _sut.Handle(UrlCommand(), CancellationToken.None));

        _webPageContentFetcher.DidNotReceiveWithAnyArgs().FetchAsync(default!, default);
    }

    private void AssertNoQuotaConsumed()
    {
        Assert.That(_member.LastAiGenerationAt, Is.Null);
        _unitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(default);
        _aiUsageLogRepository.DidNotReceiveWithAnyArgs().AddAsync(default!, default);
        _generationService.DidNotReceiveWithAnyArgs().GenerateAsync(default!, default, default!, default);
    }
}
