using ChoicePie.Backend.Domain.Aggregates.AdminAuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AdminUser;
using ChoicePie.Backend.Domain.Aggregates.AdminUser.Enums;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount;
using ChoicePie.Backend.Domain.Aggregates.AuthAccount.Enums;
using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;
using ChoicePie.Backend.Domain.Aggregates.Member;
using ChoicePie.Backend.Domain.Aggregates.Quiz;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Entities;
using ChoicePie.Backend.Domain.Aggregates.Quiz.Enums;
using ChoicePie.Backend.Domain.Aggregates.QuizAttempt;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken;
using ChoicePie.Backend.Domain.Aggregates.RefreshToken.Enums;
using ChoicePie.Backend.Infrastructure.Persistence.Contexts;
using ChoicePie.Backend.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Application.Interfaces;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Repositories;
using ChoicePie.Backend.Shared.Kernel.ValueObjects;
using Microsoft.Extensions.DependencyInjection;
using GameRoomAggregate = ChoicePie.Backend.Domain.Aggregates.GameRoom.GameRoom;
using GameSessionAggregate = ChoicePie.Backend.Domain.Aggregates.GameSession.GameSession;
using QuizAttemptAggregate = ChoicePie.Backend.Domain.Aggregates.QuizAttempt.QuizAttempt;
using RefreshTokenAggregate = ChoicePie.Backend.Domain.Aggregates.RefreshToken.RefreshToken;

namespace ChoicePie.Backend.WebApi.Tests;

/// <summary>
/// 直接對 Testcontainers Postgres 測試每個 repository 自己覆寫的部分（主要是 ApplyInclude 的
/// eager-loading 有沒有接對）——泛型 CRUD（AddAsync/UpdateAsync/DeleteAsync）已經被
/// QuizzesController/QuizAttemptsController/GameHub 等整合測試間接、大量覆蓋過，這裡不重複測。
/// </summary>
public sealed class RepositoryIntegrationTests
{
    private CustomWebApplicationFactory _factory = null!;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        _factory = new CustomWebApplicationFactory();
        await _factory.InitializeAsync();
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown() => await _factory.DisposeAsync();

    [Test]
    public async Task QuizRepository_GetByIdAsync_GivenQuizWithQuestions_WhenCalled_ThenEagerLoadsQuestions()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChoicePieDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IQuizRepository>();

        var member = Member.Create("Quiz Owner");
        var quiz = Quiz.Create(member.Id, "Repo Test Quiz", null, "🎯", "grad", Difficulty.Beginner, ["test"]);
        quiz.AddQuestion(Question.Create("Q?", ["1", "2", "3", "4"], 0, "e"));
        dbContext.Add(member);
        dbContext.Add(quiz);
        await dbContext.SaveChangesAsync();

        var loaded = await repository.GetByIdAsync(quiz.Id);

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.Questions, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task QuizAttemptRepository_GetByIdAsync_GivenAttemptWithAnswers_WhenCalled_ThenEagerLoadsAnswers()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChoicePieDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IQuizAttemptRepository>();

        var member = Member.Create("Attempt Owner");
        var quiz = Quiz.Create(member.Id, "Repo Attempt Quiz", null, "🎯", "grad", Difficulty.Beginner, ["test"]);
        var question = Question.Create("Q?", ["1", "2", "3", "4"], 1, "e");
        quiz.AddQuestion(question);
        var attempt = QuizAttemptAggregate.Start(quiz.Id, member.Id, [question.Id], DateTime.UtcNow);
        attempt.SubmitAnswer(question.Id, 1, DateTime.UtcNow);
        dbContext.Add(member);
        dbContext.Add(quiz);
        dbContext.Add(attempt);
        await dbContext.SaveChangesAsync();

        var loaded = await repository.GetByIdAsync(attempt.Id);

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.Answers, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task GameSessionRepository_GetByIdAsync_GivenPlayedSession_WhenCalled_ThenEagerLoadsQuestionsAndPlayerResults()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChoicePieDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<Domain.Aggregates.GameSession.IGameSessionRepository>();

        var hostUserId = Guid.NewGuid();
        var quizId = Guid.NewGuid();
        var questions = new List<GameQuestionSnapshot> { new(Guid.NewGuid(), "1+1=?", ["1", "2", "3", "4"], 1, "e") };
        var room = GameRoomAggregate.Create(hostUserId, "RPO123", quizId, "Repo Session Quiz", "🎯", "grad", questions, 20, DateTime.UtcNow);
        var startedAt = DateTime.UtcNow;
        var player = room.Join("Player", "conn-1", startedAt);
        room.StartGame(hostUserId, startedAt);
        room.SubmitAnswer(player.Id, 1, startedAt.AddSeconds(1));
        room.EndCurrentQuestion(hostUserId, startedAt.AddSeconds(5));
        room.AdvanceToNextQuestion(hostUserId, startedAt.AddSeconds(6));
        var session = GameSessionAggregate.RecordFrom(room, startedAt.AddMinutes(1));

        dbContext.Add(session);
        await dbContext.SaveChangesAsync();

        var loaded = await repository.GetByIdAsync(session.Id);

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.Questions, Has.Count.EqualTo(1));
        Assert.That(loaded.PlayerResults, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task AuthAccountRepository_GetByIdAsync_GivenAccountWithLoginMethod_WhenCalled_ThenEagerLoadsLoginMethods()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChoicePieDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IAuthAccountRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var member = Member.Create("Auth Account Owner");
        var authAccount = AuthAccount.Register(
            Email.Create($"{Guid.NewGuid()}@example.com"), passwordHasher.Hash("Password123!"), member.Id);
        authAccount.AddLoginMethod(LoginProvider.Google, "google-sub-123");
        dbContext.Add(member);
        dbContext.Add(authAccount);
        await dbContext.SaveChangesAsync();

        var loaded = await repository.GetByIdAsync(authAccount.Id);

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.LoginMethods, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task AdminAuthAccountRepository_GetByIdAsync_GivenAccountWithLoginMethod_WhenCalled_ThenEagerLoadsLoginMethods()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChoicePieDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IAdminAuthAccountRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();

        var adminUser = AdminUser.Create("Admin Owner", AdminRole.Admin);
        var adminAuthAccount = AdminAuthAccount.Create(
            Email.Create($"{Guid.NewGuid()}@example.com"), passwordHasher.Hash("Password123!"), adminUser.Id);
        dbContext.Add(adminUser);
        dbContext.Add(adminAuthAccount);
        await dbContext.SaveChangesAsync();

        var loaded = await repository.GetByIdAsync(adminAuthAccount.Id);

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.LoginMethods, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task MemberRepository_GetByIdAsync_GivenExistingMember_WhenCalled_ThenReturnsMember()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChoicePieDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IMemberRepository>();

        var member = Member.Create("Repo Member");
        dbContext.Add(member);
        await dbContext.SaveChangesAsync();

        var loaded = await repository.GetByIdAsync(member.Id);

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.Name, Is.EqualTo("Repo Member"));
    }

    [Test]
    public async Task AdminUserRepository_GetByIdAsync_GivenExistingAdminUser_WhenCalled_ThenReturnsAdminUser()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChoicePieDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IAdminUserRepository>();

        var adminUser = AdminUser.Create("Repo Admin", AdminRole.Staff);
        dbContext.Add(adminUser);
        await dbContext.SaveChangesAsync();

        var loaded = await repository.GetByIdAsync(adminUser.Id);

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.Role, Is.EqualTo(AdminRole.Staff));
    }

    [Test]
    public async Task RefreshTokenRepository_GetByIdAsync_GivenIssuedToken_WhenCalled_ThenReturnsToken()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChoicePieDbContext>();
        var repository = scope.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();

        var member = Member.Create("Token Owner");
        var token = RefreshTokenAggregate.Issue(member.Id, RefreshTokenOwnerType.Member, "hashed-token-value", DateTime.UtcNow);
        dbContext.Add(member);
        dbContext.Add(token);
        await dbContext.SaveChangesAsync();

        var loaded = await repository.GetByIdAsync(token.Id);

        Assert.That(loaded, Is.Not.Null);
        Assert.That(loaded!.TokenHash, Is.EqualTo("hashed-token-value"));
        Assert.That(loaded.IsActive, Is.True);
    }

    [Test]
    public async Task ReadRepository_Query_GivenPersistedQuizzes_WhenFiltered_ThenReturnsMatchingRowsProjectedFromDatabase()
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ChoicePieDbContext>();
        var readRepository = scope.ServiceProvider.GetRequiredService<IReadRepository>();

        var member = Member.Create("Read Repo Owner");
        var uniqueTitle = $"ReadRepo-{Guid.NewGuid()}";
        var quiz = Quiz.Create(member.Id, uniqueTitle, null, "🎯", "grad", Difficulty.Beginner, ["test"]);
        dbContext.Add(member);
        dbContext.Add(quiz);
        await dbContext.SaveChangesAsync();

        var titles = readRepository.Query<Quiz>().Where(q => q.Title == uniqueTitle).Select(q => q.Title).ToList();

        Assert.That(titles, Has.Count.EqualTo(1));
        Assert.That(titles[0], Is.EqualTo(uniqueTitle));
    }
}
