using System.ComponentModel.DataAnnotations;
using ChoicePie.Backend.Shared.Application.Behaviors;
using ChoicePie.Backend.Shared.Application.Exceptions;
using MediatR;

namespace ChoicePie.Backend.Shared.Application.Tests.Behaviors;

[TestFixture]
public class ValidationBehaviourTests
{
    private sealed class TestCommand : IRequest<string>
    {
        [Required(ErrorMessage = "Name is required")] public string? Name { get; set; }

        [StringLength(5, ErrorMessage = "Code too long")]
        public string? Code { get; set; }
    }

    private readonly ValidationBehaviour<TestCommand, string> _sut = new();

    [Test]
    public async Task Handle_GivenValidRequest_WhenCalled_ThenInvokesNext()
    {
        var request = new TestCommand { Name = "ok" };

        var result = await _sut.Handle(request, _ => Task.FromResult("handled"), CancellationToken.None);

        Assert.That(result, Is.EqualTo("handled"));
    }

    [Test]
    public void Handle_GivenRequestMissingRequiredField_WhenCalled_ThenThrowsWithFieldError()
    {
        var request = new TestCommand { Name = null };

        var ex = Assert.ThrowsAsync<AggregateValidationException>(() =>
            _sut.Handle(request, _ => Task.FromResult("handled"), CancellationToken.None));

        Assert.That(ex!.Errors, Contains.Key(nameof(TestCommand.Name)));
    }

    [Test]
    public void Handle_GivenRequestWithMultipleFailures_WhenCalled_ThenThrowsWithAllFieldErrors()
    {
        var request = new TestCommand { Name = null, Code = "way-too-long" };

        var ex = Assert.ThrowsAsync<AggregateValidationException>(() =>
            _sut.Handle(request, _ => Task.FromResult("handled"), CancellationToken.None));

        Assert.That(ex!.Errors.Keys, Is.EquivalentTo(new[] { nameof(TestCommand.Name), nameof(TestCommand.Code) }));
    }
}
