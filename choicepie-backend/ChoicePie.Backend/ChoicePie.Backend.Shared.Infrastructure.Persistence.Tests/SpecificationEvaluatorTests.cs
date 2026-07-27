using ChoicePie.Backend.Shared.Infrastructure.Persistence.Specifications;
using ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests.TestSupport;

namespace ChoicePie.Backend.Shared.Infrastructure.Persistence.Tests;

[TestFixture]
public class SpecificationEvaluatorTests
{
    [Test]
    public void Apply_GivenSpecification_WhenCalled_ThenFiltersQueryableUsingSpecificationExpression()
    {
        var widgets = new[] { TestWidget.Create("Alpha"), TestWidget.Create("Beta"), TestWidget.Create("Alphabet") }
            .AsQueryable();
        var spec = new TestWidgetNameStartsWithSpecification("Alpha");

        var result = SpecificationEvaluator.Apply(widgets, spec).ToList();

        Assert.That(result, Has.Count.EqualTo(2));
        Assert.That(result.Select(w => w.Name), Is.EquivalentTo(new[] { "Alpha", "Alphabet" }));
    }

    [Test]
    public void Apply_GivenSpecificationMatchingNothing_WhenCalled_ThenReturnsEmpty()
    {
        var widgets = new[] { TestWidget.Create("Alpha") }.AsQueryable();
        var spec = new TestWidgetNameStartsWithSpecification("Zeta");

        var result = SpecificationEvaluator.Apply(widgets, spec).ToList();

        Assert.That(result, Is.Empty);
    }
}
