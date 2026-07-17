using ChoicePie.Backend.Domain.Aggregates.GameRoom.ValueObjects;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.GameRoom.ValueObjects;

[TestFixture]
public class GameQuestionSnapshotTests
{
    [Test]
    public void Equals_GivenSameQuestionIdAndSameOptionsListInstance_WhenCompared_ThenAreEqual()
    {
        var questionId = Guid.NewGuid();
        var options = new List<string> { "1", "2", "3", "4" };

        var a = new GameQuestionSnapshot(questionId, "1+1=?", options, 1, "e");
        var b = new GameQuestionSnapshot(questionId, "1+1=?", options, 1, "e");

        Assert.That(a, Is.EqualTo(b));
    }

    [Test]
    public void Equals_GivenSameScalarFieldsButDifferentOptionsListInstancesWithEqualContent_WhenCompared_ThenAreNotEqual()
    {
        // record 的預設結構相等是逐屬性比較，IReadOnlyList<string> 這種集合型別屬性不會被展開比較內容，
        // 而是用 EqualityComparer<T>.Default（對參考型別等同參考相等）——所以即使兩份 Options 內容完全
        // 一樣，只要是不同的 List 實例，這兩個 GameQuestionSnapshot 就不會被判定相等。這是 C# record +
        // 集合屬性的已知限制，這裡把目前的真實行為釘住，避免之後誤以為它是「值相等」而依賴這個假設。
        var questionId = Guid.NewGuid();

        var a = new GameQuestionSnapshot(questionId, "1+1=?", new List<string> { "1", "2", "3", "4" }, 1, "e");
        var b = new GameQuestionSnapshot(questionId, "1+1=?", new List<string> { "1", "2", "3", "4" }, 1, "e");

        Assert.That(a, Is.Not.EqualTo(b));
    }

    [Test]
    public void Equals_GivenDifferentQuestionId_WhenCompared_ThenAreNotEqual()
    {
        var options = new List<string> { "1", "2", "3", "4" };

        var a = new GameQuestionSnapshot(Guid.NewGuid(), "1+1=?", options, 1, "e");
        var b = new GameQuestionSnapshot(Guid.NewGuid(), "1+1=?", options, 1, "e");

        Assert.That(a, Is.Not.EqualTo(b));
    }
}
