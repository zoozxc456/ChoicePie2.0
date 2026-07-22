using ChoicePie.Backend.Shared.Kernel.Primitives;

namespace ChoicePie.Backend.Shared.Kernel.Tests.Primitives;

[TestFixture]
public class SpecificationTests
{
    private sealed record Widget(string Name);

    private sealed class WidgetByNameSpecification(string name) : Specification<Widget>(widget => widget.Name == name);

    [Test]
    public void ToExpression_GivenMatchingEntity_WhenCompiledAndInvoked_ThenReturnsTrue()
    {
        var specification = new WidgetByNameSpecification("gadget");

        var isMatch = specification.ToExpression().Compile()(new Widget("gadget"));

        Assert.That(isMatch, Is.True);
    }

    [Test]
    public void ToExpression_GivenNonMatchingEntity_WhenCompiledAndInvoked_ThenReturnsFalse()
    {
        var specification = new WidgetByNameSpecification("gadget");

        var isMatch = specification.ToExpression().Compile()(new Widget("widget"));

        Assert.That(isMatch, Is.False);
    }

    [Test]
    public void ToExpression_GivenSequenceOfEntities_WhenFiltered_ThenReturnsOnlyMatches()
    {
        var specification = new WidgetByNameSpecification("gadget");
        var widgets = new[] { new Widget("gadget"), new Widget("widget") };

        var matches = widgets.AsQueryable().Where(specification.ToExpression()).ToList();

        Assert.That(matches, Is.EquivalentTo(new[] { new Widget("gadget") }));
    }
}
