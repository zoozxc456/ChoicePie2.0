using ChoicePie.Backend.Domain.Aggregates.Quiz.ValueObjects;

namespace ChoicePie.Backend.Domain.Tests.Aggregates.Quiz.ValueObjects;

[TestFixture]
public class TagListTests
{
    [Test]
    public void Create_GivenCaseVariantDuplicates_WhenCalled_ThenDeduplicatesCaseInsensitively()
    {
        var tagList = TagList.Create(["Go", "go", "Go"]);

        Assert.That(tagList.Values, Has.Count.EqualTo(1));
    }

    [Test]
    public void Create_GivenDistinctTags_WhenCalled_ThenKeepsAllOfThem()
    {
        var tagList = TagList.Create(["Go", "Kubernetes"]);

        Assert.That(tagList.Values, Is.EqualTo(new[] { "Go", "Kubernetes" }));
    }
}
