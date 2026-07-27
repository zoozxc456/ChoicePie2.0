using ChoicePie.Backend.Domain.Aggregates.GameRoom.Entities;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.GameRoom.Entities;

[TestFixture]
public class PlayerTests
{
    [Test]
    public void Create_GivenNicknameAndConnectionId_WhenCalled_ThenFieldsAreSet()
    {
        var player = Player.Create("小明", "connection-1");

        Assert.Multiple(() =>
        {
            Assert.That(player.Nickname, Is.EqualTo("小明"));
            Assert.That(player.ConnectionId, Is.EqualTo("connection-1"));
            Assert.That(player.Score, Is.EqualTo(0));
            Assert.That(player.SelectedOptionIndex, Is.Null);
        });
    }

    [Test]
    public void Create_GivenNoMemberId_WhenCalled_ThenMemberIdIsNull()
    {
        var player = Player.Create("小明", "connection-1");

        Assert.That(player.MemberId, Is.Null);
    }

    [Test]
    public void Create_GivenMemberId_WhenCalled_ThenMemberIdIsSet()
    {
        var memberId = Guid.NewGuid();

        var player = Player.Create("小明", "connection-1", memberId);

        Assert.That(player.MemberId, Is.EqualTo(memberId));
    }

    [Test]
    public void HasAnsweredQuestion_GivenNoAnswerRecordedYet_WhenCalled_ThenReturnsFalse()
    {
        var player = Player.Create("小明", "connection-1");

        Assert.That(player.HasAnsweredQuestion(0), Is.False);
    }

    [Test]
    public void RecordAnswer_GivenCorrectAnswer_WhenCalled_ThenScoreAccumulatesAndSelectionAndFlagAreSet()
    {
        var player = Player.Create("小明", "connection-1");

        player.RecordAnswer(questionIndex: 0, answerIndex: 2, score: 1000, isCorrect: true);

        Assert.Multiple(() =>
        {
            Assert.That(player.Score, Is.EqualTo(1000));
            Assert.That(player.SelectedOptionIndex, Is.EqualTo(2));
            Assert.That(player.HasAnsweredQuestion(0), Is.True);
            Assert.That(player.HasAnsweredQuestion(1), Is.False);
        });
    }

    [Test]
    public void RecordAnswer_GivenTwoQuestionsAnswered_WhenCalled_ThenScoreAccumulatesAcrossQuestions()
    {
        var player = Player.Create("小明", "connection-1");

        player.RecordAnswer(questionIndex: 0, answerIndex: 2, score: 1000, isCorrect: true);
        player.RecordAnswer(questionIndex: 1, answerIndex: 0, score: 800, isCorrect: true);

        Assert.That(player.Score, Is.EqualTo(1800));
    }

    [Test]
    public void Answers_GivenTwoQuestionsAnswered_WhenRead_ThenContainsBothEntriesKeyedByQuestionIndex()
    {
        var player = Player.Create("小明", "connection-1");
        player.RecordAnswer(questionIndex: 0, answerIndex: 2, score: 1000, isCorrect: true);
        player.RecordAnswer(questionIndex: 1, answerIndex: 0, score: 0, isCorrect: false);

        Assert.Multiple(() =>
        {
            Assert.That(player.Answers[0], Is.EqualTo(new PlayerAnswer(2, 1000, true)));
            Assert.That(player.Answers[1], Is.EqualTo(new PlayerAnswer(0, 0, false)));
        });
    }

    [Test]
    public void ResetForNextQuestion_GivenPreviousSelection_WhenCalled_ThenClearsSelectedOptionIndex()
    {
        var player = Player.Create("小明", "connection-1");
        player.RecordAnswer(questionIndex: 0, answerIndex: 2, score: 1000, isCorrect: true);

        player.ResetForNextQuestion();

        Assert.Multiple(() =>
        {
            Assert.That(player.SelectedOptionIndex, Is.Null);
            Assert.That(player.HasAnsweredQuestion(0), Is.True, "past answers must still be recorded after reset");
        });
    }
}
