using ChoicePie.Backend.Domain.Aggregates.GameRoom.Enums;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.GameRoom.Enums;

[TestFixture]
public class GamePhaseTests
{
    [Test]
    public void Enumerations_WhenRead_ThenContainsExactlyFourPhases()
    {
        Assert.That(GamePhase.Enumerations.Values, Is.EquivalentTo(new[]
        {
            GamePhase.Lobby, GamePhase.Question, GamePhase.Reveal, GamePhase.Ended
        }));
    }
}
