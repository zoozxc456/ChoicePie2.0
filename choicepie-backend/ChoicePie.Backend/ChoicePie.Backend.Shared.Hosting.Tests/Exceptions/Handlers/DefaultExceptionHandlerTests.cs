using System.Text.Json;
using ChoicePie.Backend.Shared.Hosting.API.Response;
using ChoicePie.Backend.Shared.Hosting.Exceptions.Handlers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace ChoicePie.Backend.Shared.Hosting.Tests.Exceptions.Handlers;

[TestFixture]
public class DefaultExceptionHandlerTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private IHostEnvironment _env = null!;
    private DefaultHttpContext _httpContext = null!;
    private MemoryStream _responseBody = null!;

    [SetUp]
    public void SetUp()
    {
        _env = Substitute.For<IHostEnvironment>();
        _responseBody = new MemoryStream();
        _httpContext = new DefaultHttpContext { Response = { Body = _responseBody } };
    }

    private DefaultExceptionHandler CreateSut() =>
        new(Substitute.For<ILogger<DefaultExceptionHandler>>(), _env);

    [Test]
    public async Task TryHandleAsync_GivenAnyException_WhenCalled_ThenSetsStatusCode500AndReturnsTrue()
    {
        _env.EnvironmentName.Returns(Environments.Production);
        var sut = CreateSut();

        var handled = await sut.TryHandleAsync(_httpContext, new InvalidOperationException("boom"), CancellationToken.None);

        Assert.That(handled, Is.True);
        Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
    }

    [Test]
    public async Task TryHandleAsync_GivenProductionEnvironment_WhenCalled_ThenReturnsGenericMessageNotExceptionDetails()
    {
        _env.EnvironmentName.Returns(Environments.Production);
        var sut = CreateSut();

        await sut.TryHandleAsync(_httpContext, new InvalidOperationException("sensitive stack trace detail"), CancellationToken.None);

        var response = await ReadResponseAsync();
        Assert.That(response.Message, Does.Not.Contain("sensitive stack trace detail"));
    }

    [Test]
    public async Task TryHandleAsync_GivenDevelopmentEnvironment_WhenCalled_ThenReturnsExceptionDetailsForDebugging()
    {
        _env.EnvironmentName.Returns(Environments.Development);
        var sut = CreateSut();

        await sut.TryHandleAsync(_httpContext, new InvalidOperationException("sensitive stack trace detail"), CancellationToken.None);

        var response = await ReadResponseAsync();
        Assert.That(response.Message, Does.Contain("sensitive stack trace detail"));
    }

    private async Task<ApiErrorResponse> ReadResponseAsync()
    {
        _responseBody.Seek(0, SeekOrigin.Begin);
        var response = await JsonSerializer.DeserializeAsync<ApiErrorResponse>(_responseBody, JsonOptions);
        Assert.That(response, Is.Not.Null);
        return response!;
    }
}
